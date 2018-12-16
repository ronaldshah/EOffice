using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Helper;
using DataModel;
using System.Data;
using System.Collections;
using Security;
using System.Web.Caching;
using System.Linq;

namespace EOffice.Areas.Users.Controllers
{
    public class BillingController : Controller
    {
        // GET: Users/Billing
        Utility objTools;
        DBClass DBA;
        

        public ActionResult Index()
        {
            return View();
        }

        #region Invoice
        public ActionResult Invoice()
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
                
                DMSearchField DY = new DMSearchField();
                DY.MonthList = objTools.GetMonths();
                DY.YearList = objTools.GetYears();

                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
            
        }
        public ActionResult InvoiceSearch(DMSearchField data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                objTools = new Utility();
                DBA = new DBClass();
                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                

                List<DataModel.DMInvoiceList> InvList = new List<DMInvoiceList>();
                DataTable Dt = new DataTable();
                Hashtable hst = new Hashtable();
                hst.Add("@Key", data.Searching.Key);
                hst.Add("@ClientID", DL.ClientID);
                hst.Add("@Month", data.Searching.Month);
                hst.Add("@Years", data.Searching.Year);
                Dt = DBA.GetDataTables("[SP_UsersInvoiceLoad]", hst);
                if (Dt.Rows.Count > 0)
                {
                    InvList = Dt.DataTableToList<DMInvoiceList>();

                    return PartialView("_InvoiceList", InvList);
                }
                else
                {
                    return Json(new { isSuccess = false, msg = string.Format("No Record(s) found")}, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        [HttpGet]
        public ActionResult NewInvoice(int CustID = 0,string InvNo="")
        {
            
            DMClientCustomerToInvoice DC = new DMClientCustomerToInvoice();

            List<DataModel.DMNewInvoice> ChaceInvoice = new List<DataModel.DMNewInvoice>();
            HttpContext.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, 0));

            DMCreateNewInvoice DCI = new DMCreateNewInvoice();
            
            DataModel.DMNewInvoice INV = new DMNewInvoice();
            List<DMNewInvoice> DI = new List<DMNewInvoice>();
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                INV.NoID = 1;
                INV.Qty = 1;
                INV.Price = 0;
                INV.Total = 0;
                DI.Add(INV);
                Hashtable hst = new Hashtable();
                objTools = new Utility();

                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                ViewBag.Foto = DL.Foto;
                ViewBag.UserName = DL.FullName;
                ViewBag.ClientID = DL.ClientID;

                ViewBag.LastLogin = Convert.ToDateTime(DL.LastLogin).ToString("dd MMM yyyy HH:mm:ss");
                hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                ViewBag.TxtMenu = objTools.CreateMenu(hst, "[SP_UsersMenuLoad]");

                var t = AddItemInvoice(DI);
                ViewBag.Total = CountTotal(t);
                DCI.ListDI = DI;

                if (CustID > 0)
                {
                    
                    DataTable dt = new DataTable();
                    
                    
                    DBA = new DBClass();
                    hst.Add("@ClientID", DL.ClientID);
                    hst.Add("@ID", CustID);
                    dt = DBA.GetDataTables("[SP_UsersCustomerDetails]", hst);
                    for(var i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        DC.ClientID = DL.ClientID;
                        DC.ContactPerson = dt.Rows[i]["ContactPerson"].ToString();
                        DCI.NC = DC;
                    }
                }
                else
                {
                    DCI.NC = DC;
                }

                return View(DCI);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        public ActionResult SaveInvoice()
        {
            return View();

        }
        public ActionResult AddItem(DMCreateNewInvoice data, string submitButton)
        {
            
            switch (submitButton)
            {
                case  "AddRow":
                    
                    DMCreateNewInvoice DCI = new DMCreateNewInvoice();
                    var t = AddItemInvoice(data.Di);
                    ViewBag.Total = CountTotal(t);
                    DCI.ListDI = t;
                    return PartialView("_NewInvoice", DCI);

                case "Save":
                    
                    break;
            }

            return View();
        }

        public ActionResult FirstRow()
        {
            DMCreateNewInvoice DCI = new DMCreateNewInvoice();
            DMCreateNewInvoice data = new DMCreateNewInvoice();
            List<DMNewInvoice> DI = new List<DMNewInvoice>();
            DMNewInvoice NI = new DMNewInvoice();

            NI.NoID = 1;
            DI.Add(NI);
            data.Di = DI;
            
            var t = AddItemInvoice(data.Di);
            ViewBag.Total = CountTotal(t);
            DCI.ListDI = t;

            return PartialView("_NewInvoice", DCI);

        }
        public ActionResult UpdateInvoiceItem(int ID,string ItemName, string Descriptions, int Qty, string UOM, int Price)
        {
            objTools = new Utility();
            DMCreateNewInvoice DCI = new DMCreateNewInvoice();

            DataModel.DMNewInvoice INV = new DMNewInvoice();
            List<DMNewInvoice> DI = new List<DMNewInvoice>();
            INV.NoID = ID;
            INV.Qty = Qty;
            INV.Price = Price;
            INV.ItemName = ItemName;
            INV.Descriptions = Descriptions;

            List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
            var CI = ChaceInvoice.FirstOrDefault(x => x.NoID == INV.NoID);
            CI.ItemName = INV.ItemName;
            CI.ItemID = INV.ItemID;
            CI.Descriptions = INV.Descriptions;
            CI.Qty = INV.Qty;
            CI.UOM = INV.UOM;
            CI.Price = INV.Price;
            CI.Total = INV.Qty * INV.Price;
            HttpContext.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
            ViewBag.Total = CountTotal(ChaceInvoice);
            DCI.ListDI = DI;
            return PartialView("_NewInvoice", DCI);
        }
        public ActionResult DeleteInvoiceItem(int ID)
        {
            
            DMCreateNewInvoice DCI = new DMCreateNewInvoice();

            DataModel.DMNewInvoice INV = new DMNewInvoice();
            int i = 1;

            INV.NoID = ID;
           
            
            List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
            List<DataModel.DMNewInvoice> TmpChaceInvoice = new List<DataModel.DMNewInvoice>();
            foreach (DataModel.DMNewInvoice DI in ChaceInvoice)
            {
                if (DI.NoID != ID)
                {
                    DataModel.DMNewInvoice NI = new DataModel.DMNewInvoice();
                    NI.NoID = i;
                    NI.ItemID = DI.ItemID;
                    NI.ItemName = DI.ItemName;
                    NI.Descriptions = DI.Descriptions;
                    NI.Qty = DI.Qty;
                    NI.UOM = DI.UOM;
                    NI.Price = DI.Price;
                    NI.Total = DI.Qty * DI.Price;
                    TmpChaceInvoice.Add(NI);
                    i++;
                }
            }
            HttpContext.Cache.Insert("ChaceInvoice", TmpChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));

            DCI.ListDI = TmpChaceInvoice;
            ViewBag.Total = CountTotal(TmpChaceInvoice);
            return PartialView("_NewInvoice", DCI);
        }
        private int CountTotal(List<DMNewInvoice> data)
        {
            int Total = 0;
            foreach(DMNewInvoice DI in data)
            {
                Total = Total + (DI.Qty * DI.Price);
            }
            return Total;
        }
        #endregion

        private List<DataModel.DMNewInvoice> AddItemInvoice(List<DataModel.DMNewInvoice> data)
        {

            List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
            try
            {
                if (ChaceInvoice == null || ChaceInvoice.Count == 0)
                {
                    DataModel.DMNewInvoice NV = new DataModel.DMNewInvoice();
                    ChaceInvoice = new List<DataModel.DMNewInvoice>();
                    ChaceInvoice.Add(new DataModel.DMNewInvoice() { NoID = data[0].NoID, ItemID = 0, ItemName = string.Empty, Descriptions = string.Empty, Price = 0, Qty = 1, Total = 0, UOM = string.Empty });

                    HttpContext.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                }
                else
                {
                    bool Addrow = false;
                    foreach (DataModel.DMNewInvoice DI in data)
                    {
                        Addrow = false;
                        if (DI.ItemName != null && DI.Qty != 0 && DI.Price != 0)
                        {
                            Addrow = true;
                            var CI = ChaceInvoice.FirstOrDefault(x => x.NoID == DI.NoID);
                            CI.ItemName = DI.ItemName;
                            CI.ItemID = DI.ItemID;
                            CI.Descriptions = DI.Descriptions;
                            CI.Qty = DI.Qty;
                            CI.UOM = DI.UOM;
                            CI.Price = DI.Price;
                            CI.Total = DI.Qty * DI.Price;
                        }
                    }

                    if (Addrow == true)
                    {
                        DataModel.DMNewInvoice NI = new DataModel.DMNewInvoice();
                        NI.NoID = ChaceInvoice.Count() + 1;
                        NI.Qty = 1;

                        ChaceInvoice.Add(NI);
                    }


                    HttpContext.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));

                }

            }
            catch { }

            return ChaceInvoice;
        }
    }
}