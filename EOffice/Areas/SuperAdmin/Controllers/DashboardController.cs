using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Helper;
using System.Collections;



namespace EOffice.Areas.SuperAdmin.Controllers
{
    public class DashboardController : Controller
    {
        // GET: SuperAdmin/Dashboard
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

                DataModel.DMLoginDetails DL = objTools.GetLoginDetails(myCookie.Value.ToString());
                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.Department = DL.DepartmentName;
                ViewBag.RolesName = DL.RolesName;
                ViewBag.LastLogin =Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy HH:mm:ss");
                hst.Add("@RoleID", Convert.ToInt16(DL.RoleID));
                ViewBag.TxtMenu = objTools.CreateMenu(hst, "[SP_T_SuperAdmin_Menu_Load]");
                return View();
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
    }
}