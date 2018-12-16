using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataModel;
using Security;
using System.Data;
using System.Collections;
using Helper;

namespace EOffice.Areas.SuperAdmin.Controllers
{
    public class DefaultController : Controller
    {
        // GET: SuperAdmin/Default
        HttpCookie HcUser;
        DBClass DBAccess = new DBClass();
        EncryptIT Encrypts = new  EncryptIT();
        Utility TheUtil = new Utility();
        public ActionResult Index()
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {

                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }


            return View();
        }

        [HttpGet]
        public ActionResult Index(string Msg="")
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {

                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            ViewBag.Msg = Msg;

            return View();
        }

        [HttpPost]
        
        public ActionResult Index(DMLogin data)
        {
            try
            {
                DataTable Dt = new DataTable();
                Hashtable hst = new Hashtable();
                Helper.Utility Helper = new Utility();
                List<DMLoginDetails> DL = new List<DMLoginDetails>();
                string HashPassword = TheUtil.CalculateMD5Hash(data.Password.ToString());
                hst.Add("@Email", data.EmailAddress.ToString());
                hst.Add("@Passwd", HashPassword);
                hst.Add("@IP", TheUtil.getIP());
                Dt = DBAccess.GetDataTables("[SP_SuperAdmin_Login]", hst);
                DL = Dt.DataTableToList<DMLoginDetails>();
                if (DL.Count>0)
                {

                    HcUser = new HttpCookie("Uinfo");
                    Helper.ClearChace();
                    string AuthToken = Helper.CreateAuthToken(DL);
                    HcUser.Value = AuthToken;
                    HcUser.Expires = DateTime.Now.AddMinutes(60);
                    HttpContext.Response.SetCookie(HcUser);





                    return Redirect("/SuperAdmin/Dashboard");

                   
                }
                return Redirect("/SuperAdmin/Default?Msg=User Name and or password incorect");
            }
            catch (Exception ex)
            {
                return Redirect("/SuperAdmin/Default?Msg="+ ex.Message.ToString());
            }
        }
    }
}