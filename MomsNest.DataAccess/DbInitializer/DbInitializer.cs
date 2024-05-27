using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace MomsNest.DataAccess.DbInitializer
{
    public class DbInitializer:IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }


            if (!_roleManager.RoleExistsAsync(StatDetails.Role_User).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StatDetails.Role_User)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StatDetails.Role_Admin)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "aiswaryakaishu98@gmail.com",
                    Email = "aiswaryakaishu98@gmail.com",
                    Name = "Aiswarya",
                    PhoneNumber = "7510709391",
                    StreetAddress = "Assaye Line",
                    State = "Karnataka",
                    PostCode = "23422",
                    City = "Bangalore"
                }, "Aiswarya@123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "aiswaryakaishu98@gmail.com");
                _userManager.AddToRoleAsync(user,StatDetails.Role_Admin).GetAwaiter().GetResult();
            }

            return;

        }

    }
}
