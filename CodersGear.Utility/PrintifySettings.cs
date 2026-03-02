namespace CodersGear.Utility
{
    public class PrintifySettings
    {
        public const string SectionName = "Printify";
        public string ApiUrl { get; set; } = "https://api.printify.com/v1";
        public string ApiToken { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
