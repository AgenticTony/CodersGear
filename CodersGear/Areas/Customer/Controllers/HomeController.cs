using System.Diagnostics;
using System.Security.Claims;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class 
        HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Get all products with categories
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category");

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

            // Get all products with categories
            ShoppingCart cart = new()
            {
                Count = 1,
                ProductId = productId.Value,
                Product = _unitOfWork.Product.Get(u => u.ProductId == productId.Value, includeProperties: "Category")
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
            // Check if user is authenticated
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                // Not logged in - redirect to login with returnUrl back to product page
                string returnUrl = $"/Customer/Home/Details/{shoppingCart.ProductId}";
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            // Check if shopping cart already exists for this user and product
            // Using tracked: false so we don't accidentally auto-update without calling Update()
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId,
                tracked: false);

            if (cartFromDb != null)
            {
                // Shopping cart already exists, update the count
                shoppingCart.Count += cartFromDb.Count;
                _unitOfWork.ShoppingCart.Update(shoppingCart);
            }
            else
            {
                // Shopping cart doesn't exist, add new entry
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();
            TempData["success"] = "Cart updated successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult BuyNow(int productId)
        {
            // Check if user is authenticated
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                // Not logged in - redirect to login with returnUrl to this product
                string returnUrl = $"/Customer/Home/Details/{productId}";
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Check if shopping cart already exists for this user and product
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.ProductId == productId,
                tracked: false);

            ShoppingCart shoppingCart = new ShoppingCart
            {
                ProductId = productId,
                Count = 1,
                ApplicationUserId = userId
            };

            if (cartFromDb != null)
            {
                // Shopping cart already exists, update the count
                shoppingCart.Count += cartFromDb.Count;
                _unitOfWork.ShoppingCart.Update(shoppingCart);
            }
            else
            {
                // Shopping cart doesn't exist, add new entry
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();
            TempData["success"] = "Item added to cart";

            // Redirect to Cart page (not Index)
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Hoddies()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 2);
            return View(products);
        }

        public IActionResult TShirts()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 1);
            return View(products);
        }

        public IActionResult Mugs()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 3);
            return View(products);
        }

        public IActionResult Accessories()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.CategoryId == 4);
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
    }
}
