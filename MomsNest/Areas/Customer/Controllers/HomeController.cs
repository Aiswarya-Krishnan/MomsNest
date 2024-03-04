using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var categories = unitOfWork.Category.GetAll().ToList();
            ViewBag.Categories = categories;
            return View(PrductList);
            
        }

        public IActionResult ProductByCategory(int? category)
        {
            IEnumerable<Product> productList = unitOfWork.Product.GetAll().ToList();
            var categories = unitOfWork.Category.GetAll().ToList();
            ViewBag.Categories = categories; // Pass categories to the layout

            if (category != null)
            {
                productList = productList.Where(p => p.CategoryID == category);
            }

            return View(productList.ToList());
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
        public IActionResult Search(string searchString, int? categoryId)
        {
            // Get all products
            IEnumerable<Product> productList = unitOfWork.Product.GetAll().ToList();

            // Filter by category if provided
            if (categoryId != null)
            {
                productList = productList.Where(p => p.CategoryID == categoryId);
            }

            // Filter by search string if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower(); // Convert search string to lowercase for case-insensitive comparison

                // Filter by product name or category name containing the search string
                productList = productList.Where(p =>
                    p.ProductName.ToLower().Contains(searchString) ||
                    p.Category.Name.ToLower().Contains(searchString));
            }

            var categories = unitOfWork.Category.GetAll().ToList();
            ViewBag.Categories = categories; // Pass categories to the layout

            return View("ProductByCategory", productList.ToList());
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
