using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodersGear.Utility
{
    #region Shop Models
    public class PrintifyShop
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("sales_channel")]
        public string SalesChannel { get; set; } = string.Empty;
    }
    #endregion

    #region Product Models
    public class PrintifyProduct
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("options")]
        public List<PrintifyProductOption> Options { get; set; } = new();

        [JsonPropertyName("variants")]
        public List<PrintifyVariant> Variants { get; set; } = new();

        [JsonPropertyName("images")]
        public List<PrintifyImage> Images { get; set; } = new();

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("is_locked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("blueprint_id")]
        public int BlueprintId { get; set; }

        [JsonPropertyName("print_provider_id")]
        public int PrintProviderId { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;
    }

    public class PrintifyProductOption
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("values")]
        public List<PrintifyOptionValue> Values { get; set; } = new();
    }

    public class PrintifyOptionValue
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class PrintifyVariant
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("grams")]
        public int Grams { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("is_printify_express_eligible")]
        public bool IsPrintifyExpressEligible { get; set; }

        [JsonPropertyName("options")]
        public List<int> Options { get; set; } = new();

        // Parsed from Title (e.g., "Small / Black" -> Size="Small", Color="Black")
        public string? Size { get; set; }
        public string? Color { get; set; }

        public void ParseTitle()
        {
            if (string.IsNullOrEmpty(Title))
                return;

            var parts = Title.Split(new[] { " / ", " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
                Size = parts[0].Trim();
            if (parts.Length >= 2)
                Color = parts[1].Trim();
        }
    }

    public class PrintifyImage
    {
        [JsonPropertyName("src")]
        public string Src { get; set; } = string.Empty;

        [JsonPropertyName("variant_ids")]
        public List<int> VariantIds { get; set; } = new();

        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }
    #endregion

    #region Order Models
    public class PrintifyOrderRequest
    {
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("line_items")]
        public List<PrintifyLineItem> LineItems { get; set; } = new();

        [JsonPropertyName("shipping_method")]
        public int ShippingMethod { get; set; } = 1;

        [JsonPropertyName("is_printify_express")]
        public bool IsPrintifyExpress { get; set; } = false;

        [JsonPropertyName("is_economy_shipping")]
        public bool IsEconomyShipping { get; set; } = false;

        [JsonPropertyName("send_shipping_notification")]
        public bool SendShippingNotification { get; set; } = false;

        [JsonPropertyName("address_to")]
        public PrintifyAddress AddressTo { get; set; } = null!;
    }

    public class PrintifyLineItem
    {
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("variant_id")]
        public int VariantId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }
    }

    public class PrintifyAddress
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = "US";

        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        [JsonPropertyName("address1")]
        public string Address1 { get; set; } = string.Empty;

        [JsonPropertyName("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("zip")]
        public string Zip { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string? Company { get; set; }
    }

    public class PrintifyOrderResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class PrintifyOrder
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("line_items")]
        public List<PrintifyOrderLineItem> LineItems { get; set; } = new();

        [JsonPropertyName("shipments")]
        public List<PrintifyShipment> Shipments { get; set; } = new();

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("sent_to_production_at")]
        public string? SentToProductionAt { get; set; }

        [JsonPropertyName("fulfilled_at")]
        public string? FulfilledAt { get; set; }
    }

    public class PrintifyOrderLineItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("variant_id")]
        public int VariantId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class PrintifyShipment
    {
        [JsonPropertyName("carrier")]
        public string Carrier { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public string TrackingNumber { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string TrackingUrl { get; set; } = string.Empty;
    }
    #endregion

    #region Webhook Models
    public class PrintifyWebhookEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("resource")]
        public PrintifyWebhookResource Resource { get; set; } = null!;
    }

    public class PrintifyWebhookResource
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }

    public class PrintifyOrderUpdatedData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class PrintifyShipmentData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("shipped_at")]
        public string ShippedAt { get; set; } = string.Empty;

        [JsonPropertyName("carrier")]
        public PrintifyCarrier Carrier { get; set; } = null!;

        [JsonPropertyName("skus")]
        public List<string> Skus { get; set; } = new();
    }

    public class PrintifyCarrier
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("tracking_number")]
        public string TrackingNumber { get; set; } = string.Empty;

        [JsonPropertyName("tracking_url")]
        public string TrackingUrl { get; set; } = string.Empty;
    }

    public class PrintifyProductPublishingData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("external")]
        public PrintifyProductPublishingExternal? External { get; set; }
    }

    public class PrintifyProductPublishingExternal
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("handle")]
        public string Handle { get; set; } = string.Empty;
    }

    public class PrintifyPublishStartedData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("publish_details")]
        public PrintifyPublishDetails? PublishDetails { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;
    }

    public class PrintifyPublishDetails
    {
        [JsonPropertyName("title")]
        public bool Title { get; set; }

        [JsonPropertyName("description")]
        public bool Description { get; set; }

        [JsonPropertyName("images")]
        public bool Images { get; set; }

        [JsonPropertyName("variants")]
        public bool Variants { get; set; }

        [JsonPropertyName("tags")]
        public bool Tags { get; set; }
    }

    public class PrintifyShipmentDeliveredData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("delivered_at")]
        public string DeliveredAt { get; set; } = string.Empty;

        [JsonPropertyName("skus")]
        public List<string> Skus { get; set; } = new();
    }

    public class PrintifyProductDeletedData
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
    #endregion
}
