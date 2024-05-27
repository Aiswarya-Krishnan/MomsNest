using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MomsNest.DataAccess.Migrations;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Utilities;
using Microsoft.AspNetCore.Http;
using Offer = MomsNest.Models.Offer;
using utilities;
using Stripe;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MomsNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ShoppingCartController> _logger;
        [BindProperty]
        public ShoppingCartViewModel shoppingCartViewModel { get; set; }

        public Coupons coupons { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork, ILogger<ShoppingCartController> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                shoppingCartViewModel = new()
                {
                    ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.UserID == userId, includeProperties: "Product"),
                    OrderHeader = new()
                };
                IEnumerable<ProductImage> productImages = unitOfWork.ProductImages.GetAll();
                foreach (var pric in shoppingCartViewModel.ShoppingCartList)
                {
                    pric.Product.ProductImages = productImages.Where(u => u.ProductId == pric.Product_ID).ToList();
                    pric.Price = GetPrice(pric);
                    shoppingCartViewModel.OrderHeader.OrderTotal += (pric.Price * pric.Count);
                }

                return View(shoppingCartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the shopping cart.");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public IActionResult Plus(int cartid)
        {
            try
            {
                var cart = unitOfWork.ShoppingCart.Get(u => u.ShopingcartId == cartid);
                cart.Count += 1;
                unitOfWork.ShoppingCart.Update(cart);
                unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while incrementing the cart item count.");
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Minus(int cartid)
        {
            try
            {
                var cart = unitOfWork.ShoppingCart.Get(u => u.ShopingcartId == cartid);
                if (cart.Count <= 1)
                {
                    unitOfWork.ShoppingCart.Remove(cart);
                }
                else
                {
                    cart.Count -= 1;
                    unitOfWork.ShoppingCart.Update(cart);
                }

                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while decrementing the cart item count.");
                return RedirectToAction(nameof(Index));
            }

        }

        public IActionResult Remove(int cartid)
        {
            try
            {
                var cart = unitOfWork.ShoppingCart.Get(u => u.ShopingcartId == cartid);

                unitOfWork.ShoppingCart.Remove(cart);
                unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while remove the cart item.");
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult OrderSummary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Fetch the ApplicationUser including the WalletAmount
                var applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);

                var shoppingCartItems = unitOfWork.ShoppingCart.GetAll(
                    u => u.UserID == userId,
                    includeProperties: "Product,Product.Category"
                );

                var offers = unitOfWork.Offer.GetAll();
                var validShoppingCartItems = new List<ShoppingCart>();
                foreach (var cartItem in shoppingCartItems)
                {
                    if (cartItem.Product.StockQuantity >= cartItem.Count)
                    {
                        validShoppingCartItems.Add(cartItem);
                    }
                }

                var previousAddresses = unitOfWork.OrderHeader
                   .GetAll(o => o.AppUser_Id == userId)
                   .Select(o => new SelectListItem
                   {
                       Text = $"{o.Name} , {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode} , {o.PhoneNumber} ",
                       Value = o.StreetAddress // You can set this to any unique identifier of the address if needed
                   })
                   .Distinct() // Remove duplicates
                   .ToList();

                ViewBag.PreviousAddresses = previousAddresses;
                var coupon = unitOfWork.Coupons.GetAll();

                var usedCoupons = unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUser.Id == userId && u.CouponCode != null)
                    .Select(u => u.CouponCode)
                    .ToHashSet();

                var validCoupons = coupon.Where(c => !usedCoupons.Contains(c.Code)).ToList();




                var shoppingCartViewModel = new ShoppingCartViewModel
                {
                    ShoppingCartList = validShoppingCartItems,
                    OrderHeader = new OrderHeader(),
                    Coupons = validCoupons.ToList(),
                    Offers = offers.ToList(),
                };

                // Set OrderHeader properties from ApplicationUser
                shoppingCartViewModel.OrderHeader.ApplicationUser = applicationUser;
                shoppingCartViewModel.OrderHeader.Name = applicationUser.Name;
                shoppingCartViewModel.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
                shoppingCartViewModel.OrderHeader.StreetAddress = applicationUser.StreetAddress;
                shoppingCartViewModel.OrderHeader.City = applicationUser.City;
                shoppingCartViewModel.OrderHeader.State = applicationUser.State;
                shoppingCartViewModel.OrderHeader.PostalCode = applicationUser.PostCode;

                // Calculate Order Total
                foreach (var pric in shoppingCartViewModel.ShoppingCartList)
                {
                    if (pric.Product.StockQuantity > 0)
                    {
                        string categoryName = pric.Product.Category?.Name;
                        pric.Price = GetPrice(pric);

                        var applicableOffer = shoppingCartViewModel.Offers.FirstOrDefault(o =>
                            (o.Offertype == Offer.OfferType.Product && o.OfferItem == pric.Product.ProductName) ||
                            (o.Offertype == Offer.OfferType.Category && o.OfferItem == categoryName)
                        );
                        if (applicableOffer != null)
                        {
                            decimal discountAmount = (decimal)applicableOffer.OfferDiscount;
                            shoppingCartViewModel.OrderHeader.OrderTotal -= discountAmount; // Subtract discount amount from order total
                        }

                        shoppingCartViewModel.OrderHeader.OrderTotal += (pric.Price * pric.Count);
                    }
                }

                // Check and deduct from wallet if necessary
                if (applicationUser.wallet <= shoppingCartViewModel.OrderHeader.OrderTotal)
                {
                    shoppingCartViewModel.OrderHeader.OrderTotal -= (decimal)applicationUser.wallet;

                }
                else
                {

                    shoppingCartViewModel.OrderHeader.OrderTotal = 0;

                }


                // Save changes to the database
                unitOfWork.Save();

                HttpContext.Session.SetObject("ShoppingCartViewModel", shoppingCartViewModel);
                return View(shoppingCartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the order summary.");
                return RedirectToAction(nameof(Index));
            }
        }





        [HttpPost]
        [ActionName("OrderSummary")]
        public IActionResult OrderSummaryPost(string paymentMethod)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                shoppingCartViewModel.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.UserID == userId, includeProperties: "Product");

                shoppingCartViewModel.OrderHeader.OrderDate = System.DateTime.Now;
                shoppingCartViewModel.OrderHeader.AppUser_Id = userId;
                ApplicationUser applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);
                var offers = unitOfWork.Offer.GetAll();
                var coupons = unitOfWork.Coupons.GetAll();
                string appliedOfferItem = null;
                // Create a list to store successfully ordered items
                var orderedItems = new List<ShoppingCart>();

                // Initialize total order amount
                decimal orderTotal = 0;
                shoppingCartViewModel = HttpContext.Session.GetObject<ShoppingCartViewModel>("ShoppingCartViewModel");
                shoppingCartViewModel.OrderHeader.PaymentMethod = paymentMethod;
                string appliedCouponCode = shoppingCartViewModel.OrderHeader.CouponCode;
                decimal discount = 0;
                // If a coupon code is applied, calculate and apply the discount
                if (!string.IsNullOrEmpty(appliedCouponCode))
                {
                    var coupon = unitOfWork.Coupons.Get(c => c.Code == appliedCouponCode);

                    if (coupon != null)
                    {
                        if (coupon.Type == Coupons.DiscountType.DiscountPercentage)
                        {
                            discount = (decimal)(shoppingCartViewModel.OrderHeader.OrderTotal * coupon.DiscountPercentage / 100);
                        }
                        else if (coupon.Type == Coupons.DiscountType.DiscountAmount)
                        {
                            discount = (decimal)coupon.DiscountAmount;
                        }


                    }
                }

                foreach (var cartItem in shoppingCartViewModel.ShoppingCartList)
                {
                    // Initialize discount and order total for each item
                    decimal discountAmount = 0;
                    decimal itemTotal = 0;

                    var product = unitOfWork.Product.Get(
                        p => p.ProductId == cartItem.Product_ID,
                        includeProperties: "Category"
                    );

                    if (product != null && product.StockQuantity >= cartItem.Count)
                    {
                        string categoryName = product.Category.Name;
                        cartItem.Price = GetPrice(cartItem);

                        var productOffer = offers.FirstOrDefault(o => o.Offertype == Models.Offer.OfferType.Product && o.OfferItem == product.ProductName);
                        if (productOffer != null)
                        {
                            // Apply offer discount based on offer type
                            if (productOffer.Offertype == Models.Offer.OfferType.Product)
                            {
                                discountAmount += (decimal)productOffer.OfferDiscount;
                                appliedOfferItem = productOffer.OfferItem;
                            }
                        }

                        var categoryOffer = offers.FirstOrDefault(o => o.Offertype == Models.Offer.OfferType.Category && o.OfferItem == product.Category.Name);
                        if (categoryOffer != null)
                        {
                            // Apply offer discount based on offer type
                            if (categoryOffer.Offertype == Offer.OfferType.Category)
                            {
                                discountAmount += (decimal)categoryOffer.OfferDiscount;
                                appliedOfferItem = categoryOffer.OfferItem;
                            }
                        }

                        // Calculate total price for the current item after applying discounts
                        itemTotal = ((cartItem.Price * cartItem.Count) - discountAmount);

                        // Accumulate the item total to the overall order total
                        orderTotal += itemTotal;

                        // Reduce the stock quantity only if there is sufficient stock
                        product.StockQuantity -= cartItem.Count;
                        unitOfWork.Product.Update(product);

                        // Add the item to the list of successfully ordered items
                        orderedItems.Add(cartItem);
                        var entry = unitOfWork.Context.Entry(cartItem);
                        if (entry.State != EntityState.Detached)
                        {
                            entry.State = EntityState.Detached;
                        }

                    }
                }

                if (applicationUser != null)
                {
                    if (applicationUser.wallet <= orderTotal)
                    {
                        // Deduct the order total from the wallet amount
                        orderTotal -= (decimal)applicationUser.wallet;
                        shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusApproved;
                        shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusApproved;
                    }
                    else
                    {
                        orderTotal = 0;
                        shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusPending;
                        shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusPending;
                    }



                    orderTotal -= discount;
                    shoppingCartViewModel.OrderHeader.OrderTotal = orderTotal;
                    shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Today;
                    shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusPending;
                    shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusPending;

                    unitOfWork.OrderHeader.Update(shoppingCartViewModel.OrderHeader);
                    unitOfWork.Save();
                    foreach (var cart in orderedItems)
                    {
                        // Add order details only for products with sufficient stock quantity
                        OrderDetails orderDetails = new()
                        {
                            Product_ID = cart.Product_ID,
                            OrderHeader_ID = shoppingCartViewModel.OrderHeader.OrderHeaderId,
                            Price = cart.Price,
                            Count = cart.Count,
                        };
                        unitOfWork.OrderDetails.Add(orderDetails);
                    }

                    unitOfWork.Save();

                    if (orderTotal == 0)
                    {
                        shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusApproved;
                        shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusApproved;

                        unitOfWork.OrderHeader.Update(shoppingCartViewModel.OrderHeader);
                        unitOfWork.Save();


                        return RedirectToAction(nameof(OrderConfirmation), new { orderId = shoppingCartViewModel.OrderHeader.OrderHeaderId });
                    }

                    else if (shoppingCartViewModel.OrderHeader.PaymentMethod == "CashOnDelivery")
                    {
                        shoppingCartViewModel.OrderHeader.PaymentStatus = StatDetails.PaymentStatusPending;
                        shoppingCartViewModel.OrderHeader.OrderStatus = StatDetails.StatusApproved;

                        unitOfWork.OrderHeader.Update(shoppingCartViewModel.OrderHeader);

                        unitOfWork.Save();


                        return RedirectToAction(nameof(OrderConfirmation), new { orderId = shoppingCartViewModel.OrderHeader.OrderHeaderId });

                    }
                    else
                    {


                        var domain = "https://localhost:7257/";
                        // Check if the payment process is ongoing from the order summary page
                        bool isPaymentInProgress = HttpContext.Session.GetString("PaymentInProgress") == "true";
                        if (isPaymentInProgress)
                        {
                            // If payment is in progress, reset the session variable and redirect to the order summary page
                            HttpContext.Session.Remove("PaymentInProgress");
                            return RedirectToAction(nameof(OrderSummary));
                        }
                        var options = new SessionCreateOptions
                        {
                            SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?orderId={shoppingCartViewModel.OrderHeader.OrderHeaderId}",
                            CancelUrl = domain + "Customer/ShoppingCart/Index",
                            LineItems = new List<SessionLineItemOptions>(),
                            Mode = "payment",

                        };



                        foreach (var item in shoppingCartViewModel.ShoppingCartList)
                        {
                            decimal itemTotal = item.Price;

                            if (applicationUser.wallet <= itemTotal)
                            {

                                itemTotal -= (decimal)applicationUser.wallet;
                                applicationUser.wallet = 0;

                            }
                            else
                            {
                                itemTotal = 0;

                            }


                            decimal discountAmount = 0;


                            var product = unitOfWork.Product.Get(
                                p => p.ProductId == item.Product_ID,
                                includeProperties: "Category"
                            );

                            if (product != null && product.StockQuantity >= item.Count)
                            {
                                string categoryName = product.Category.Name;
                                item.Price = GetPrice(item);

                                var productOffer = offers.FirstOrDefault(o => o.Offertype == Models.Offer.OfferType.Product && o.OfferItem == product.ProductName);
                                if (productOffer != null)
                                {
                                    // Apply offer discount based on offer type
                                    if (productOffer.Offertype == Models.Offer.OfferType.Product)
                                    {
                                        discountAmount += (decimal)productOffer.OfferDiscount;

                                    }
                                }

                                var categoryOffer = offers.FirstOrDefault(o => o.Offertype == Models.Offer.OfferType.Category && o.OfferItem == product.Category.Name);
                                if (categoryOffer != null)
                                {
                                    // Apply offer discount based on offer type
                                    if (categoryOffer.Offertype == Offer.OfferType.Category)
                                    {
                                        discountAmount += (decimal)categoryOffer.OfferDiscount;

                                    }
                                }

                                itemTotal -= discountAmount;
                            }
                            if (discount != 0 && itemTotal > discount)
                            {
                                itemTotal -= discount;
                                discount = 0;
                            }


                            var sessionLineItems = new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long)(itemTotal * 100), // Use discounted price as unit amount
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = item.Product.ProductName
                                    }
                                },
                                Quantity = item.Count
                            };

                            options.LineItems.Add(sessionLineItems);
                        }

                        // Create payment session
                        var service = new SessionService();
                        Session session = service.Create(options);

                        unitOfWork.OrderHeader.UpdateStripePaymentID(shoppingCartViewModel.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                        unitOfWork.Save();

                        // Redirect to the payment gateway
                        Response.Headers.Add("Location", session.Url);
                        return new StatusCodeResult(303);
                    }





                    // Remove items from shopping cart
                    foreach (var cartItem in shoppingCartViewModel.ShoppingCartList)
                    {
                        unitOfWork.ShoppingCart.Remove(cartItem);
                    }
                }

                return RedirectToAction(nameof(OrderConfirmation), new { orderId = shoppingCartViewModel.OrderHeader.OrderHeaderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while placing the order.");
                return RedirectToAction(nameof(Index));
            }
        }





        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {

            shoppingCartViewModel = HttpContext.Session.GetObject<ShoppingCartViewModel>("ShoppingCartViewModel");


            // Ensure shoppingCartViewModel is not null
            if (shoppingCartViewModel == null || shoppingCartViewModel.OrderHeader == null)
            {
                // Handle the null reference gracefully
                return BadRequest("Shopping cart view model or order header is not initialized.");
            }

            var coupon = unitOfWork.Coupons.Get(c => c.Code == couponCode);
            if (coupon == null)
            {

                return BadRequest("Invalid coupon code");
            }
            if (shoppingCartViewModel.OrderHeader.CouponCode == couponCode)
            {
                return BadRequest("Coupon already applied");
            }


            decimal discount = 0;
            if (shoppingCartViewModel.OrderHeader.CouponCode == null)
            {
                if (coupon.Type == Coupons.DiscountType.DiscountPercentage)
                {
                    // Ensure proper type conversion by explicitly casting to decimal
                    discount = (decimal)(shoppingCartViewModel.OrderHeader.OrderTotal * coupon.DiscountPercentage / 100);
                }
                else if (coupon.Type == Coupons.DiscountType.DiscountAmount)
                {
                    // Ensure proper type conversion by explicitly casting to decimal
                    discount = (decimal)coupon.DiscountAmount;
                }

                // Ensure OrderTotal is not null before deducting the discount
                if (shoppingCartViewModel.OrderHeader.OrderTotal != null)
                {
                    // Update the order total by deducting the discount
                    shoppingCartViewModel.OrderHeader.OrderTotal -= discount;
                }
                else
                {
                    // Handle the case where OrderTotal is null
                    return BadRequest("Order total is null");
                }

                shoppingCartViewModel.OrderHeader.CouponCode = couponCode;
            }

            HttpContext.Session.SetObject("ShoppingCartViewModel", shoppingCartViewModel);
            // Return the updated order total back to the client
            return Json(new { newOrderTotal = shoppingCartViewModel.OrderHeader.OrderTotal });
        }

        [HttpPost]
        public IActionResult RemoveCoupon(string couponCode)
        {

            shoppingCartViewModel = HttpContext.Session.GetObject<ShoppingCartViewModel>("ShoppingCartViewModel");
            if (shoppingCartViewModel.OrderHeader.CouponCode != null)
            {
                var appliedCoupon = unitOfWork.Coupons.Get(u => u.Code == shoppingCartViewModel.OrderHeader.CouponCode);
                if (appliedCoupon != null)
                {
                    if (appliedCoupon.Type == Coupons.DiscountType.DiscountAmount)
                    {
                        shoppingCartViewModel.OrderHeader.OrderTotal += (decimal)appliedCoupon.DiscountAmount;
                    }
                    if (appliedCoupon.Type == Coupons.DiscountType.DiscountPercentage)
                    {
                        decimal discountAmount = shoppingCartViewModel.OrderHeader.OrderTotal * (decimal)appliedCoupon.DiscountPercentage / (100 - (decimal)appliedCoupon.DiscountPercentage);
                        shoppingCartViewModel.OrderHeader.OrderTotal += discountAmount;
                    }

                }

                shoppingCartViewModel.OrderHeader.CouponCode = null;
            }
            else
            {
                return BadRequest("Coupon is not applied");
            }


            HttpContext.Session.SetObject("ShoppingCartViewModel", shoppingCartViewModel);

            return Json(new { newOrderTotal = shoppingCartViewModel.OrderHeader.OrderTotal });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            try
            {
                OrderHeader orderHeader = unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderId, includeProperties: "ApplicationUser");
                List<OrderDetails> orderDetailsList = unitOfWork.OrderDetails.GetAll(u => u.OrderHeader_ID == orderId).ToList();

                decimal totalPrice = 0;

                foreach (var orderDetail in orderDetailsList)
                {
                    totalPrice += orderDetail.Price * orderDetail.Count;
                }

                if (orderHeader.OrderTotal == 0)
                {
                    // Update order status directly without involving Stripe
                    unitOfWork.OrderHeader.UpdateStatus(orderId, StatDetails.StatusApproved, StatDetails.PaymentStatusApproved);
                    unitOfWork.Save();

                    // Update user wallet if applicable
                    if (orderHeader.ApplicationUser != null)
                    {


                        if (orderHeader.ApplicationUser.wallet <= totalPrice)
                        {
                            orderHeader.ApplicationUser.wallet = 0;
                        }
                        else
                        {
                            orderHeader.ApplicationUser.wallet -= totalPrice;
                        }

                        unitOfWork.ApplicationUser.Update(orderHeader.ApplicationUser);
                        unitOfWork.Save();
                    }

                    // Remove shopping cart items
                    List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.UserID == orderHeader.AppUser_Id).ToList();
                    unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                    unitOfWork.Save();

                    return View(orderId);
                }
                else if (orderHeader.PaymentMethod == "CashOnDelivery")
                {
                    unitOfWork.OrderHeader.UpdateStatus(orderId, StatDetails.StatusApproved, StatDetails.PaymentStatusPending);
                    unitOfWork.Save();

                    // Update user wallet if applicable
                    if (orderHeader.ApplicationUser != null)
                    {


                        if (orderHeader.ApplicationUser.wallet <= totalPrice)
                        {
                            orderHeader.ApplicationUser.wallet = 0;
                        }
                        else
                        {
                            orderHeader.ApplicationUser.wallet -= totalPrice;
                        }

                        unitOfWork.ApplicationUser.Update(orderHeader.ApplicationUser);
                        unitOfWork.Save();
                    }
                    List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.UserID == orderHeader.AppUser_Id).ToList();
                    unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                    unitOfWork.Save();

                    return View(orderId);
                }
                else
                {


                    if (orderHeader.PaymentStatus != StatDetails.PaymentStatusDelayedPayment)
                    {
                        //this order of cs
                        var service = new SessionService();
                        Session session = service.Get(orderHeader.SessionId);
                        if (session.PaymentStatus.ToLower() == "paid")
                        {
                            unitOfWork.OrderHeader.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
                            unitOfWork.OrderHeader.UpdateStatus(orderId, StatDetails.StatusApproved, StatDetails.PaymentStatusApproved);
                            unitOfWork.Save();
                        }
                        if (orderHeader.ApplicationUser != null)
                        {
                            if (orderHeader.ApplicationUser.wallet <= orderHeader.OrderTotal)
                            {
                                orderHeader.ApplicationUser.wallet = 0;
                            }
                            else
                            {
                                orderHeader.ApplicationUser.wallet -= orderHeader.OrderTotal;
                            }

                            unitOfWork.ApplicationUser.Update(orderHeader.ApplicationUser);
                            unitOfWork.Save();

                        }
                    }

                    List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.
                        GetAll(u => u.UserID == orderHeader.AppUser_Id).ToList();
                    unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                    unitOfWork.Save();

                    return View(orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Confirming the order.");
                return RedirectToAction(nameof(Index));
            }
        }

        private decimal GetPrice(ShoppingCart shopping)
        {

            return shopping.Product.Price;
        }


    }
}
