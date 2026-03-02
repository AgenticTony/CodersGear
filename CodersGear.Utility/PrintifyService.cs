using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace CodersGear.Utility
{
    public interface IPrintifyService
    {
        Task<List<PrintifyShop>> GetShopsAsync();
        Task<List<PrintifyProduct>> GetProductsAsync(string shopId);
        Task<PrintifyProduct?> GetProductAsync(string shopId, string productId);
        Task<PrintifyOrderResponse> CreateOrderAsync(string shopId, PrintifyOrderRequest order);
        Task<PrintifyOrder?> GetOrderAsync(string shopId, string orderId);
        Task<bool> CreateWebhookAsync(string shopId, string webhookUrl, List<string> events);
        Task<List<PrintifyWebhook>> GetWebhooksAsync(string shopId);
        Task<bool> DeleteWebhookAsync(string shopId, string webhookId);
    }

    public class PrintifyService : IPrintifyService
    {
        private readonly HttpClient _httpClient;
        private readonly PrintifySettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public PrintifyService(HttpClient httpClient, IOptions<PrintifySettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        private void SetAuthHeader()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiToken}");
        }

        public async Task<List<PrintifyShop>> GetShopsAsync()
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/shops.json");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get shops: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PrintifyShop>>(content, _jsonOptions) ?? new List<PrintifyShop>();
        }

        public async Task<List<PrintifyProduct>> GetProductsAsync(string shopId)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/shops/{shopId}/products.json");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get products: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PrintifyProduct>>(content, _jsonOptions) ?? new List<PrintifyProduct>();
        }

        public async Task<PrintifyProduct?> GetProductAsync(string shopId, string productId)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/shops/{shopId}/products/{productId}.json");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw new HttpRequestException($"Failed to get product: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PrintifyProduct>(content, _jsonOptions);
        }

        public async Task<PrintifyOrderResponse> CreateOrderAsync(string shopId, PrintifyOrderRequest order)
        {
            SetAuthHeader();
            var json = JsonSerializer.Serialize(order, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/shops/{shopId}/orders.json", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create order: {response.StatusCode}. Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PrintifyOrderResponse>(responseContent, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize order response");
        }

        public async Task<PrintifyOrder?> GetOrderAsync(string shopId, string orderId)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/shops/{shopId}/orders/{orderId}.json");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw new HttpRequestException($"Failed to get order: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PrintifyOrder>(content, _jsonOptions);
        }

        public async Task<bool> CreateWebhookAsync(string shopId, string webhookUrl, List<string> events)
        {
            SetAuthHeader();
            var webhookRequest = new
            {
                url = webhookUrl,
                events = events,
                headers = new
                {
                    Authorization = _settings.WebhookSecret
                }
            };

            var json = JsonSerializer.Serialize(webhookRequest, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/shops/{shopId}/webhooks.json", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create webhook: {response.StatusCode}. Error: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<List<PrintifyWebhook>> GetWebhooksAsync(string shopId)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/shops/{shopId}/webhooks.json");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get webhooks: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PrintifyWebhook>>(content, _jsonOptions) ?? new List<PrintifyWebhook>();
        }

        public async Task<bool> DeleteWebhookAsync(string shopId, string webhookId)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"{_settings.ApiUrl}/shops/{shopId}/webhooks/{webhookId}.json");

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw new HttpRequestException($"Failed to delete webhook: {response.StatusCode}");
            }

            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
        }
    }

    public class PrintifyWebhook
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("events")]
        public List<string> Events { get; set; } = new();

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
