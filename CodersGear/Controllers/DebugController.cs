using CodersGear.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Controllers
{
    public class DebugController : Controller
    {
        private readonly IPrintifyProductSyncService _syncService;
        private readonly ILogger<DebugController> _logger;

        public DebugController(IPrintifyProductSyncService syncService, ILogger<DebugController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> SyncPrintify()
        {
            try
            {
                _logger.LogInformation("Manual sync triggered via Debug/SyncPrintify");
                await _syncService.SyncProductsAsync();
                return "Sync completed successfully! Check your database for Printify products.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync failed: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                return $"ERROR: {ex.GetType().Name} - {ex.Message}\n\n{ex.StackTrace}";
            }
        }
    }
}
