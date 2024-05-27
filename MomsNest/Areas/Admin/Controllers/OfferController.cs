using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using Stripe;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = StatDetails.Role_Admin)]
    public class OfferController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OfferViewModel offerViewModel { get; set; }

        public OfferController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }
        public IActionResult Index()
        {
            List<Offer> offers = unitOfWork.Offer.GetAll().ToList();
            return View(offers);
        }
        public IActionResult Create()
        {
            var categories = unitOfWork.Category.GetAll().ToList();
            var products = unitOfWork.Product.GetAll().ToList();


            var viewModel = new OfferViewModel
            {
                Categories = categories,
                Products = products
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Create(OfferViewModel viewModel)
        {

            if (OfferExists(viewModel))
            {
                ModelState.AddModelError("OfferItem", "An offer for this category or product already exists.");
                viewModel.Categories = unitOfWork.Category.GetAll().ToList();
                viewModel.Products = unitOfWork.Product.GetAll().ToList();
                return View(viewModel);
            }

            // Create a new Offer object
            Offer offer = new Offer()
            {
                OfferName = viewModel.Offer.OfferName,
                Offertype = viewModel.Offer.Offertype,
                OfferDiscount = viewModel.Offer.OfferDiscount,
                OfferDescription = viewModel.Offer.OfferDescription,
                // Assign the value of OfferItem based on the selected offer type
                OfferItem = GetOfferItemName(viewModel.Offer.Offertype, viewModel.SelectedCategoryId, viewModel.SelectedProductId)

            };

            // Check if the offer already exists
            var existingOffer = unitOfWork.Offer.Get(c => c.OfferName == viewModel.Offer.OfferName);

            if (existingOffer != null)
            {
                ModelState.AddModelError("Offer.OfferName", "Offer already exists.");
                viewModel.Categories = unitOfWork.Category.GetAll().ToList();
                viewModel.Products = unitOfWork.Product.GetAll().ToList();
                return View(viewModel);
            }

            // Add the new offer to the database
            unitOfWork.Offer.Add(offer);
            unitOfWork.Save();
            if (viewModel.Offer.Offertype == Offer.OfferType.Category)
            {
                var productsInCategory = unitOfWork.Product.GetAll()
                    .Where(p => p.CategoryID == viewModel.SelectedCategoryId).ToList();

                foreach (var product in productsInCategory)
                {
                    product.Discount = viewModel.Offer.OfferDiscount;
                    unitOfWork.Product.Update(product);
                }
            }
            else if (viewModel.Offer.Offertype == Offer.OfferType.Product)
            {
                var product = unitOfWork.Product.Get(p => p.ProductId == viewModel.SelectedProductId);
                if (product != null)
                {
                    product.Discount = viewModel.Offer.OfferDiscount;
                    unitOfWork.Product.Update(product);
                }

            }
            unitOfWork.Save();

            TempData["Success"] = "Offer Created successfully";
            return RedirectToAction("Index");
        }





        private string GetOfferItemName(Offer.OfferType offerType, int? categoryId, int? productId)
        {
            if (offerType == Offer.OfferType.Category)
            {
                var category = unitOfWork.Category.Get(c => c.CategoryId == categoryId);
                return category != null ? category.Name : "";
            }
            else if (offerType == Offer.OfferType.Product)
            {
                var product = unitOfWork.Product.Get(p => p.ProductId == productId);
                return product != null ? product.ProductName : "";
            }
            else
            {
                return ""; // Handle other offer types if necessary
            }
        }

        private bool OfferExists(OfferViewModel viewModel)
        {
            var existingOffer = unitOfWork.Offer.Get(
                o => o.OfferItem == GetOfferItemName(viewModel.Offer.Offertype, viewModel.SelectedCategoryId, viewModel.SelectedProductId));

            return existingOffer != null;
        }




        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            // Fetch the offer from the database based on the provided ID
            Offer offer = unitOfWork.Offer.Get(u => u.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            // Retrieve categories and products for dropdown selection
            var categories = unitOfWork.Category.GetAll().ToList();
            var products = unitOfWork.Product.GetAll().ToList();

            // Create view model with offer details and available categories/products
            var viewModel = new OfferViewModel
            {
                Offer = offer,
                Categories = categories,
                Products = products
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(OfferViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var previousOffer = unitOfWork.Offer.Get(u => u.OfferId == viewModel.Offer.OfferId);

                // Update the offer details
                unitOfWork.Offer.Update(viewModel.Offer);
                unitOfWork.Save();

                if (previousOffer.OfferDiscount != viewModel.Offer.OfferDiscount)
                {
                    // If the discount amount has changed, update affected products
                    UpdateAffectedProductsDiscount(previousOffer, viewModel.Offer);
                }
                TempData["Success"] = "Offer Updated successfully";
                return RedirectToAction("Index");
            }

            viewModel.Categories = unitOfWork.Category.GetAll().ToList();
            viewModel.Products = unitOfWork.Product.GetAll().ToList();

            return View(viewModel);
        }


        private void UpdateAffectedProductsDiscount(Offer previousOffer, Offer updatedOffer)
        {
            // Get the affected products based on the offer type and item
            List<Models.Product> affectedProducts = new List<Models.Product>();
            if (updatedOffer.Offertype == Offer.OfferType.Category)
            {
                affectedProducts = unitOfWork.Product.GetAll()
                    .Where(p => p.Category.Name == updatedOffer.OfferItem)
                    .ToList();
            }
            else if (updatedOffer.Offertype == Offer.OfferType.Product)
            {
                var product = unitOfWork.Product.Get(p => p.ProductName == updatedOffer.OfferItem);
                if (product != null)
                {
                    affectedProducts.Add(product);
                }
            }

            // Update the discount field accordingly for each affected product
            foreach (var product in affectedProducts)
            {
                product.Discount = updatedOffer.OfferDiscount;
                unitOfWork.Product.Update(product);
            }

            unitOfWork.Save();
        }


        #region API calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Offer> offerList = unitOfWork.Offer.GetAll().ToList();
            return Json(new { data = offerList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var offerToBeDeleted = unitOfWork.Offer.Get(u => u.OfferId == id);

            if (offerToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // Get the products affected by the offer deletion
            var affectedProducts = new List<Models.Product>();

            if (offerToBeDeleted.Offertype == Offer.OfferType.Category)
            {
                // If offer type is Category, find products in that category
                affectedProducts = unitOfWork.Product.GetAll(includeProperties: "Category")
                                                                .Where(p => p.Category.Name == offerToBeDeleted.OfferItem)
                                                                .ToList();
            }
            else if (offerToBeDeleted.Offertype == Offer.OfferType.Product)
            {
                // If offer type is Product, find the specific product
                var product = unitOfWork.Product.Get(p => p.ProductName == offerToBeDeleted.OfferItem);
                if (product != null)
                {
                    affectedProducts.Add(product);
                }
            }

            // Update the discount field accordingly
            foreach (var product in affectedProducts)
            {
                product.Discount -= offerToBeDeleted.OfferDiscount;
                unitOfWork.Product.Update(product);
            }

            unitOfWork.Offer.Remove(offerToBeDeleted);
            unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion



    }
}

