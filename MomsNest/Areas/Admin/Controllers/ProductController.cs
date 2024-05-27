using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MomsNest.DataAccess.Migrations;
using MomsNest.DataAccess.Repository;
using MomsNest.DataAccess.Repository.Interfaces;
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
        [BindProperty]
        OrderDetails OrderDetails { get; set; }

        public ProductController(IUnitOfWork context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = context.Product.GetAll(includeProperties: "Category").ToList();

            return View(ProductList);
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
                Product = new Product()

            };
            if (id == null || id == 0)
            {
                //create
                return View(productViewModel);
            }
            else
            {
                //update
                productViewModel.Product = context.Product.Get(u => u.ProductId == id, includeProperties: "ProductImages");
                return View(productViewModel);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {


                if (obj.Product.ProductId == 0)
                {

                    context.Product.Add(obj.Product);
                }
                else
                {

                    context.Product.Update(obj.Product);
                }

                context.Save();
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"Images\Products\product-" + obj.Product.ProductId;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var filestream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = obj.Product.ProductId,
                        };

                        if (obj.Product.ProductImages == null)
                        {
                            obj.Product.ProductImages = new List<ProductImage>();
                        }
                        obj.Product.ProductImages.Add(productImage);

                    }
                    context.Product.Update(obj.Product);
                    context.Save();

                }

                TempData["Success"] = "Product updated successfully";
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

        public IActionResult DeleteImage(int ImageId)
        {
            var imageToBeDeleted = context.ProductImages.Get(u => u.productImageId == ImageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {

                    var OldImagePath =
                         Path.Combine(webHostEnvironment.WebRootPath,
                         imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(OldImagePath))
                    {
                        System.IO.File.Delete(OldImagePath);
                    }

                }
                context.ProductImages.Remove(imageToBeDeleted);
                context.Save();
                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }




        #region API calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> ProductList = context.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = ProductList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = context.Product.Get(u => u.ProductId == id);
           

            if (productToBeDeleted == null)
            {
                return Json(new { sucess = false, message = "Error while deleting" });
            }
            
            string productPath = @"Images\Products\product-" + id;
            string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

            if (!Directory.Exists(finalPath))
            {
                string[] filePath = Directory.GetFiles(finalPath);

                foreach (string filePath2 in filePath)
                {
                    System.IO.File.Delete(filePath2);
                }
                Directory.Delete(finalPath);
            }

            context.Product.Remove(productToBeDeleted);
            context.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion
    }
}