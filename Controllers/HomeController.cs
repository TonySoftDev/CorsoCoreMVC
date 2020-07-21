using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers
{
    public class HomeController : Controller
    {
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [ResponseCache(CacheProfileName = "Home")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Home Page WebApp";
            return View();
        }
    }
}
