using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;

namespace Pronia.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
