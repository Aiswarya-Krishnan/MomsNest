using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MomsNest.Data;
using MomsNest.Models;

namespace MomsNest.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext context;

        public CategoryController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = context.Categories.ToList();
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
                context.Categories.Add(category);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
                return View();
        }
    }
}