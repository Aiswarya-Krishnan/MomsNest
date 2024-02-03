using Microsoft.AspNetCore.Mvc;
using MomsNest.DataAccess.Repository;
using MomsNest.Models;
using System.Diagnostics;

namespace MomsNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        
        private readonly IUnitOfWork unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> PrductList = unitOfWork.Product.GetAll().ToList();
            return View(PrductList);
        }

        public IActionResult Details(int id)
        {
            Product product = unitOfWork.Product.Get(u => u.ProductId == id);
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
