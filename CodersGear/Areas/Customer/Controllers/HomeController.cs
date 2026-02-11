using System.Diagnostics;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
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

        public IActionResult Details(int productId)
        {
            // Get all products with categories
            Product product = _unitOfWork.Product.Get(u=>u.ProductId==productId, includeProperties: "Category");
            return View(product);
        }

        public IActionResult Hoddies()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Category != null && p.Category.Name.Contains("Hoodi"));
            return View(products);
        }

        public IActionResult TShirts()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Category != null && p.Category.Name.Contains("T-shirt"));
            return View(products);
        }

        public IActionResult Mugs()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Category != null && p.Category.Name.Contains("Mug"));
            return View(products);
        }

        public IActionResult Accessories()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Category != null && p.Category.Name.Contains("Accessorie"));
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
