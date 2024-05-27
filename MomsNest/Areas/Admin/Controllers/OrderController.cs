using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MomsNest.DataAccess.Repository;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork context;
        [BindProperty]
        public OrderViewModel orderViewModel { get; set; }

        private readonly IWebHostEnvironment webHostEnvironment;

        public OrderController(IUnitOfWork context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
		{
            return View();
        }
      
        public IActionResult Details(int orderId)
        {
            orderViewModel = new()
            {
                OrderHeader = context.OrderHeader.Get(u => u.OrderHeaderId == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = context.OrderDetails.GetAll(u => u.OrderHeader_ID == orderId, includeProperties: "Product")
               
            };

            return View(orderViewModel);
        }


        [HttpPost]
        [Authorize]
        public IActionResult UpdateDetails()
        {
            var orderHeaderDB = context.OrderHeader.Get(u => u.OrderHeaderId == orderViewModel.OrderHeader.OrderHeaderId);
            orderHeaderDB.Name=orderViewModel.OrderHeader.Name;
            orderHeaderDB.PhoneNumber=orderViewModel.OrderHeader.PhoneNumber;
            orderHeaderDB.StreetAddress=orderViewModel.OrderHeader.StreetAddress;
            orderHeaderDB.City=orderViewModel.OrderHeader.City;
            orderHeaderDB.State = orderViewModel.OrderHeader.State;
            orderHeaderDB.PostalCode = orderViewModel.OrderHeader.PostalCode;

            context.OrderHeader.Update(orderHeaderDB);
            context.Save();

            TempData["Success"] = "Order Details Updated successfully";
            return RedirectToAction(nameof(Details), new {orderId=orderHeaderDB.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = StatDetails.Role_Admin)]
        public IActionResult StartProccessing()
        {
            context.OrderHeader.UpdateStatus(orderViewModel.OrderHeader.OrderHeaderId, StatDetails.StatusInProcess);
            context.Save();
            TempData["success"] = "Order details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.OrderHeader.OrderHeaderId });
        }
        [HttpPost]
        [Authorize(Roles = StatDetails.Role_Admin)]
        public IActionResult ShipOrder()
        {
            var orderHeader = context.OrderHeader.Get(u => u.OrderHeaderId == orderViewModel.OrderHeader.OrderHeaderId);
            orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = StatDetails.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            context.OrderHeader.Update(orderHeader);
            context.Save();

            TempData["success"] = "Order shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.OrderHeader.OrderHeaderId });
        }




        [HttpPost]
        public IActionResult CancelOrder()
        {
            var orderHeader=context.OrderHeader.Get(u=>u.OrderHeaderId==orderViewModel.OrderHeader.OrderHeaderId);
            if (orderHeader != null)
            {
                var AppUser = context.ApplicationUser.Get(u => u.Id == orderHeader.AppUser_Id);

                decimal? currentWalletAmount = AppUser.wallet ?? 0;
                if (orderHeader.PaymentStatus == StatDetails.PaymentStatusApproved)
                {
                    currentWalletAmount += orderHeader.OrderTotal;
                }

                AppUser.wallet = currentWalletAmount;
                context.ApplicationUser.Update(AppUser);
            }
       
            

            context.OrderHeader.UpdateStatus(orderHeader.OrderHeaderId,StatDetails.StatusCancelled,StatDetails.StatusCancelled);
            context.Save();

            var orderDetails = context.OrderDetails.GetAll(u => u.OrderHeader_ID == orderHeader.OrderHeaderId, includeProperties: "Product");

            // Increment stock quantity for each product in the order
            foreach (var orderDetail in orderDetails)
            {
                var product = orderDetail.Product;
                if (product != null)
                {
                    product.StockQuantity += orderDetail.Count;
                    context.Product.Update(product);
                }
            }

            context.Save();


            TempData["Success"] = "Order Cancelled  successfully";
            return RedirectToAction(nameof(Index));

        }



        [Authorize]
        public IActionResult Wallet()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin"); // Check if the user is in the Admin role

            if (userId == null && !isAdmin)
            {
                return RedirectToAction("Index", "Home"); // Redirect to home or any other appropriate action
            }

            List<OrderHeader> cancelledOrders;

            if (isAdmin)
            {
                // Retrieve all cancelled orders for admin
                cancelledOrders = context.OrderHeader
                    .GetAll(u => u.OrderStatus == StatDetails.StatusCancelled, includeProperties: "ApplicationUser")
                    .Where(orderHeader => orderHeader != null) // Filter out null OrderHeader instances
                    .ToList(); // Materialize the query to a list
            }
            else
            {
                // Retrieve cancelled orders for the logged-in user
                cancelledOrders = context.OrderHeader
                    .GetAll(u => u.AppUser_Id == userId && u.OrderStatus == StatDetails.StatusCancelled, includeProperties: "ApplicationUser")
                    .Where(orderHeader => orderHeader != null) // Filter out null OrderHeader instances
                    .ToList(); // Materialize the query to a list
            }

            var cancelledOrderViewModels = cancelledOrders
                .Select(orderHeader => new OrderViewModel
                {
                    OrderHeader = orderHeader,
                    OrderDetails = context.OrderDetails
                        .GetAll(od => od.OrderHeader_ID == orderHeader.OrderHeaderId, includeProperties: "Product")
                        .ToList()
                })
                .ToList();

            return View(cancelledOrderViewModels);
        }



        public IActionResult Invoice(int orderId)
        {
            orderViewModel = new()
            {
                OrderHeader = context.OrderHeader.Get(u => u.OrderHeaderId == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = context.OrderDetails.GetAll(u => u.OrderHeader_ID == orderId, includeProperties: "Product")
            };

            return View(orderViewModel);
        }



        #region API calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if(User.IsInRole(StatDetails.Role_Admin))
             {
                orderHeaders = context.OrderHeader
                                          .GetAll(includeProperties: "ApplicationUser")
                                          .OrderByDescending(o => o.OrderHeaderId)
                                          .ToList();
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = context.OrderHeader
                                          .GetAll(u => u.AppUser_Id == userId, includeProperties: "ApplicationUser")
                                          .OrderByDescending(o => o.OrderHeaderId)
                                          .ToList();
            }


            switch (status)
            {
                case "Processing":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StatDetails.StatusInProcess);
                    break;
                case "Pending":
                    orderHeaders=orderHeaders.Where(u=>u.PaymentStatus==StatDetails.PaymentStatusPending);
                    break;
                case "Shipped":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StatDetails.StatusShipped);
                    break;
                case "Approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StatDetails.StatusApproved);
                    break;
                case "Cancelled":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StatDetails.StatusCancelled);
                    break;
                default:
                    break;
            }



            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
