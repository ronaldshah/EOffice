using Helper;
using Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EOffice.Areas.Users.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Users/Dashboard
        Security.EncryptIT Encrypts = new EncryptIT();
        Helper.Utility objTools;
        public ActionResult Index()
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                Hashtable hst = new Hashtable();
                objTools = new Utility();

                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.ClientID = DL.ClientID;
                
                ViewBag.LastLogin = Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy HH:mm:ss");
                hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                ViewBag.TxtMenu = objTools.CreateMenu(hst, "[SP_UsersMenuLoad]");
                return View();
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
    }
}