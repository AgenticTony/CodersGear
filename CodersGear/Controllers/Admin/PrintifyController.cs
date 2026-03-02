using CodersGear.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodersGear.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PrintifyController : Controller
    {
        private readonly IPrintifyService _printifyService;
        private readonly PrintifySettings _settings;

        public PrintifyController(IPrintifyService printifyService, IOptions<PrintifySettings> settings)
        {
            _printifyService = printifyService;
            _settings = settings.Value;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var shops = await _printifyService.GetShopsAsync();
                return View(shops);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error fetching shops: {ex.Message}";
                return View(new List<PrintifyShop>());
            }
        }

        public async Task<IActionResult> Products()
        {
            if (string.IsNullOrEmpty(_settings.ShopId))
            {
                TempData["error"] = "Shop ID is not configured. Please get your Shop ID first.";
                return RedirectToAction("Index");
            }

            try
            {
                var products = await _printifyService.GetProductsAsync(_settings.ShopId);
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error fetching products: {ex.Message}";
                return View(new List<PrintifyProduct>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncProducts()
        {
            if (string.IsNullOrEmpty(_settings.ShopId))
            {
                return Json(new { success = false, message = "Shop ID is not configured." });
            }

            try
            {
                var products = await _printifyService.GetProductsAsync(_settings.ShopId);
                return Json(new { success = true, message = $"Found {products.Count} products.", count = products.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
