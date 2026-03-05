using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using CodersGear.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace CodersGear.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private const string SessionKeyId = "GuestCartId";

        private string GetOrSetSessionId()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Logged in users don't use session-based cart
                return null;
            }

            string sessionId = HttpContext.Session.GetString(SessionKeyId);
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(SessionKeyId, sessionId);
            }
            return sessionId;
        }

        private string GetUserId()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            IEnumerable<ShoppingCart> cartList;

            if (userId != null)
            {
                // Logged in user - get by user ID
                cartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product,Product.Category");
            }
            else
            {
                // Guest user - get by session ID
                cartList = _unitOfWork.ShoppingCart.GetBySessionId(sessionId);
            }

            ShoppingCartVM = new()
            {
                ShoppingCartList = cartList,
                OrderHeader = new()
            };

            // Calculate order total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Increment(int id)
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            // Get cart item with ownership verification
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id &&
                (userId != null ? u.ApplicationUserId == userId : u.SessionId == sessionId));

            if (cartFromDb != null)
            {
                cartFromDb.Count += 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Decrement(int id)
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            // Get cart item with ownership verification
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id &&
                (userId != null ? u.ApplicationUserId == userId : u.SessionId == sessionId));

            if (cartFromDb != null && cartFromDb.Count > 1)
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            // Get cart item with ownership verification
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id &&
                (userId != null ? u.ApplicationUserId == userId : u.SessionId == sessionId));

            if (cartFromDb != null)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Item removed from cart";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Summary()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity", returnUrl = "/Customer/Cart/Summary" });
            }

            var sessionId = HttpContext.Session.GetString(SessionKeyId);

            // Get shopping cart items - merge session cart with user cart if session exists
            IEnumerable<ShoppingCart> cartList;

            if (!string.IsNullOrEmpty(sessionId))
            {
                // Merge session cart with user cart
                _unitOfWork.ShoppingCart.MergeSessionCartToUserCart(sessionId, userId);
                // Clear session ID after merge
                HttpContext.Session.Remove(SessionKeyId);
            }

            // Get user's cart
            cartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.Category");

            ShoppingCartVM = new()
            {
                ShoppingCartList = cartList,
                OrderHeader = new()
            };

            // Get user information and populate OrderHeader
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (applicationUser != null)
            {
                ShoppingCartVM.OrderHeader.Name = applicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = applicationUser.City;
                ShoppingCartVM.OrderHeader.State = applicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;
                ShoppingCartVM.OrderHeader.Country = applicationUser.Country;
            }

            // Calculate order total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Populate ShoppingCartList from database
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.Category");

            // Get ApplicationUser
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            // Clear any existing model state errors
            ModelState.Clear();

            // Populate from ApplicationUser if fields are empty or null
            ShoppingCartVM.OrderHeader.Name ??= applicationUser?.Name ?? "";
            ShoppingCartVM.OrderHeader.PhoneNumber ??= applicationUser?.PhoneNumber ?? "";
            ShoppingCartVM.OrderHeader.StreetAddress ??= applicationUser?.StreetAddress ?? "";
            ShoppingCartVM.OrderHeader.City ??= applicationUser?.City ?? "";
            ShoppingCartVM.OrderHeader.State ??= applicationUser?.State ?? "";
            ShoppingCartVM.OrderHeader.PostalCode ??= applicationUser?.PostalCode ?? "";
            ShoppingCartVM.OrderHeader.Country ??= applicationUser?.Country ?? "";

            // Set OrderDate and ApplicationUserId
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            // Calculate order total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            // Check if it's a company account or regular customer
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Regular customer - payment required via Stripe
                ShoppingCartVM.OrderHeader.OrderStatus = SD.Status_Pending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_Pending;
            }
            else
            {
                // Company account - 30 days delayed payment
                ShoppingCartVM.OrderHeader.OrderStatus = SD.Status_Approved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_DelayedPayment;
            }

            // Create OrderHeader
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // Create OrderDetail for each item in shopping cart
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                    // Copy variant info for Printify order submission
                    Size = cart.Size,
                    Color = cart.Color,
                    PrintifyVariantId = cart.PrintifyVariantId
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();

            // Check if it's a company account or regular customer for payment processing
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Regular customer - capture payment via Stripe

                // STRIPE BEST PRACTICE: Create or retrieve customer first
                var customerService = new CustomerService();
                string stripeCustomerId;

                // Check if user already has a Stripe customer ID
                if (!string.IsNullOrEmpty(applicationUser.StripeCustomerId))
                {
                    stripeCustomerId = applicationUser.StripeCustomerId;
                }
                else
                {
                    // Create new customer in Stripe
                    var customerOptions = new CustomerCreateOptions
                    {
                        Email = applicationUser.Email,
                        Name = applicationUser.Name,
                        Phone = applicationUser.PhoneNumber,
                        Metadata = new Dictionary<string, string>
                        {
                            { "application_user_id", applicationUser.Id }
                        }
                    };
                    var customer = customerService.Create(customerOptions);
                    stripeCustomerId = customer.Id;

                    // Optionally save customer ID to ApplicationUser for future use
                    // (This would require updating ApplicationUser model and repository)
                }

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                var options = new SessionCreateOptions
                {
                    // STRIPE BEST PRACTICE: Always associate customer with session
                    Customer = stripeCustomerId,

                    // STRIPE BEST PRACTICE: Use {CHECKOUT_SESSION_ID} placeholder for reliable tracking
                    SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = domain + "customer/cart/index",

                    // STRIPE BEST PRACTICE: Add metadata for tracking and reconciliation
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", ShoppingCartVM.OrderHeader.Id.ToString() },
                        { "user_id", userId }
                    },

                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                // Add line items for each product in shopping cart
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Stripe requires amount in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.ProductName,
                                Description = item.Product.Description
                            },
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                // Create Stripe checkout session
                var service = new SessionService();
                Session session = service.Create(options);

                // Update OrderHeader with Stripe session info
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId, stripeCustomerId);
                _unitOfWork.Save();

                // Redirect to Stripe checkout using HTTP 303 (See Other)
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                // Company account - redirect to order confirmation page
                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
            }
        }

        [Authorize]
        public IActionResult OrderConfirmation(int id)
        {
            // Get order from database
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id);

            if (orderHeader == null)
            {
                return View("Error", new ErrorViewModel { RequestId = id.ToString() });
            }

            // Verify payment if this was a Stripe checkout (not company account)
            if (orderHeader.PaymentStatus == SD.PaymentStatus_DelayedPayment)
            {
                // Company account - already approved, show confirmation
                return View("OrderConfirmation", orderHeader);
            }
            else
            {
                // Regular customer - verify payment with Stripe
                var sessionService = new SessionService();

                try
                {
                    if (!string.IsNullOrEmpty(orderHeader.SessionId))
                    {
                        Session session = sessionService.Get(orderHeader.SessionId);

                        // Verify payment status from Stripe
                        if (session.PaymentStatus == "paid")
                        {
                            // Update order status if not already updated by webhook
                            if (orderHeader.PaymentStatus != SD.PaymentStatus_Approved)
                            {
                                // Update PaymentIntentId if available
                                if (!string.IsNullOrEmpty(session.PaymentIntentId))
                                {
                                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, orderHeader.SessionId, session.PaymentIntentId);
                                }

                                _unitOfWork.OrderHeader.UpdateStatus(id, SD.Status_Approved, SD.PaymentStatus_Approved);

                                // Clear shopping cart
                                var shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(
                                    sc => sc.ApplicationUserId == orderHeader.ApplicationUserId
                                ).ToList();

                                if (shoppingCartItems.Any())
                                {
                                    _unitOfWork.ShoppingCart.RemoveRange(shoppingCartItems);
                                    _unitOfWork.Save();
                                }
                            }

                            // Reload order header to get updated data
                            orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id);
                            return View("OrderConfirmation", orderHeader);
                        }
                        else
                        {
                            // Payment not yet completed or failed
                            TempData["error"] = "Payment not completed. Please try again.";
                            return RedirectToAction("Index", "Cart");
                        }
                    }
                    else
                    {
                        // No session ID - something went wrong
                        TempData["error"] = "Order session not found. Please contact support.";
                        return View("Error", new ErrorViewModel { RequestId = id.ToString() });
                    }
                }
                catch (StripeException e)
                {
                    TempData["error"] = $"Error verifying payment: {e.Message}";
                    return View("Error", new ErrorViewModel { RequestId = id.ToString() });
                }
            }
        }

        private decimal GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
