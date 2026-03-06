using Microsoft.AspNetCore.Mvc;
using CodersGear.Utility;

namespace CodersGear.Controllers.Admin
{
    [Route("admin/[controller]")]
    [ApiController]
    public class WebhookManagementController : ControllerBase
    {
        private readonly IPrintifyService _printifyService;
        private readonly ILogger<WebhookManagementController> _logger;
        private readonly IConfiguration _configuration;

        public WebhookManagementController(
            IPrintifyService printifyService,
            ILogger<WebhookManagementController> logger,
            IConfiguration configuration)
        {
            _printifyService = printifyService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get list of valid Printify webhook topics
        /// GET: /admin/webhookmanagement/topics
        /// </summary>
        [HttpGet("topics")]
        public IActionResult GetValidTopics()
        {
            return Ok(new
            {
                success = true,
                topics = GetValidWebhookTopics()
            });
        }

        /// <summary>
        /// List all webhooks for a Printify shop
        /// GET: /admin/webhookmanagement/list?shopId={shopId}
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> ListWebhooks([FromQuery] string shopId)
        {
            try
            {
                var webhooks = await _printifyService.GetWebhooksAsync(shopId);
                return Ok(new
                {
                    success = true,
                    shopId,
                    webhooks = webhooks.Select(w => new
                    {
                        id = w.Id,
                        topic = w.Topic,
                        url = w.Url,
                        shopId = w.ShopId,
                        createdAt = w.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing webhooks for shop {ShopId}", shopId);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new webhook for a Printify shop (simple GET method for browser testing)
        /// GET: /admin/webhookmanagement/create-simple?shopId={shopId}&webhookUrl={url}&topic={topic}
        /// </summary>
        [HttpGet("create-simple")]
        public async Task<IActionResult> CreateWebhookSimple(
            [FromQuery] string shopId,
            [FromQuery] string webhookUrl,
            [FromQuery] string? topic = null)
        {
            try
            {
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest(new { success = false, error = "shopId is required" });
                }

                if (string.IsNullOrEmpty(webhookUrl))
                {
                    return BadRequest(new { success = false, error = "webhookUrl is required" });
                }

                // Use provided topic or default to order:updated
                var webhookTopic = topic ?? "order:updated";

                // Get webhook secret from configuration
                var secret = _configuration["Printify:WebhookSecret"];

                var success = await _printifyService.CreateWebhookAsync(shopId, webhookTopic, webhookUrl, secret);

                if (success)
                {
                    _logger.LogInformation("Created webhook for shop {ShopId} at {Url} with topic {Topic}", shopId, webhookUrl, webhookTopic);
                    return Ok(new
                    {
                        success = true,
                        message = "Webhook created successfully!",
                        shopId = shopId,
                        url = webhookUrl,
                        topic = webhookTopic
                    });
                }
                else
                {
                    return BadRequest(new { success = false, error = "Failed to create webhook" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook for shop {ShopId}", shopId);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new webhook for a Printify shop
        /// POST: /admin/webhookmanagement/create
        /// Body: { "shopId": "123456", "webhookUrl": "https://yourdomain.com/api/printifywebhook", "topic": "order:updated" }
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ShopId))
                {
                    return BadRequest(new { success = false, error = "shopId is required" });
                }

                if (string.IsNullOrEmpty(request.WebhookUrl))
                {
                    return BadRequest(new { success = false, error = "webhookUrl is required" });
                }

                if (string.IsNullOrEmpty(request.Topic))
                {
                    return BadRequest(new { success = false, error = "topic is required. Valid topics: " + string.Join(", ", GetValidWebhookTopics()) });
                }

                // Get webhook secret from configuration
                var secret = _configuration["Printify:WebhookSecret"];

                var success = await _printifyService.CreateWebhookAsync(request.ShopId, request.Topic, request.WebhookUrl, secret);

                if (success)
                {
                    _logger.LogInformation("Created webhook for shop {ShopId} at {Url} with topic {Topic}", request.ShopId, request.WebhookUrl, request.Topic);
                    return Ok(new
                    {
                        success = true,
                        message = "Webhook created successfully",
                        shopId = request.ShopId,
                        url = request.WebhookUrl,
                        topic = request.Topic
                    });
                }
                else
                {
                    return BadRequest(new { success = false, error = "Failed to create webhook" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook for shop {ShopId}", request.ShopId);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create webhooks for all recommended topics
        /// POST: /admin/webhookmanagement/create-all
        /// Body: { "shopId": "123456", "webhookUrl": "https://yourdomain.com/api/printifywebhook" }
        /// </summary>
        [HttpPost("create-all")]
        public async Task<IActionResult> CreateAllWebhooks([FromBody] CreateWebhookRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ShopId))
                {
                    return BadRequest(new { success = false, error = "shopId is required" });
                }

                if (string.IsNullOrEmpty(request.WebhookUrl))
                {
                    return BadRequest(new { success = false, error = "webhookUrl is required" });
                }

                var results = new List<object>();
                var topics = GetDefaultTopics();
                var secret = _configuration["Printify:WebhookSecret"];

                foreach (var topic in topics)
                {
                    try
                    {
                        var success = await _printifyService.CreateWebhookAsync(request.ShopId, topic, request.WebhookUrl, secret);
                        results.Add(new { topic, success });
                        _logger.LogInformation("Created webhook for topic {Topic}: {Success}", topic, success);
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { topic, success = false, error = ex.Message });
                        _logger.LogError(ex, "Failed to create webhook for topic {Topic}", topic);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Webhook creation completed",
                    shopId = request.ShopId,
                    webhookUrl = request.WebhookUrl,
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating all webhooks for shop {ShopId}", request.ShopId);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a webhook from a Printify shop
        /// DELETE: /admin/webhookmanagement/delete?shopId={shopId}&webhookId={webhookId}&webhookUrl={webhookUrl}
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteWebhook([FromQuery] string shopId, [FromQuery] string webhookId, [FromQuery] string webhookUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest(new { success = false, error = "shopId is required" });
                }

                if (string.IsNullOrEmpty(webhookId))
                {
                    return BadRequest(new { success = false, error = "webhookId is required" });
                }

                // Extract host from webhook URL (required by Printify API for deletion)
                string host = "";
                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    var uri = new Uri(webhookUrl);
                    host = uri.Host;
                }

                var success = await _printifyService.DeleteWebhookAsync(shopId, webhookId, host);

                if (success)
                {
                    _logger.LogInformation("Deleted webhook {WebhookId} from shop {ShopId}", webhookId, shopId);
                    return Ok(new
                    {
                        success = true,
                        message = "Webhook deleted successfully",
                        shopId,
                        webhookId
                    });
                }
                else
                {
                    return BadRequest(new { success = false, error = "Failed to delete webhook" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webhook {WebhookId} from shop {ShopId}", webhookId, shopId);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get the configured Printify shop ID from appsettings
        /// GET: /admin/webhookmanagement/shop-id
        /// </summary>
        [HttpGet("shop-id")]
        public IActionResult GetShopId()
        {
            var shopId = _configuration["Printify:ShopId"];
            return Ok(new
            {
                success = true,
                shopId
            });
        }

        /// <summary>
        /// Valid Printify webhook topics for reference
        /// </summary>
        private List<string> GetValidWebhookTopics()
        {
            return new List<string>
            {
                "shop:disconnected",
                "product:deleted",
                "product:publish:started",
                "order:created",
                "order:updated",
                "order:sent-to-production",
                "order:shipment:created",
                "order:shipment:delivered"
            };
        }

        /// <summary>
        /// Default topics recommended for this application
        /// </summary>
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

    public class CreateWebhookRequest
    {
        public string ShopId { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Single topic for the webhook (Printify requires one topic per webhook)
        /// </summary>
        public string? Topic { get; set; }
    }
}
