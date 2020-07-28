using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Watched.Controllers
{
    public class PlayerController : Controller
    {
        public IActionResult RealTime()
        {
            return View();
        }
    }
}