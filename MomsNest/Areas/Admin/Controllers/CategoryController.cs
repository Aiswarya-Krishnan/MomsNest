using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using System.Linq;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                context.Category.Update(category);
                context.Save();
                TempData["Success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        /*-----Delete-----*/
        public IActionResult Delete(int? id)
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
        [HttpPost, ActionName("Delete")]

        public IActionResult DeletePost(int? id)
        {
            Category category = context.Category.Get(u => u.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            context.Category.Remove(category);
            context.Save();
            TempData["Success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }





    }
}