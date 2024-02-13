using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MomsNest.Areas.Customer.Controllers
{
    [Area("Customer")]
  
    public class HomeController : Controller
    {
        
        private readonly IUnitOfWork unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> PrductList = unitOfWork.Product.GetAll().ToList();
            return View(PrductList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cart = new()
            {
                Product = unitOfWork.Product.Get(u => u.ProductId == id),
                Count = 1,
                Product_ID = id
            };
            
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserID= userId;

            ShoppingCart shoppingfromDDB = unitOfWork.ShoppingCart.Get(u => u.ApplicationUserID == userId &&
            u.Product_ID == shoppingCart.Product_ID);

            if (shoppingfromDDB != null)
            {
                shoppingfromDDB.Count += shoppingCart.Count;
                unitOfWork.ShoppingCart.Update(shoppingfromDDB);

            }
            else
            {
                unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            unitOfWork.Save();

            TempData["Success"] = "Cart Updated successfully";
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
