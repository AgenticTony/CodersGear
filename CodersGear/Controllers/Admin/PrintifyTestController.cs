using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PrintifyTestController : Controller
    {
        private readonly IPrintifyProductSyncService _syncService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrintifyTestController> _logger;

        public PrintifyTestController(
            IPrintifyProductSyncService syncService,
            IUnitOfWork unitOfWork,
            ILogger<PrintifyTestController> logger)
        {
            _syncService = syncService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var printifyProducts = _unitOfWork.Product.GetAll(p => p.IsPrintifyProduct == true).ToList();
            var allProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            ViewData["PrintifyProducts"] = printifyProducts;
            ViewData["TotalProducts"] = allProducts.Count;
            ViewData["PrintifyCount"] = printifyProducts.Count;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SyncNow()
        {
            try
            {
                _logger.LogInformation("Manual sync triggered from Admin panel");
                await _syncService.SyncProductsAsync();

                var printifyProducts = _unitOfWork.Product.GetAll(p => p.IsPrintifyProduct == true).ToList();
                return Json(new { success = true, count = printifyProducts.Count, message = $"Synced {printifyProducts.Count} products" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync failed: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
