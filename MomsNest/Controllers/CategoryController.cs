using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

using MomsNest.DataAccess.Data;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using System.Linq;

namespace MomsNest.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository context;

        public CategoryController(ICategoryRepository context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = context.GetAll().ToList();
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
                ModelState.AddModelError("Name","Name and Display Order Should not be same");
            }
           

            if (ModelState.IsValid)
            {
                context.Add(category);
                context.Save();
                TempData["Success"] = "Category Created successfully";
                return RedirectToAction("Index");
            }
                return View();
        }

        /*----Edit------*/
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Category CategoryL = context.Get(u=>u.CategoryId == id);
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
                context.Update(category);
                context.Save();
                TempData["Success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category CategoryL = context.Get(u => u.CategoryId == id);
            if (CategoryL == null)
            {
                return NotFound();
            }
            return View(CategoryL);

        }
        [HttpPost,ActionName("Delete")]

        public IActionResult DeletePost(int? id)
        {
            Category category=context.Get(u => u.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

                context.Remove(category);
                context.Save();
            TempData["Success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
       

        


    }
}