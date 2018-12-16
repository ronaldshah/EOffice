using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Helper;
using Security;
using System.Data;
using System.Web.Caching;


using System.Net;

using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;

namespace EOffice.Areas.SuperAdmin.Controllers
{
    public class ClientController : Controller
    {
        // GET: SuperAdmin/Client
        EncryptIT Encrypts = new EncryptIT();
        DBClass DBA;
        Helper.Utility objTools;
        public ActionResult Index()
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {

                Hashtable hst = new Hashtable();
                DBA = new DBClass();
                objTools = new Utility();
                DataModel.DMLoginDetails DL = objTools.GetLoginDetails(myCookie.Value.ToString());
                hst.Add("@RoleID", Convert.ToInt16(DL.RoleID));
                ViewBag.TxtMenu = objTools.CreateMenu(hst, "[SP_T_SuperAdmin_Menu_Load]");

                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.Department = DL.DepartmentName;
                ViewBag.RolesName = DL.RolesName;
                ViewBag.LastLogin = Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy hh:mm:ss");
                List<DataModel.DMClientMaster> CLList = new List<DataModel.DMClientMaster>();
                HttpContext.Cache.Insert("ChaceClientList", CLList, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));

                return View();
                
            }
            else
            {
                return RedirectPermanent("/SuperAdmin/Default/");
            }
        }

        public ActionResult SearchClient(string Keys)
        {
            Hashtable hst = new Hashtable();
            DBA = new DBClass();
            List<DataModel.DMClientMaster> CLList = new List<DataModel.DMClientMaster>();
            
            List<DataModel.DMClientMaster> ChaceMenu = HttpContext.Cache["ChaceClientList"] as List<DataModel.DMClientMaster>;
            if (ChaceMenu == null || ChaceMenu.Count == 0)
            {
                DataTable Dt = new DataTable();

                hst.Clear();
                hst.Add("@Key", Keys);
                Dt = DBA.GetDataTables("[SP_T_ClientMaster_Load]", hst);
                CLList = Dt.DataTableToList<DataModel.DMClientMaster>();
                HttpContext.Cache.Insert("ChaceClientList", CLList, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                return PartialView("_ClientList", CLList);
            }
            else
            {
                var CLLists = ChaceMenu.Where(x => x.CompanyName.Contains(Keys) || x.ContactName.Contains(Keys));
                CLList = CLLists.ToList();
                return PartialView("_ClientList", CLList);
            }
            
            

           
        }

        #region New Client
        public ActionResult NewClient()
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {

                Hashtable hst = new Hashtable();
                DBA = new DBClass ();
                objTools = new Utility();
                DataModel.DMLoginDetails DL = objTools.GetLoginDetails(myCookie.Value.ToString());
                hst.Add("@RoleID", Convert.ToInt16(DL.RoleID));
                ViewBag.TxtMenu = objTools.CreateMenu(hst, "[SP_T_SuperAdmin_Menu_Load]");

                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.Department = DL.DepartmentName;
                ViewBag.RolesName = DL.RolesName;
                ViewBag.LastLogin = Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy hh:mm:ss");

                hst.Clear();
                DataModel.DMInsertNewClient DNC = new DataModel.DMInsertNewClient();
                List<DataModel.DMEnum> DE = objTools.GetEnum("Types of business");
                List<DataModel.DMEnum> EMPSize = objTools.GetEnum("Employee Size");
                List<DataModel.DMEnum> IDType = objTools.GetEnum("ID Type");
                List<DataModel.DMEnum> BillingCycle = objTools.GetEnum("Billing Cycle");
                List<DataModel.DMEnum> Products = objTools.GetProducts();

                DNC.TOB = DE;
                DNC.EMPSize = EMPSize;
                DNC.IDType = IDType;
                DNC.BillingCylce = BillingCycle;
                DNC.Products = Products;



                return View(DNC);

            }
            else
            {
                return RedirectPermanent("/SuperAdmin/Default/");
            }
        }
        [HttpPost]
        public ActionResult AddClient(DataModel.DMInsertNewClient data, HttpPostedFileBase npwpimg, HttpPostedFileBase idimage)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            DBA = new DBClass();

            if (myCookie != null)
            {
                try
                {
                    

                    DataModel.DMLoginDetails DL = objTools.GetLoginDetails(myCookie.Value.ToString());
                    Hashtable hst = new Hashtable();
                    hst.Add("@CompanyName", data.DC.CompanyName);
                    hst.Add("@Address", data.DC.Street);
                    hst.Add("@City", data.DC.City);
                    hst.Add("@Province", data.DC.Province);
                    //hst.Add("@Country", data.DC.Country);
                    hst.Add("@Phone1", data.DC.Phone);
                   // hst.Add("@Fax", data.DC.FAX);
                    hst.Add("@NPWP", data.DC.NPWP);
                    hst.Add("@ContactName", data.DC.ContactName);
                    hst.Add("@EmailAddress", data.DC.Email);

                    hst.Add("@MobileNo", data.DC.ContactNumber);
                    hst.Add("@UserName", DL.FullName);
                    string CID= DBA.ExecSPReturnVals("[SP_ClientMaster_Insert]", hst);
                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/Documents/" + CID + "/"));
                    if (!exists) System.IO.Directory.CreateDirectory(Server.MapPath("~/Documents/" + CID + "/"));
                    
                    
                    if (npwpimg != null && npwpimg.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(npwpimg.FileName);
                        var path = Path.Combine(Server.MapPath("~/Documents/" + CID + "/"), fileName);
                        npwpimg.SaveAs(path);
                    }
                    if (idimage != null && idimage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(idimage.FileName);
                        var path = Path.Combine(Server.MapPath("~/Documents/" + CID + "/"), fileName);
                        idimage.SaveAs(path);
                    }
                    hst.Clear();
                    hst.Add("@ClientID", CID);
                    hst.Add("@PromoName", data.DC.Promo);
                    hst.Add("@Discount", data.DC.Discount);
                    hst.Add("@SetupFee", data.DC.SetupFee);
                    hst.Add("@UserName", DL.FullName);
                    hst.Add("@ProductName", "EOffice Full");
                    hst.Add("@BillingCycle", data.DC.BillingCycle);
                    DBA.ExecSP("[SP_T_BillingInsert]",hst);


                    hst.Clear();
                    hst.Add("@ClientID", CID);
                    hst.Add("@Email", data.DC.Email);
                    hst.Add("@Roles", "Administrator");
                    hst.Add("@ProductName", "EOffice Full");
                    hst.Add("@UserName", DL.FullName);
                    


                    

                    
                    DBA = new DBClass();
                    List<DataModel.DMClientMaster> CLList = new List<DataModel.DMClientMaster>();

                    List<DataModel.DMClientMaster> ChaceMenu = HttpContext.Cache["ChaceClientList"] as List<DataModel.DMClientMaster>;
                    DataTable Dt = new DataTable();
                    hst.Clear();
                    hst.Add("@Key", CID);
                    Dt = DBA.GetDataTables("[SP_T_ClientMaster_Load]", hst);
                    CLList = Dt.DataTableToList<DataModel.DMClientMaster>();
                    HttpContext.Cache.Insert("ChaceClientList", CLList, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                    DBA.CommitTransaction();
                    return RedirectPermanent("/SuperAdmin/Client/");

                }
                catch (Exception ex)
                {
                    DBA.RollBackTransaction();
                    throw new Exception(ex.Message.ToString());
                }
            }
            else
            {
                throw new Exception("ERROR");
            }
        }

        #endregion

    }
}