using DataModel;
using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EOffice.Areas.Users.Controllers
{
    public class AdministratorController : Controller
    {
        // GET: Users/Administrator
        Utility objTools;
        DBClass DBA;
        public ActionResult Index()
        {
            return View();
        }
        
    }
}