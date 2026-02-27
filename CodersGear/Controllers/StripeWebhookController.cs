using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace CodersGear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IUnitOfWork unitOfWork,
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripeWebhookController> logger)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                // STRIPE BEST PRACTICE: Always verify webhook signature
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret
                );

                _logger.LogInformation($"Received Stripe webhook event: {stripeEvent.Type}");

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;

                    case "checkout.session.async_payment_succeeded":
                        await HandleCheckoutSessionAsyncPaymentSucceeded(stripeEvent);
                        break;

                    case "checkout.session.async_payment_failed":
                        await HandleCheckoutSessionAsyncPaymentFailed(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError($"Stripe error: {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"General error: {e.Message}");
                return StatusCode(500);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            if (session == null)
            {
                _logger.LogWarning("Session object was null in checkout.session.completed event");
                return;
            }

            // Extract order_id from metadata
            if (session.Metadata != null && session.Metadata.TryGetValue("order_id", out var orderIdStr))
            {
                if (int.TryParse(orderIdStr, out int orderId))
                {
                    var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);

                    if (orderHeader != null)
                    {
                        // STRIPE BEST PRACTICE: Verify the session hasn't already been processed
                        // Check if PaymentIntentId is already set (idempotency check)
                        if (orderHeader.PaymentStatus != SD.PaymentStatus_Approved)
                        {
                            // Update order status based on payment status from Stripe
                            _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Status_Approved, SD.PaymentStatus_Approved);

                            // Update PaymentIntentId and PaymentDate if available
                            if (!string.IsNullOrEmpty(session.PaymentIntentId))
                            {
                                _unitOfWork.OrderHeader.UpdateStripePaymentID(
                                    orderId,
                                    session.Id,
                                    session.PaymentIntentId,
                                    session.CustomerId
                                );
                            }

                            // Clear shopping cart items for this user
                            var shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(
                                sc => sc.ApplicationUserId == orderHeader.ApplicationUserId
                            ).ToList();

                            _unitOfWork.ShoppingCart.RemoveRange(shoppingCartItems);
                            _unitOfWork.Save();

                            _logger.LogInformation($"Order {orderId} marked as approved and payment complete.");
                        }
                        else
                        {
                            _logger.LogInformation($"Order {orderId} was already processed. Skipping duplicate webhook.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Order {orderId} not found in database.");
                    }
                }
            }
        }

        private async Task HandleCheckoutSessionAsyncPaymentSucceeded(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            if (session == null) return;

            _logger.LogInformation($"Async payment succeeded for session: {session.Id}");

            // Handle async payment methods like SOFORT, etc.
            // Similar logic to checkout.session.completed
            await HandleCheckoutSessionCompleted(stripeEvent);
        }

        private async Task HandleCheckoutSessionAsyncPaymentFailed(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            if (session == null) return;

            _logger.LogWarning($"Async payment failed for session: {session.Id}");

            // Extract order_id from metadata
            if (session.Metadata != null && session.Metadata.TryGetValue("order_id", out var orderIdStr))
            {
                if (int.TryParse(orderIdStr, out int orderId))
                {
                    var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);

                    if (orderHeader != null)
                    {
                        // Update order status to cancelled/failed
                        _unitOfWork.OrderHeader.UpdateStatus(
                            orderId,
                            SD.Status_Cancelled,
                            SD.PaymentStatus_Rejected
                        );
                        _unitOfWork.Save();

                        _logger.LogInformation($"Order {orderId} marked as cancelled due to failed async payment.");
                    }
                }
            }
        }
    }
}
