﻿using Microsoft.AspNetCore.Mvc;

namespace MvcProject.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            return View();
        }
    }

}
