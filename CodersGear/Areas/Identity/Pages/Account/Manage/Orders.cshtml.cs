#nullable disable

using System.Security.Claims;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodersGear.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrdersModel> _logger;

        public OrdersModel(IUnitOfWork unitOfWork, ILogger<OrdersModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public List<OrderWithDetails> Orders { get; set; }
        public string StatusMessage { get; set; }

        public class OrderWithDetails
        {
            public OrderHeader OrderHeader { get; set; }
            public List<OrderDetail> OrderDetails { get; set; }
            public int Id => OrderHeader.Id;
            public DateTime OrderDate => OrderHeader.OrderDate;
            public string OrderStatus => OrderHeader.OrderStatus;
            public string PaymentStatus => OrderHeader.PaymentStatus;
            public decimal OrderTotal => OrderHeader.OrderTotal;
            public string Name => OrderHeader.Name;
            public string PhoneNumber => OrderHeader.PhoneNumber;
            public string StreetAddress => OrderHeader.StreetAddress;
            public string City => OrderHeader.City;
            public string State => OrderHeader.State;
            public string PostalCode => OrderHeader.PostalCode;
            public string Country => OrderHeader.Country;
            public string TrackingNumber => OrderHeader.TrackingNumber;
            public string Carrier => OrderHeader.Carrier;
            public DateTime ShippingDate => OrderHeader.ShippingDate;
            public string SessionId => OrderHeader.SessionId;
            public string PrintifyOrderId => OrderHeader.PrintifyOrderId;
        }

        public void OnGet()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    Orders = new List<OrderWithDetails>();
                    StatusMessage = "Unable to load user information.";
                    return;
                }

                // Get all orders for this user
                var orderHeaders = _unitOfWork.OrderHeader
                    .GetAll(o => o.ApplicationUserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                // Load order details for each order
                Orders = new List<OrderWithDetails>();
                foreach (var orderHeader in orderHeaders)
                {
                    var orderDetails = _unitOfWork.OrderDetail
                        .GetAll(od => od.OrderHeaderId == orderHeader.Id, includeProperties: "Product")
                        .ToList();

                    Orders.Add(new OrderWithDetails
                    {
                        OrderHeader = orderHeader,
                        OrderDetails = orderDetails
                    });
                }

                _logger.LogInformation($"Loaded {Orders.Count} orders for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for user");
                Orders = new List<OrderWithDetails>();
                StatusMessage = "Error loading orders. Please try again later.";
            }
        }
    }
}
