using Helper;
using Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Security;

namespace EOffice.Areas.Users.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Users/Customer
        Security.EncryptIT Encrypts = new EncryptIT();
        DBClass DBA;
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

                DataModel.DMSearchField DY = new DataModel.DMSearchField();
                DY.MonthList = objTools.GetMonths();
                DY.YearList = objTools.GetYears();

                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCustomer(DataModel.DMUserNewCustomerInsert data, HttpPostedFileBase npwpimg, HttpPostedFileBase idimage)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            DBA = new DBClass();
            objTools = new Utility();
            

            if (myCookie != null)
            {

                try
                {

                    if (ModelState.IsValid==false)
                    {
                        return Json(new { isSuccess = false, msg = string.Format("Please Fill Out All Data") }, JsonRequestBehavior.AllowGet);
                        
                    }
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    

                    Hashtable hst = new Hashtable();
                    hst.Add("@CompanyName", data.NewCustomer.CompanyName);
                    hst.Add("@Address", data.NewCustomer.Address);
                    hst.Add("@City", data.NewCustomer.City);
                    hst.Add("@Province", data.NewCustomer.Province);
                    hst.Add("@Country", string.Empty);
                    hst.Add("@Phone", data.NewCustomer.Phone);
                    hst.Add("@ZIP", data.NewCustomer.ZIP);
                    // hst.Add("@Fax", data.DC.FAX);
                    hst.Add("@NPWP", data.NewCustomer.NPWP);
                    
                    
                    hst.Add("@ContactPerson", data.NewCustomer.ContactPerson);
                    hst.Add("@CAddress", data.NewCustomer.CAddress);
                    hst.Add("@CCity", data.NewCustomer.CCity);
                    hst.Add("@CProvince", data.NewCustomer.CProvince);
                    hst.Add("@CZIP", data.NewCustomer.CZip);
                    hst.Add("@IDType", data.NewCustomer.IDType);
                    hst.Add("@IDNumber", data.NewCustomer.IDNumber);
                    hst.Add("@MobilePhone", data.NewCustomer.MobilePhone);
                    hst.Add("@EMail", data.NewCustomer.Email);
                    hst.Add("@UserName", DL.FullName);
                    hst.Add("@ClientID", DL.ClientID);
                    
                    //string CID = objTools.SaveClient(hst, ST, SC);
                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/Users-Documents/" + DL.ClientID + "/"));
                    if (!exists) System.IO.Directory.CreateDirectory(Server.MapPath("~/Users-Documents/" + DL.ClientID + "/"));


                    if (npwpimg != null && npwpimg.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(npwpimg.FileName);
                        var path = Path.Combine(Server.MapPath("~/Users-Documents/" + DL.ClientID + "/"), fileName);
                        npwpimg.SaveAs(path);
                        hst.Add("@NPWPPic", fileName);
                    }
                    else
                    {
                        hst.Add("@NPWPPic", string.Empty);
                    }
                    if (idimage != null && idimage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(idimage.FileName);
                        var path = Path.Combine(Server.MapPath("~/Users-Documents/" + DL.ClientID + "/"), fileName);
                        idimage.SaveAs(path);
                        hst.Add("@CustIDPIC", fileName);
                    }
                    else
                    {
                        hst.Add("@CustIDPIC", string.Empty);
                    }

                    DBA.ExecSP("[SP_UsersCustomerInsert]", hst);
                    DBA.CommitTransaction();
                  


                    return Json(new { isSuccess = true, url = "/Users/Customer/" }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    DBA.RollBackTransaction();
                    return Json(new { isSuccess = false, msg = string.Format(ex.Message.ToString()) }, JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        public ActionResult NewCustomer(int id = 0)
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

                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.LastLogin = Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy hh:mm:ss");

                hst.Clear();
                DataModel.DMUserNewCustomerInsert DNC = new DataModel.DMUserNewCustomerInsert();
                DataModel.DMUsersNewCustomer DC = new DataModel.DMUsersNewCustomer();
                List<DataModel.DMEnum> IDType = objTools.GetEnum("ID Type");
                if (id != 0)
                {
                    DBA = new DBClass();
                    
                    DataTable dt = new DataTable();
                    hst.Clear();
                    hst.Add("@ClientID", DL.ClientID);
                    hst.Add("@ID", id);
                    dt = DBA.GetDataTables("[SP_UsersCustomerDetails]", hst);
                    for(var i=0; i<= dt.Rows.Count - 1; i++)
                    {
                        DC.CompanyName = dt.Rows[i]["CompanyName"].ToString();
                        DC.Address = dt.Rows[i]["Address1"].ToString();
                        DC.City = dt.Rows[i]["City"].ToString();
                        DC.Province = dt.Rows[i]["StateProvince"].ToString();
                        DC.ZIP = dt.Rows[i]["ZipCode"].ToString();
                        DC.Phone = dt.Rows[i]["Phone"].ToString();
                        DC.NPWP = dt.Rows[i]["NPWP"].ToString();
                        DC.ContactPerson = dt.Rows[i]["ContactPerson"].ToString();

                        
                    }
                }
                DNC.NewCustomer = DC;
                DNC.IDType = IDType;
                
                return View(DNC);

            }
            else
            {
                return RedirectPermanent("/");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerSearch(DataModel.DMSearchField data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                objTools = new Utility();
                DBA = new DBClass();
                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());


                List<DataModel.DMUserCustomerList> CList = new List<DataModel.DMUserCustomerList>();
                DataTable Dt = new DataTable();
                Hashtable hst = new Hashtable();
                hst.Add("@Key", data.Searching.Key);
                hst.Add("@ClientID", DL.ClientID);
                //hst.Add("@Month", data.Month);
                //hst.Add("@Years", data.Year);
                Dt = DBA.GetDataTables("[SP_UsersCustomerLoad]", hst);
                if (Dt.Rows.Count > 0)
                {
                    CList = Dt.DataTableToList<DataModel.DMUserCustomerList>();

                    return PartialView("_CustomerList", CList);
                }
                else
                {
                    return Json(new { isSuccess = false, msg = string.Format("No Record(s) found") }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
    }
    
}