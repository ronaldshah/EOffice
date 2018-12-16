using DataModel;
using Helper;
using Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EOffice.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        HttpCookie HcUser;
        
        Security.EncryptIT Encrypts = new EncryptIT();
        Utility TheUtil = new Utility();
        DBClass DBA = new DBClass();
        public ActionResult Index()
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                return View();
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Index(string Msg = "")
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
                List<DMUsersLoginDetails> DL = new List<DMUsersLoginDetails>();
                string HashPassword = TheUtil.CalculateMD5Hash(data.Password.ToString());
                hst.Add("@Email", data.EmailAddress.ToString());
                hst.Add("@Passwd", HashPassword);
                hst.Add("@IP", TheUtil.getIP());
                Dt = DBA.GetDataTables("[SP_Users_Login]", hst);
                DL = Dt.DataTableToList<DMUsersLoginDetails>();
                if (DL.Count > 0)
                {

                    HcUser = new HttpCookie("Uinfo");
                    Helper.ClearChace();
                    string AuthToken = Helper.CreateUserAuthToken(DL);
                    HcUser.Value = AuthToken;
                    HcUser.Expires = DateTime.Now.AddMinutes(60);
                    HttpContext.Response.SetCookie(HcUser);





                    return Redirect("/Users/Dashboard");


                }
                return Redirect("/Home?Msg=User Name and or password incorect");
            }
            catch (Exception ex)
            {
                return Redirect("/Home?Msg=" + ex.Message.ToString());
            }
        }
    }
}