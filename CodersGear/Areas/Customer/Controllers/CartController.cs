using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodersGear.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Get all shopping carts with product and category, filtered by user
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.Category"),
                OrderHeader = new()
            };

            // Calculate order total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Increment(int id)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id);
            if (cartFromDb != null)
            {
                cartFromDb.Count += 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Decrement(int id)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id);
            if (cartFromDb != null && cartFromDb.Count > 1)
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == id);
            if (cartFromDb != null)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Item removed from cart";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Get shopping cart items
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.Category"),
                OrderHeader = new()
            };

            // Get user information and populate OrderHeader
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (applicationUser != null)
            {
                ShoppingCartVM.OrderHeader.Name = applicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = applicationUser.City;
                ShoppingCartVM.OrderHeader.State = applicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;
                ShoppingCartVM.OrderHeader.Country = applicationUser.Country;
            }

            // Calculate order total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        public IActionResult Summary(ShoppingCartVM shoppingCartVM)
        {
            // This is where you would process payment
            // For now, just show a success message
            TempData["success"] = "Order placed successfully! (Demo mode - no actual payment processed)";
            return RedirectToAction("Index", "Home");
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return (double)shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return (double)shoppingCart.Product.Price50;
                }
                else
                {
                    return (double)shoppingCart.Product.Price100;
                }
            }
        }
    }
}
