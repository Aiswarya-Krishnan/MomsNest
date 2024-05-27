using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using MomsNest.DataAccess.Repository;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using System.Linq;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =StatDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork context;

        public CategoryController(IUnitOfWork context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = context.Category.GetAll().ToList();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and Display Order Should not be same");
            }


            if (ModelState.IsValid)
            {
                var existingCategory = context.Category.Get(c => c.Name == category.Name);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "Category name already exists.");
                    return View(category); // Return the view with the error message
                }
                var existingDisplay = context.Category.Get(c => c.DisplayOrder == category.DisplayOrder);
                if (existingDisplay != null)
                {
                    ModelState.AddModelError("DisplayOrder", "Display Order number already exists.");
                    return View(category); // Return the view with the error message
                }
                context.Category.Add(category);
                context.Save();
                TempData["Success"] = "Category Created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        /*----Edit------*/
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category CategoryL = context.Category.Get(u => u.CategoryId == id);
            if (CategoryL == null)
            {
                return NotFound();
            }
            return View(CategoryL);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check if the category name or display order has been changed
                var existingCategory = context.Category.Get(c => c.CategoryId != category.CategoryId && c.Name == category.Name);
                var existingDisplay = context.Category.Get(c => c.CategoryId != category.CategoryId && c.DisplayOrder == category.DisplayOrder);

                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "Category name already exists.");
                    return View(category); // Return the view with the error message
                }

                if (existingDisplay != null)
                {
                    ModelState.AddModelError("DisplayOrder", "Display Order number already exists.");
                    return View(category); // Return the view with the error message
                }

                // Update the category
                context.Category.Update(category);
                context.Save();
                TempData["Success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        #region API calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Category> CategoryList = context.Category.GetAll().ToList(); 

            return Json(new { data = CategoryList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var categoryToBeDeleted = context.Category.Get(u => u.CategoryId == id);

            if (categoryToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            context.Category.Remove(categoryToBeDeleted);
            context.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion





    }
}