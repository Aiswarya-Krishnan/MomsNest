using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using System.Collections.Generic;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]

        [Authorize(Roles = StatDetails.Role_Admin)]
    public class ProductController : Controller
        {
            private readonly IUnitOfWork context;
            private readonly IWebHostEnvironment webHostEnvironment;

            public ProductController(IUnitOfWork context,IWebHostEnvironment webHostEnvironment)
            {
                this.context = context;
                this.webHostEnvironment = webHostEnvironment;
            }
            public IActionResult Index()
            {
              List<Product>PrductList = context.Product.GetAll().ToList();
            
                return View(PrductList);
            }

        /*--- Create----*/

        public IActionResult Upsert(int? id)
        {
            
            ProductViewModel productViewModel = new()
            {
                CategoryList = context.Category.GetAll().Select
                (u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                }),
            Product=new Product()

            };
            if (id == null || id == 0)
            {

                return View(productViewModel);
            }
            else
            {
                productViewModel.Product =context.Product.Get(u => u.ProductId == id);
                return View(productViewModel);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string ProductPath = Path.Combine(wwwRootPath, @"Images\Product");
                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //delete old image
                        var oldimage=Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldimage))
                        {
                            System.IO.File.Delete(oldimage);
                        }
                    }
                    using(var filestream=new FileStream(Path.Combine(ProductPath, filename),FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    obj.Product.ImageUrl = @"\Images\Product\" + filename;
                }



                if (obj.Product.ProductId == 0)
                {

                    context.Product.Add(obj.Product);
                }
                else
                {

                    context.Product.Update(obj.Product);
                }

                context.Save();
                TempData["Success"] = "Product Created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                
                    obj.CategoryList = context.Category.GetAll().Select
                (u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });

                return View(obj);

            }
        }


        /*-----Edit-----*/
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product ProductL = context.Product.Get(u => u.ProductId == id);
        //    if (ProductL == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(ProductL);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        context.Product.Update(product);
        //        context.Save();
        //        TempData["Success"] = "Product Updated successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        /*-----Delete-----*/
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product ProductL = context.Product.Get(u => u.ProductId == id);
            if (ProductL == null)
            {
                return NotFound();
            }
            return View(ProductL);

        }
        [HttpPost, ActionName("Delete")]

        public IActionResult DeletePost(int? id)
        {
            Product product = context.Product.Get(u => u.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            context.Product.Remove(product);
            context.Save();
            TempData["Success"] = "Product Deleted successfully";
            return RedirectToAction("Index");
        }


    }
}
