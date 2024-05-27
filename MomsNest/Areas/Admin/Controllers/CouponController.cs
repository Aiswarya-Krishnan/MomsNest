using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StatDetails.Role_Admin)]
    public class CouponController : Controller
    {
        private readonly IUnitOfWork context;

        public CouponController(IUnitOfWork context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            List<Coupons> CouponList = context.Coupons.GetAll().ToList();
            return View(CouponList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Coupons coupons)
        {
           
            if (ModelState.IsValid)
            {
                var existingcoupon = context.Coupons.Get(c => c.Code == coupons.Code);
               

                if (existingcoupon != null)
                {
                    ModelState.AddModelError("Code", "Coupon code already exists.");
                    return View(coupons); // Return the view with the error message
                }
                if (coupons.DiscountAmount > coupons.MinimumAmount)
                {
                    ModelState.AddModelError("DiscountAmount", "DiscountAmount should be less than the minimum amount.");
                    return View(coupons); // Return the view with the error message


                }
                if (coupons.MinimumAmount < 250)
                {
                    ModelState.AddModelError("MinimumAmount", "Minimum amount should be greater than 250");
                    return View(coupons);
                }
                if (coupons.DiscountPercentage>99)

                {
                    ModelState.AddModelError("DiscountPercentage", "Maximum discount percentage is 99%");
                    return View(coupons);
                }
                if(coupons.MinimumAmount - coupons.DiscountAmount < 250)
                {
                    ModelState.AddModelError("DiscountAmount", "There should be minimum 250 RS difference between the minimum amount and coupon amount");
                    return View(coupons);
                }
                context.Coupons.Add(coupons);
                context.Save();
                TempData["Success"] = "Coupon Created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        /*------Edit--------*/

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Coupons CouponList = context.Coupons.Get(u => u.CID == id);
            if (CouponList == null)
            {
                return NotFound();
            }
            return View(CouponList);
        }

        [HttpPost]
        public IActionResult Edit(Coupons coupons)
        {
            if (ModelState.IsValid)
            {
                context.Coupons.Update(coupons);
                context.Save();
                TempData["Success"] = "Coupon Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }




        #region API calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Coupons> CouponList = context.Coupons.GetAll().ToList();
            return Json(new { data = CouponList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var couponToBeDeleted = context.Coupons.Get(u => u.CID == id);

            if (couponToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            context.Coupons.Remove(couponToBeDeleted);
            context.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion


    }
}