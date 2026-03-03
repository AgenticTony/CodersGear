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
                        url = w.Url,
                        events = w.Events,
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
        /// GET: /admin/webhookmanagement/create-simple?shopId={shopId}&webhookUrl={url}
        /// </summary>
        [HttpGet("create-simple")]
        public async Task<IActionResult> CreateWebhookSimple([FromQuery] string shopId, [FromQuery] string webhookUrl)
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

                var events = GetDefaultEvents();
                var success = await _printifyService.CreateWebhookAsync(shopId, webhookUrl, events);

                if (success)
                {
                    _logger.LogInformation("Created webhook for shop {ShopId} at {Url}", shopId, webhookUrl);
                    return Ok(new
                    {
                        success = true,
                        message = "Webhook created successfully!",
                        shopId = shopId,
                        url = webhookUrl,
                        events = events
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
        /// Body: { "shopId": "123456", "webhookUrl": "https://yourdomain.com/api/printifywebhook", "events": ["order:updated", "order:shipment:created"] }
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

                if (request.Events == null || !request.Events.Any())
                {
                    request.Events = GetDefaultEvents();
                }

                var success = await _printifyService.CreateWebhookAsync(request.ShopId, request.WebhookUrl, request.Events);

                if (success)
                {
                    _logger.LogInformation("Created webhook for shop {ShopId} at {Url}", request.ShopId, request.WebhookUrl);
                    return Ok(new
                    {
                        success = true,
                        message = "Webhook created successfully",
                        shopId = request.ShopId,
                        url = request.WebhookUrl,
                        events = request.Events
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
        /// Delete a webhook from a Printify shop
        /// DELETE: /admin/webhookmanagement/delete?shopId={shopId}&webhookId={webhookId}
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteWebhook([FromQuery] string shopId, [FromQuery] string webhookId)
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

                var success = await _printifyService.DeleteWebhookAsync(shopId, webhookId);

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

        private List<string> GetDefaultEvents()
        {
            return new List<string>
            {
                "order:updated",
                "order:shipment:created",
                "product:publishing_succeeded",
                "product:publishing_failed",
                "product:unpublished"
            };
        }
    }

    public class CreateWebhookRequest
    {
        public string ShopId { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public List<string>? Events { get; set; }
    }
}
