using System.Text;
using System.Text.Json;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodersGear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintifyWebhookController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PrintifySettings _printifySettings;
        private readonly ILogger<PrintifyWebhookController> _logger;
        private readonly IWebhookSignatureVerifier _signatureVerifier;

        public PrintifyWebhookController(
            IUnitOfWork unitOfWork,
            IOptions<PrintifySettings> printifySettings,
            ILogger<PrintifyWebhookController> logger,
            IWebhookSignatureVerifier signatureVerifier)
        {
            _unitOfWork = unitOfWork;
            _printifySettings = printifySettings.Value;
            _logger = logger;
            _signatureVerifier = signatureVerifier;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                // Verify webhook signature if secret is configured
                if (!string.IsNullOrEmpty(_printifySettings.WebhookSecret))
                {
                    // Printify sends the signature in the X-Pfy-Signature header
                    if (!Request.Headers.TryGetValue("X-Pfy-Signature", out var signatureHeader))
                    {
                        _logger.LogWarning("Printify webhook missing X-Pfy-Signature header");
                        return Unauthorized();
                    }

                    if (!_signatureVerifier.VerifyPrintifySignature(json, signatureHeader!, _printifySettings.WebhookSecret))
                    {
                        _logger.LogWarning("Printify webhook signature verification failed");
                        return Unauthorized();
                    }
                }

                var webhookEvent = JsonSerializer.Deserialize<PrintifyWebhookEvent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize Printify webhook event");
                    return BadRequest();
                }

                _logger.LogInformation($"Received Printify webhook: {webhookEvent.Type} for resource {webhookEvent.Resource?.Id}");

                switch (webhookEvent.Type)
                {
                    case "order:updated":
                        await HandleOrderUpdated(webhookEvent);
                        break;

                    case "order:shipment:created":
                        await HandleShipmentCreated(webhookEvent);
                        break;

                    case "product:publishing_succeeded":
                        await HandleProductPublishingSucceeded(webhookEvent);
                        break;

                    case "product:publishing_failed":
                        await HandleProductPublishingFailed(webhookEvent);
                        break;

                    case "product:unpublished":
                        await HandleProductUnpublished(webhookEvent);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled Printify webhook type: {webhookEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (JsonException e)
            {
                _logger.LogError($"JSON parsing error: {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"General error: {e.Message}");
                return StatusCode(500);
            }
        }

        private async Task HandleOrderUpdated(PrintifyWebhookEvent webhookEvent)
        {
            if (webhookEvent.Resource?.Data.ValueKind == JsonValueKind.Object)
            {
                var orderData = JsonSerializer.Deserialize<PrintifyOrderUpdatedData>(webhookEvent.Resource.Data.GetRawText());

                if (orderData != null)
                {
                    _logger.LogInformation($"Order {webhookEvent.Resource.Id} updated to status: {orderData.Status}");

                    // Find the local order by Printify Order ID
                    var orderHeader = _unitOfWork.OrderHeader.Get(o => o.PrintifyOrderId == webhookEvent.Resource.Id);

                    if (orderHeader != null)
                    {
                        // Map Printify status to CodersGear status
                        string newStatus = MapPrintifyStatusToOrderStatus(orderData.Status);

                        if (!string.IsNullOrEmpty(newStatus))
                        {
                            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, newStatus, orderHeader.PaymentStatus);
                            _unitOfWork.Save();

                            _logger.LogInformation($"Local order {orderHeader.Id} updated to status: {newStatus}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No local order found for Printify Order ID: {webhookEvent.Resource.Id}");
                    }
                }
            }
        }

        private async Task HandleShipmentCreated(PrintifyWebhookEvent webhookEvent)
        {
            if (webhookEvent.Resource?.Data.ValueKind == JsonValueKind.Object)
            {
                var shipmentData = JsonSerializer.Deserialize<PrintifyShipmentData>(webhookEvent.Resource.Data.GetRawText());

                if (shipmentData != null)
                {
                    _logger.LogInformation($"Shipment created for Printify order: {webhookEvent.Resource.Id}");

                    // Find the local order by Printify Order ID
                    var orderHeader = _unitOfWork.OrderHeader.Get(o => o.PrintifyOrderId == webhookEvent.Resource.Id);

                    if (orderHeader != null)
                    {
                        // Update order with tracking information
                        orderHeader.Carrier = shipmentData.Carrier?.Code;
                        orderHeader.TrackingNumber = shipmentData.Carrier?.TrackingNumber;
                        orderHeader.OrderStatus = SD.Status_Shipped;
                        if (!string.IsNullOrEmpty(shipmentData.ShippedAt) && DateTime.TryParse(shipmentData.ShippedAt, out var shippedDate))
                        {
                            orderHeader.ShippingDate = shippedDate;
                        }

                        _unitOfWork.OrderHeader.Update(orderHeader);
                        _unitOfWork.Save();

                        _logger.LogInformation($"Local order {orderHeader.Id} marked as shipped. Tracking: {shipmentData.Carrier?.TrackingNumber}");
                    }
                    else
                    {
                        _logger.LogWarning($"No local order found for Printify Order ID: {webhookEvent.Resource.Id}");
                    }
                }
            }
        }

        private string MapPrintifyStatusToOrderStatus(string printifyStatus)
        {
            return printifyStatus switch
            {
                PrintifyConstants.ORDER_STATUS_PENDING => SD.Status_Pending,
                PrintifyConstants.ORDER_STATUS_ON_HOLD => SD.Status_Pending,
                PrintifyConstants.ORDER_STATUS_SENDING_TO_PRODUCTION => SD.Status_InProcess,
                PrintifyConstants.ORDER_STATUS_IN_PRODUCTION => SD.Status_InProcess,
                PrintifyConstants.ORDER_STATUS_FULFILLED => SD.Status_Shipped,
                PrintifyConstants.ORDER_STATUS_PARTIALLY_FULFILLED => SD.Status_Shipped,
                PrintifyConstants.ORDER_STATUS_CANCELED => SD.Status_Cancelled,
                PrintifyConstants.ORDER_STATUS_HAS_ISSUES => SD.Status_InProcess,
                _ => SD.Status_Pending
            };
        }

        private async Task HandleProductPublishingSucceeded(PrintifyWebhookEvent webhookEvent)
        {
            _logger.LogInformation($"Product publishing succeeded: {webhookEvent.Resource?.Id}");

            // Find the local product by Printify Product ID
            var product = _unitOfWork.Product.Get(p => p.PrintifyProductId == webhookEvent.Resource.Id);

            if (product != null)
            {
                product.IsPrintifyProduct = true;
                product.Visible = true; // Make the product visible
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                _logger.LogInformation($"Product {product.ProductId} (Printify ID: {webhookEvent.Resource.Id}) is now published and visible");
            }
            else
            {
                _logger.LogWarning($"Product not found for Printify ID: {webhookEvent.Resource.Id}. Consider syncing this product.");
            }
        }

        private async Task HandleProductPublishingFailed(PrintifyWebhookEvent webhookEvent)
        {
            _logger.LogWarning($"Product publishing failed: {webhookEvent.Resource?.Id}");

            // Find the local product by Printify Product ID
            var product = _unitOfWork.Product.Get(p => p.PrintifyProductId == webhookEvent.Resource.Id);

            if (product != null)
            {
                product.Visible = false; // Hide the product since publishing failed
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                _logger.LogInformation($"Product {product.ProductId} has been hidden due to publishing failure.");
            }
        }

        private async Task HandleProductUnpublished(PrintifyWebhookEvent webhookEvent)
        {
            _logger.LogInformation($"Product unpublished: {webhookEvent.Resource?.Id}");

            // Find the local product by Printify Product ID
            var product = _unitOfWork.Product.Get(p => p.PrintifyProductId == webhookEvent.Resource.Id);

            if (product != null)
            {
                product.Visible = false; // Hide the unpublished product
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                _logger.LogInformation($"Product {product.ProductId} (Printify ID: {webhookEvent.Resource.Id}) has been unpublished and hidden.");
            }
        }
    }
}
