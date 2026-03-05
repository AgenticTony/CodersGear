using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Utility;
using IPrintifyService = CodersGear.Utility.IPrintifyService;

namespace CodersGear.Services
{
    public interface IPrintifyOrderService
    {
        Task<bool> SendOrderToPrintifyAsync(int orderId);
    }

    public class PrintifyOrderService : IPrintifyOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrintifyService _printifyService;
        private readonly Utility.PrintifySettings _settings;
        private readonly ILogger<PrintifyOrderService> _logger;

        public PrintifyOrderService(
            IUnitOfWork unitOfWork,
            IPrintifyService printifyService,
            IOptions<Utility.PrintifySettings> settings,
            ILogger<PrintifyOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _printifyService = printifyService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendOrderToPrintifyAsync(int orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.ShopId))
                {
                    _logger.LogWarning($"Printify ShopId is not configured. Cannot send order {orderId} to Printify.");
                    return false;
                }

                var orderHeader = _unitOfWork.OrderHeader.Get(
                    o => o.Id == orderId,
                    includeProperties: "ApplicationUser"
                );

                if (orderHeader == null)
                {
                    _logger.LogWarning($"Order {orderId} not found.");
                    return false;
                }

                // Check if order was already sent to Printify
                if (orderHeader.SentToPrintify)
                {
                    _logger.LogInformation($"Order {orderId} was already sent to Printify.");
                    return true;
                }

                // Get order details
                var orderDetails = _unitOfWork.OrderDetail.GetAll(d => d.OrderHeaderId == orderId).ToList();

                // Check if order contains any Printify products
                var printifyOrderDetails = orderDetails.Where(d =>
                {
                    var product = _unitOfWork.Product.Get(p => p.ProductId == d.ProductId);
                    return product != null && product.IsPrintifyProduct;
                }).ToList();

                if (!printifyOrderDetails.Any())
                {
                    _logger.LogInformation($"Order {orderId} does not contain any Printify products. Skipping Printify submission.");
                    return false;
                }

                // Build Printify order request
                var printifyOrderRequest = new Utility.PrintifyOrderRequest
                {
                    ExternalId = orderId.ToString(),
                    Label = $"CodersGear Order #{orderId}",
                    LineItems = new List<Utility.PrintifyLineItem>(),
                    ShippingMethod = Utility.PrintifyConstants.SHIPPING_STANDARD,
                    AddressTo = new Utility.PrintifyAddress
                    {
                        FirstName = orderHeader.Name.Split(' ').FirstOrDefault() ?? string.Empty,
                        LastName = string.Join(" ", orderHeader.Name.Split(' ').Skip(1)),
                        Email = orderHeader.ApplicationUser?.Email ?? string.Empty,
                        Phone = orderHeader.PhoneNumber,
                        Country = orderHeader.Country,
                        Region = orderHeader.State,
                        Address1 = orderHeader.StreetAddress,
                        City = orderHeader.City,
                        Zip = orderHeader.PostalCode
                    }
                };

                // Add line items for Printify products
                foreach (var orderDetail in printifyOrderDetails)
                {
                    var product = _unitOfWork.Product.Get(p => p.ProductId == orderDetail.ProductId);

                    if (product != null && !string.IsNullOrEmpty(product.PrintifyProductId))
                    {
                        // Use the variant ID stored in the order detail (from cart)
                        // This is critical - we must use the variant the customer actually selected!
                        int? variantId = null;

                        // Try to parse the stored variant ID
                        if (!string.IsNullOrEmpty(orderDetail.PrintifyVariantId) && int.TryParse(orderDetail.PrintifyVariantId, out int parsedId))
                        {
                            variantId = parsedId;
                        }

                        // Fallback: if no variant ID stored, try to find matching variant by size/color
                        if (variantId == null && !string.IsNullOrEmpty(product.PrintifyVariantData))
                        {
                            var variants = JsonSerializer.Deserialize<List<Utility.PrintifyVariant>>(product.PrintifyVariantData);

                            // Try to match by size and color (parse from Title which is like "Small / Black")
                            if (!string.IsNullOrEmpty(orderDetail.Size) || !string.IsNullOrEmpty(orderDetail.Color))
                            {
                                var matchingVariant = variants?.FirstOrDefault(v =>
                                {
                                    if (!v.IsEnabled) return false;

                                    // Parse size and color from title (format: "Size / Color" or similar)
                                    var parts = v.Title.Split('/', StringSplitOptions.TrimEntries);
                                    var variantSize = parts.Length > 0 ? parts[0] : "";
                                    var variantColor = parts.Length > 1 ? parts[1] : "";

                                    bool sizeMatches = string.IsNullOrEmpty(orderDetail.Size) ||
                                                       variantSize.Equals(orderDetail.Size, StringComparison.OrdinalIgnoreCase);
                                    bool colorMatches = string.IsNullOrEmpty(orderDetail.Color) ||
                                                        variantColor.Equals(orderDetail.Color, StringComparison.OrdinalIgnoreCase);

                                    return sizeMatches && colorMatches;
                                });

                                variantId = matchingVariant?.Id;
                            }

                            // Last resort: use default or first variant
                            if (variantId == null)
                            {
                                var fallbackVariant = variants?.FirstOrDefault(v => v.IsDefault) ?? variants?.FirstOrDefault(v => v.IsEnabled);
                                variantId = fallbackVariant?.Id;
                            }
                        }

                        if (variantId.HasValue)
                        {
                            printifyOrderRequest.LineItems.Add(new Utility.PrintifyLineItem
                            {
                                ProductId = product.PrintifyProductId ?? string.Empty,
                                VariantId = variantId.Value,
                                Quantity = orderDetail.Count,
                                ExternalId = orderDetail.Id.ToString()
                            });
                        }
                    }
                }

                if (!printifyOrderRequest.LineItems.Any())
                {
                    _logger.LogWarning($"No valid Printify line items could be created for order {orderId}.");
                    return false;
                }

                // Send order to Printify
                _logger.LogInformation($"Sending order {orderId} to Printify...");
                var printifyResponse = await _printifyService.CreateOrderAsync(_settings.ShopId, printifyOrderRequest);

                // Update order with Printify Order ID
                orderHeader.PrintifyOrderId = printifyResponse.Id;
                orderHeader.SentToPrintify = true;
                orderHeader.SentToPrintifyAt = DateTime.UtcNow;
                orderHeader.OrderStatus = SD.Status_InProcess;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                _logger.LogInformation($"Order {orderId} successfully sent to Printify. Printify Order ID: {printifyResponse.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending order {orderId} to Printify: {ex.Message}");
                return false;
            }
        }
    }
}
