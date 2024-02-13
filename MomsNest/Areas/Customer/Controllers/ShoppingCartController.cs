using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using System.Security.Claims;

namespace MomsNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
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
                ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId,includeProperties:"Product")
            };

            foreach(var pric in shoppingCartViewModel.ShoppingCartList)
            {
                pric.Price=GetPrice(pric);
                shoppingCartViewModel.OrderTotal +=( pric.Price * pric.Count);
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
            return View();
        }

        private decimal GetPrice(ShoppingCart shopping)
        {
            return shopping.Product.Price;
        }
    }
}
