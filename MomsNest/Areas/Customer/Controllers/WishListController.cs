using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MomsNest.DataAccess.Migrations;
using MomsNest.DataAccess.Repository;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Utilities;

namespace MomsNest.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class WishListController:Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public WishListVM WishList {  get; set; }
        public WishListController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var WishlistItems = unitOfWork.WishList.GetAll(
               u => u.ApplicationUserID == userId,
               includeProperties: "Product.ProductImages"
           );



            return View(WishlistItems);
            
        }

        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            var existingWishlistItem = unitOfWork.WishList.Get(u => u.ApplicationUserID == claim.Value && u.Product_ID == productId);
            

            Product product = unitOfWork.Product.Get(p => p.ProductId == productId);


            if (existingWishlistItem == null)
            {
                var newWishlistItem = new WishList
                {

                    ApplicationUserID = claim.Value,
                    Product_ID = productId,
                    Description = product.Description,
                    ProdctName = product.ProductName,
                    Price=product.Price,

                };

                unitOfWork.WishList.Add(newWishlistItem);
                unitOfWork.Save();

                TempData["SuccessMessage"] = "Product added to Wishlist successfully.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        public ActionResult RemoveFromWishlist(int id)
        {
            var WishlistItem = unitOfWork.WishList.Get(u => u.WishListId == id);

            if (WishlistItem != null)
            {
                unitOfWork.WishList.Remove(WishlistItem);
                unitOfWork.Save();


                TempData["SuccessMessage"] = "Product removed from Wishlist successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Wishlist item not found.";
            }

            return RedirectToAction(nameof(Index));

        }
    }
}