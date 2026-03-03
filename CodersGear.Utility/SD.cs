using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_InProcess = "InProcess";
        public const string Status_Shipped = "Shipped";
        public const string Status_Delivered = "Delivered";
        public const string Status_Cancelled = "Cancelled";

        public const string PaymentStatus_Pending = "Pending";
        public const string PaymentStatus_Approved = "Approved";
        public const string PaymentStatus_DelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatus_Rejected = "Rejected";
    }
}
