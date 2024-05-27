using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using MomsNest.Models.ViewModels;
using System.Security.Claims;
using Utilities;

namespace MomsNest.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = StatDetails.Role_Admin)]
    public class UserController : Controller
    {
        private readonly AppDbContext context;
        private readonly IUnitOfWork unitOfWork;

        public UserController(AppDbContext context, IUnitOfWork unitOfWork)
        {
            this.context = context;
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(string id)
        {
            ApplicationUser application = unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (application == null)
            {
                return NotFound(); // Or handle the situation appropriately
            }
            return View(application);
        }
        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = true;
            await context.SaveChangesAsync();
            TempData["Success"] = "User Blocked successfully";


            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = false;
            await context.SaveChangesAsync();
            TempData["Success"] = "User Unblocked successfully";


            return RedirectToAction("Index");
        }


        #region API calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> UserList = context.ApplicationUsers.ToList();
            return Json(new { data = UserList });
        }


        #endregion
    }
}
