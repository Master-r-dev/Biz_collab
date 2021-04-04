using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Controllers
{
    public class ModulesController : Controller
    {
        public IActionResult Module1()
        {
            return View();
        }
        public IActionResult Module2()
        {
            return View();
        }
        public IActionResult Module3()
        {
            return View();
        }

    }
}
