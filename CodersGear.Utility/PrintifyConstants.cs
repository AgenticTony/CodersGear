namespace CodersGear.Utility
{
    public static class PrintifyConstants
    {
        // Order Status Mapping
        public const string ORDER_STATUS_PENDING = "pending";
        public const string ORDER_STATUS_ON_HOLD = "on-hold";
        public const string ORDER_STATUS_SENDING_TO_PRODUCTION = "sending-to-production";
        public const string ORDER_STATUS_IN_PRODUCTION = "in-production";
        public const string ORDER_STATUS_FULFILLED = "fulfilled";
        public const string ORDER_STATUS_PARTIALLY_FULFILLED = "partially-fulfilled";
        public const string ORDER_STATUS_CANCELED = "canceled";
        public const string ORDER_STATUS_HAS_ISSUES = "has-issues";

        // Product Status
        public const string PRODUCT_VISIBLE = "visible";
        public const string PRODUCT_HIDDEN = "hidden";

        // Shipping Methods
        public const int SHIPPING_STANDARD = 1;
        public const int SHIPPING_PRIORITY = 2;
        public const int SHIPPING_PRINTIFY_EXPRESS = 3;
        public const int SHIPPING_ECONOMY = 4;
    }
}
