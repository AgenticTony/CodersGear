using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CodersGear.Services;

namespace CodersGear.Services
{
    public class PrintifyBackgroundSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PrintifyBackgroundSyncService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromHours(1); // Sync every hour

        public PrintifyBackgroundSyncService(
            IServiceProvider serviceProvider,
            ILogger<PrintifyBackgroundSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("=== Printify Background Sync Service is starting. ===");

            // Do an initial sync
            try
            {
                await PerformSyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FATAL ERROR during initial sync: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_syncInterval, stoppingToken);
                    await PerformSyncAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Normal cancellation, exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in background sync: {ex.GetType().Name} - {ex.Message}");
                    _logger.LogError($"Stack Trace: {ex.StackTrace}");
                    // Wait before retrying to avoid tight error loop
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("=== Printify Background Sync Service is stopping. ===");
        }

        private async Task PerformSyncAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IPrintifyProductSyncService>();

            _logger.LogInformation("Starting scheduled Printify product sync...");
            await syncService.SyncProductsAsync();
            _logger.LogInformation("Scheduled Printify product sync completed.");
        }
    }
}
