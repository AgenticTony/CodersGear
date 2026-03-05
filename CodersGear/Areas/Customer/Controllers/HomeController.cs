using System.Diagnostics;
using System.Security.Claims;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using CodersGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
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
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrintifyProductSyncService _printifySyncService;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IPrintifyProductSyncService printifySyncService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _printifySyncService = printifySyncService;
        }

        public IActionResult Index()
        {
            // Get all visible products with categories
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Visible);

            // Group by category
            List<CategoryProductViewModel> categoryGroups = products
                .GroupBy(p => new { CategoryId = p.CategoryId ?? 0, CategoryName = p.Category != null ? p.Category.Name : "Uncategorized", DisplayOrder = p.Category != null ? p.Category.DisplayOrder : 999 })
                .Select(g => new CategoryProductViewModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    DisplayOrder = g.Key.DisplayOrder,
                    Products = g.ToList()
                })
                .OrderBy(vm => vm.DisplayOrder)
                .ToList();

            return View(categoryGroups);
        }

        public IActionResult Details(int? productId)
        {
            if (productId == null || productId == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            // Get product (must be visible)
            ShoppingCart cart = new()
            {
                Count = 1,
                ProductId = productId.Value,
                Product = _unitOfWork.Product.Get(u => u.ProductId == productId.Value && u.Visible, includeProperties: "Category")
            };

            if (cart.Product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(cart);
        }
        [HttpPost]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            if (userId != null)
            {
                // Logged in user
                shoppingCart.ApplicationUserId = userId;
                shoppingCart.SessionId = null;

                // Check if shopping cart already exists for this user, product AND variant
                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                    u => u.ApplicationUserId == userId &&
                         u.ProductId == shoppingCart.ProductId &&
                         u.Size == shoppingCart.Size &&
                         u.Color == shoppingCart.Color,
                    tracked: false);

                if (cartFromDb != null)
                {
                    // Shopping cart already exists with same variant, update the count
                    shoppingCart.Count += cartFromDb.Count;
                    shoppingCart.Id = cartFromDb.Id;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                }
                else
                {
                    // Shopping cart doesn't exist (different variant or new product), add new entry
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
            }
            else
            {
                // Guest user - use session-based cart
                shoppingCart.SessionId = sessionId;
                shoppingCart.ApplicationUserId = null;

                // Get session cart for this product AND variant
                var existingCart = _unitOfWork.ShoppingCart.Get(
                    u => u.SessionId == sessionId &&
                         u.ProductId == shoppingCart.ProductId &&
                         u.Size == shoppingCart.Size &&
                         u.Color == shoppingCart.Color,
                    tracked: false);

                if (existingCart != null)
                {
                    // Session cart already exists with same variant, update the count
                    shoppingCart.Count += existingCart.Count;
                    shoppingCart.Id = existingCart.Id;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                }
                else
                {
                    // Session cart doesn't exist (different variant or new product), add new entry
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
            }

            _unitOfWork.Save();
            TempData["success"] = "Cart updated successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult BuyNow(int productId)
        {
            var userId = GetUserId();
            var sessionId = GetOrSetSessionId();

            ShoppingCart shoppingCart = new ShoppingCart
            {
                ProductId = productId,
                Count = 1
            };

            if (userId != null)
            {
                // Logged in user
                shoppingCart.ApplicationUserId = userId;
                shoppingCart.SessionId = null;

                // Check if shopping cart already exists for this user and product
                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                    u => u.ApplicationUserId == userId && u.ProductId == productId,
                    tracked: false);

                if (cartFromDb != null)
                {
                    // Shopping cart already exists, update the count
                    shoppingCart.Count += cartFromDb.Count;
                    shoppingCart.Id = cartFromDb.Id;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                }
                else
                {
                    // Shopping cart doesn't exist, add new entry
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
            }
            else
            {
                // Guest user - use session-based cart
                shoppingCart.SessionId = sessionId;
                shoppingCart.ApplicationUserId = null;

                // Get all session carts for this product
                var sessionCarts = _unitOfWork.ShoppingCart.GetAll(
                    u => u.SessionId == sessionId && u.ProductId == productId);

                if (sessionCarts.Any())
                {
                    // Session cart already exists, update the count
                    var existingCart = sessionCarts.First();
                    shoppingCart.Count += existingCart.Count;
                    shoppingCart.Id = existingCart.Id;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                }
                else
                {
                    // Session cart doesn't exist, add new entry
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
            }

            _unitOfWork.Save();
            TempData["success"] = "Item added to cart";

            // Redirect to Cart page (not Index)
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Hoddies()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 2 && p.Visible);
            return View(products);
        }

        public IActionResult TShirts()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 1 && p.Visible);
            return View(products);
        }

        public IActionResult Mugs()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 3 && p.Visible);
            return View(products);
        }

        public IActionResult Accessories()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 4 && p.Visible);
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Debug action to trigger Printify sync
        public async Task<string> SyncPrintify()
        {
            try
            {
                _logger.LogInformation("=== MANUAL PRINTIFY SYNC TRIGGERED ===");
                await _printifySyncService.SyncProductsAsync();

                var printifyProducts = _unitOfWork.Product.GetAll(p => p.IsPrintifyProduct == true).ToList();
                return $"SUCCESS! Synced {printifyProducts.Count} Printify products. " +
                       $"Check server console for details. Products: {string.Join(", ", printifyProducts.Select(p => p.ProductName))}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== PRINTIFY SYNC FAILED ===");
                _logger.LogError($"Error: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                return $"ERROR: {ex.GetType().Name} - {ex.Message}\n\n{ex.StackTrace}";
            }
        }
    }
}
