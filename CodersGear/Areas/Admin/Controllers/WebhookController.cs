using Microsoft.AspNetCore.Mvc;
using CodersGear.Utility;
using CodersGear.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;

namespace CodersGear.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class WebhookController : Controller
    {
        private readonly IPrintifyService _printifyService;
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public WebhookController(
            IPrintifyService printifyService,
            ILogger<WebhookController> logger,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _printifyService = printifyService;
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var shopId = _configuration["Printify:ShopId"];

            if (string.IsNullOrEmpty(shopId))
            {
                TempData["error"] = "Printify Shop ID is not configured. Please set Printify:ShopId in your configuration.";
                return View(new WebhookIndexViewModel
                {
                    ShopId = "",
                    Webhooks = new List<PrintifyWebhook>(),
                    WebhookUrl = ""
                });
            }

            try
            {
                var webhooks = await _printifyService.GetWebhooksAsync(shopId);
                var webhookUrl = GetWebhookUrl();

                var viewModel = new WebhookIndexViewModel
                {
                    ShopId = shopId,
                    Webhooks = webhooks,
                    WebhookUrl = webhookUrl,
                    ValidTopics = GetValidWebhookTopics()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching webhooks for shop {ShopId}", shopId);
                TempData["error"] = $"Error fetching webhooks: {ex.Message}";
                return View(new WebhookIndexViewModel
                {
                    ShopId = shopId ?? "",
                    Webhooks = new List<PrintifyWebhook>(),
                    WebhookUrl = GetWebhookUrl()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string topic)
        {
            var shopId = _configuration["Printify:ShopId"];
            var webhookUrl = GetWebhookUrl();
            var secret = _configuration["Printify:WebhookSecret"];

            if (string.IsNullOrEmpty(shopId))
            {
                TempData["error"] = "Printify Shop ID is not configured.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(topic))
            {
                TempData["error"] = "Topic is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _printifyService.CreateWebhookAsync(shopId, topic, webhookUrl, secret);

                if (success)
                {
                    _logger.LogInformation("Created webhook for shop {ShopId} with topic {Topic}", shopId, topic);
                    TempData["success"] = $"Webhook created successfully for topic: {topic}";
                }
                else
                {
                    TempData["error"] = "Failed to create webhook.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook for shop {ShopId}", shopId);
                TempData["error"] = $"Error creating webhook: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAll()
        {
            var shopId = _configuration["Printify:ShopId"];
            var webhookUrl = GetWebhookUrl();
            var secret = _configuration["Printify:WebhookSecret"];

            if (string.IsNullOrEmpty(shopId))
            {
                TempData["error"] = "Printify Shop ID is not configured.";
                return RedirectToAction(nameof(Index));
            }

            var topics = GetDefaultTopics();
            var results = new List<string>();

            foreach (var topic in topics)
            {
                try
                {
                    var success = await _printifyService.CreateWebhookAsync(shopId, topic, webhookUrl, secret);
                    results.Add($"{topic}: {(success ? "✓" : "✗")}");
                }
                catch (Exception ex)
                {
                    results.Add($"{topic}: Error - {ex.Message}");
                }
            }

            TempData["success"] = $"Webhook creation results:\n{string.Join("\n", results)}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string webhookId)
        {
            var shopId = _configuration["Printify:ShopId"];

            if (string.IsNullOrEmpty(shopId))
            {
                TempData["error"] = "Printify Shop ID is not configured.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(webhookId))
            {
                TempData["error"] = "Webhook ID is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _printifyService.DeleteWebhookAsync(shopId, webhookId);

                if (success)
                {
                    _logger.LogInformation("Deleted webhook {WebhookId} from shop {ShopId}", webhookId, shopId);
                    TempData["success"] = "Webhook deleted successfully.";
                }
                else
                {
                    TempData["error"] = "Failed to delete webhook.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webhook {WebhookId}", webhookId);
                TempData["error"] = $"Error deleting webhook: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetWebhookUrl()
        {
            var baseUrl = _configuration["Printify:WebhookBaseUrl"]
                ?? $"{Request.Scheme}://{Request.Host}";
            return $"{baseUrl.TrimEnd('/')}/api/printifywebhook";
        }

        private List<WebhookTopicInfo> GetValidWebhookTopics()
        {
            return new List<WebhookTopicInfo>
            {
                new() { Topic = "order:created", Description = "Order was created" },
                new() { Topic = "order:updated", Description = "Order status was updated" },
                new() { Topic = "order:sent-to-production", Description = "Order sent to production" },
                new() { Topic = "order:shipment:created", Description = "Items have been shipped" },
                new() { Topic = "order:shipment:delivered", Description = "Items have been delivered" },
                new() { Topic = "product:publish:started", Description = "Product publishing started" },
                new() { Topic = "product:deleted", Description = "Product was deleted" },
                new() { Topic = "shop:disconnected", Description = "Shop was disconnected" }
            };
        }

        private List<string> GetDefaultTopics()
        {
            return new List<string>
            {
                "order:updated",
                "order:shipment:created",
                "order:shipment:delivered",
                "product:publish:started",
                "product:deleted"
            };
        }
    }

    public class WebhookIndexViewModel
    {
        public string ShopId { get; set; } = string.Empty;
        public List<PrintifyWebhook> Webhooks { get; set; } = new();
        public string WebhookUrl { get; set; } = string.Empty;
        public List<WebhookTopicInfo> ValidTopics { get; set; } = new();
    }

    public class WebhookTopicInfo
    {
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
