using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsNest.DataAccess.Repository;
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
            return RedirectToAction(nameof(Details), new {orderid=orderHeaderDB.OrderHeaderId });
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            var orderHeader=context.OrderHeader.Get(u=>u.OrderHeaderId==orderViewModel.OrderHeader.OrderHeaderId);

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

        #region API calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if(User.IsInRole(StatDetails.Role_Admin))
             {
                 orderHeaders= context.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = context.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
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
