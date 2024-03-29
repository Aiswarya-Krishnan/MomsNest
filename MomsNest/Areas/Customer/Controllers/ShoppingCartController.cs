﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using Utilities;

namespace MomsNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public ShoppingCartViewModel shoppingCartViewModel { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork)
        {

            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartViewModel = new()
            {
                ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId,includeProperties:"Product"),
                OrderHeader=new()
            };

            foreach(var pric in shoppingCartViewModel.ShoppingCartList)
            {
                pric.Price=GetPrice(pric);
                shoppingCartViewModel.OrderHeader.OrderTotal +=( pric.Price * pric.Count);
            }

            return View(shoppingCartViewModel);
        }
        public IActionResult Plus(int cartid)
        {
            var cart = unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
            cart.Count += 1;
            unitOfWork.ShoppingCart.Update(cart);
            unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartid)
        {
            var cart = unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
            if (cart.Count <= 1)
            {
                unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count -= 1;
                unitOfWork.ShoppingCart.Update(cart);
            }

            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartid)
        {
            var cart = unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
           
                unitOfWork.ShoppingCart.Remove(cart);
                unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult OrderSummary()
        {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			var previousAddresses = unitOfWork.OrderHeader
		   .GetAll(o => o.ApplicationUserId == userId)
		   .Select(o => new SelectListItem
		   {
			   Text = $"{o.Name} , {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode} , {o.PhoneNumber} ",
			   Value = o.StreetAddress // You can set this to any unique identifier of the address if needed
		   })
		   .Distinct() // Remove duplicates
		   .ToList();

			// Populate the dropdown options
			ViewBag.PreviousAddresses = previousAddresses;


			shoppingCartViewModel = new()
                {
                    ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product"),
                    OrderHeader = new()
                };

            shoppingCartViewModel.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            shoppingCartViewModel.OrderHeader.Name=shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartViewModel.OrderHeader.StreetAddress = shoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartViewModel.OrderHeader.City = shoppingCartViewModel.OrderHeader.ApplicationUser.City;
            shoppingCartViewModel.OrderHeader.State = shoppingCartViewModel.OrderHeader.ApplicationUser.State;
            shoppingCartViewModel.OrderHeader.PostalCode = shoppingCartViewModel.OrderHeader.ApplicationUser.PostCode;
           


            foreach (var pric in shoppingCartViewModel.ShoppingCartList)
                {
                    pric.Price = GetPrice(pric);
                    shoppingCartViewModel.OrderHeader.OrderTotal += (pric.Price * pric.Count);
                }

                return View(shoppingCartViewModel);
            
        }




        [HttpPost]
        [ActionName("OrderSummary")]
		public IActionResult OrderSummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartViewModel.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product");
			
            
            shoppingCartViewModel.OrderHeader.OrderDate=System.DateTime.Now;
            shoppingCartViewModel.OrderHeader.ApplicationUserId=userId;
			ApplicationUser applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            foreach (var pric in shoppingCartViewModel.ShoppingCartList)
            {
                pric.Price = GetPrice(pric);
                shoppingCartViewModel.OrderHeader.OrderTotal += (pric.Price * pric.Count);

                // Retrieve the product from the repository by its ID
                var product = unitOfWork.Product.Get(p => p.ProductId == pric.Product_ID);

                if (product != null)
                {
                    // Reduce the stock quantity by one
                    product.StockQuantity -= pric.Count;
                    unitOfWork.Product.Update(product);
                    unitOfWork.Save();
                }
            }


            if (applicationUser != null)
            {
                shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusPending;
                shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusPending;
            }
            unitOfWork.OrderHeader.Add(shoppingCartViewModel.OrderHeader);
            unitOfWork.Save();

            foreach(var cart in shoppingCartViewModel.ShoppingCartList)
            {
                OrderDetails orderDetails = new()
                {
                    Product_ID=cart.Product_ID,
                    OrderHeader_ID=shoppingCartViewModel.OrderHeader.OrderHeaderId,
                    Price=cart.Price,
                    Count=cart.Count,

                };
                unitOfWork.OrderDetails.Add(orderDetails);
                unitOfWork.Save();
            }

            unitOfWork.ShoppingCart.RemoveRange(shoppingCartViewModel.ShoppingCartList);
            unitOfWork.Save();

            if (applicationUser != null)
            {
				
			}

            return RedirectToAction(nameof(OrderConfirmation), new { orderId = shoppingCartViewModel.OrderHeader.OrderHeaderId });

		}

        public IActionResult OrderConfirmation(int orderId)
        {
           
            return View(orderId);                                                           
        }


		private decimal GetPrice(ShoppingCart shopping)
        {
            return shopping.Product.Price;
        }
    }
}
