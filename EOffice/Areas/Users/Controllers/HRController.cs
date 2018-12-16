using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Helper;
using DataModel;
using System.Data;
using System.Collections;
using Security;
using System.Web.Caching;

namespace EOffice.Areas.Users.Controllers
{
    public class HRController : Controller
    {
        // GET: Users/HR
        Helper.Utility objTools;
        DBClass DBA;
        public ActionResult Index()
        {
            return View();
        }

        #region Attendance

        public ActionResult UpdateWorkingHour(int TimeGroupID)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try
                {
                    DBA = new DBClass();
                    objTools = new Utility();
                    Hashtable hst = new Hashtable();
                    DataTable dt = new DataTable();
                    List<DMNewWorkingHour> Data = new List<DMNewWorkingHour>();
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    hst.Add("@TimeGroupID",TimeGroupID);
                    hst.Add("@UserName", DL.FullName);
                    
                    dt = DBA.GetDataTables("[SP_AttendanceTimeGroup_FActive]", hst);
                    Data = dt.DataTableToList<DMNewWorkingHour>();
                    return PartialView("_WorkingTimeGroupList", Data);
                }
                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = string.Format("Failed!<br/>" + ex.Message.ToString()), url = "/" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
        }

        public ActionResult WorkingTimeGroupSearch(DMSearchField data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try
                {
                    DBA = new DBClass();
                    objTools = new Utility();
                    Hashtable hst = new Hashtable();
                    DataTable dt = new DataTable();
                    List<DMNewWorkingHour> Data = new List<DMNewWorkingHour>();
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                    hst.Add("@BranchID",data.Searching.Key2);
                    hst.Add("@Key", data.Searching.Key);
                    dt = DBA.GetDataTables("[SP_AttendanceTimeGroup_View]", hst);
                    Data = dt.DataTableToList<DMNewWorkingHour>();
                    return PartialView("_WorkingTimeGroupList", Data);




                }
                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = string.Format("Failed!<br/>" + ex.Message.ToString()), url = "/" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveNewWorkingHour(DMNewWorkingHour data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try
                {
                    DBA = new DBClass();
                    objTools = new Utility();
                    Hashtable hst = new Hashtable();
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());


                    hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                    hst.Add("@BranchID", data.Branch);
                    hst.Add("@TimeGroupName", data.TimeGroupName);
                    hst.Add("@CurrentIn", data.CurrentIn);
                    hst.Add("@MaxIn", data.MaxIn);
                    hst.Add("@MinIn", data.MinIn);
                    hst.Add("@CurrentOut", data.CurrentOut);
                    hst.Add("@MaxOut", data.MaxOut);
                    hst.Add("@MinOut", data.MinOut);
                    hst.Add("@UserName", DL.FullName);

                    DBA.ExecSP("[SP_AttendanceTimeGroup_Insert]", hst);
                    return Json(new { isSuccess = true, msg = string.Format("Save") }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = string.Format("Failed!" + ex.Message.ToString()), url = "/" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }

        }
        public ActionResult NewWorkingHour(int BranchID = 0)
        {
            if (BranchID == 0)
            {
                ViewBag.BranchName = "All Branch";
            }
            ViewBag.BranchID = BranchID;
            return PartialView("_NewWorkingHour");
        }
        public ActionResult WorkingTime()
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
                hst.Clear();
                hst.Add("@ClientID", DL.ClientID);
                DY.BranchOffice = objTools.FillCombo("[SP_OfficeToCombo]", hst);


                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        #endregion

        #region Group Shift
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGroupShiftRow(DMGroupShiftInsert data, string submitButton)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            List<DMGroupShiftDetails> ChaceGroupShift = HttpContext.Cache["ChaceGroupShift"] as List<DMGroupShiftDetails>;
            DMGroupShiftDetails GroupShift = new DMGroupShiftDetails();
            DMGroupShiftHeader GroupShiftHeader = new DMGroupShiftHeader();
            DMGroupShiftInsert GroupShiftInsert = new DMGroupShiftInsert();
            List<DMNewWorkingHour> TimeGroup = new List<DMNewWorkingHour>();

            DBA = new DBClass();

            DataTable Dt = new DataTable();
            Hashtable hst = new Hashtable();
            
            if (myCookie != null)
            {

                switch (submitButton)
                {
                    case "AddRow":
                        
                        bool Addrow = false;
                        objTools = new Utility();
                        DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());

                        ViewBag.BranchID = data.GroupShiftHeader.BranchID;
                        ViewBag.BranchName = data.GroupShiftHeader.BranchName;
                        hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                        hst.Add("@BranchID", Convert.ToInt16(DL.BranchID));
                        Dt = DBA.GetDataTables("[SP_AttendanceTimeGroupToCombo]", hst);
                        TimeGroup = Dt.DataTableToList<DMNewWorkingHour>();

                        GroupShiftInsert.TimeGroup = TimeGroup;

                        GroupShiftHeader.BranchName = data.GroupShiftHeader.BranchName;
                        GroupShiftHeader.BranchID = data.GroupShiftHeader.BranchID;
                        GroupShiftHeader.ClientID = data.GroupShiftHeader.ClientID;
                        GroupShiftHeader.EffectiveDate = data.GroupShiftHeader.EffectiveDate;
                        GroupShiftHeader.ID = data.GroupShiftHeader.ID;
                        GroupShiftHeader.ShiftName = data.GroupShiftHeader.ShiftName;

                        foreach (DMGroupShiftDetails DI in data.GroupShiftDetails)
                        {
                            Addrow = false;
                            if (DI.ID != 0 && DI.WorkingHourID !=0 )
                            {
                                Addrow = true;
                                var CI = ChaceGroupShift.FirstOrDefault(x => x.ID == DI.ID);
                                CI.WorkingHourID = DI.WorkingHourID;
                                CI.WorkingDay = DI.WorkingDay;
                                CI.OffDay = DI.OffDay;
                                
                            }
                        }

                        if (Addrow == true)
                        {
                            DMGroupShiftDetails ShiftDetails = new DMGroupShiftDetails();
                            ShiftDetails.ID= ChaceGroupShift.Count() + 1;
                            ShiftDetails.WorkingHourID = 0;
                            ShiftDetails.WorkingDay = 0;
                            ShiftDetails.OffDay = 0;
                            ChaceGroupShift.Add(ShiftDetails);
                        }

                        GroupShiftInsert.GroupShiftDetails = ChaceGroupShift;
                        GroupShiftInsert.GroupShiftHeader = GroupShiftHeader;
                        HttpContext.Cache.Insert("ChaceGroupShift", ChaceGroupShift, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                        return PartialView("_GroupShiftNew", GroupShiftInsert);

                    case "Save":
                        objTools = new Utility();
                        DBA = new DBClass();
                        DMUsersLoginDetails DLD = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                        hst.Clear();
                        hst.Add("@ClientID", Convert.ToInt16(DLD.ClientID));
                        hst.Add("@BranchID", Convert.ToInt16(DLD.BranchID));
                        hst.Add("@ShiftName", data.GroupShiftHeader.ShiftName);
                        hst.Add("@EfectiveDate", data.GroupShiftHeader.EffectiveDate);
                        hst.Add("@UserName", DLD.FullName);
                        string NewID = "0";
                        NewID= DBA.GetVals("[SP_AttendanceShiftGroup_Insert]", hst);
                        foreach(DMGroupShiftDetails DD in data.GroupShiftDetails)
                        {
                            hst.Clear();
                            hst.Add("@ShiftID", Convert.ToInt64(NewID));
                            hst.Add("@WorkingTimeID", DD.WorkingHourID);
                            hst.Add("@WorkingDays", DD.WorkingDay);
                            hst.Add("@OffDays", DD.OffDay);
                            DBA.ExecSP("[SP_AttendanceShiftGroupDetails_Insert]", hst);
                           
                        }
                        return RedirectPermanent("/Users/HR/GroupShift");

                }
                return View();
                
            }
            else {
                return RedirectPermanent("/");
            }
        }

        public ActionResult EDShiftGroup(int ShiftID,int BranchID=0)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try
                {
                    DBA = new DBClass();
                    objTools = new Utility();
                    Hashtable hst = new Hashtable();
                    DataTable dt = new DataTable();
                    DMGroupShiftList Data = new DMGroupShiftList();
                    List<DMGroupShiftHeader> ShiftHeader = new List<DMGroupShiftHeader>();
                    List<DMGroupShiftDetails> ShiftDetails = new List<DMGroupShiftDetails>();

                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    hst.Add("@ShiftID", ShiftID);
                    hst.Add("@UserName", DL.FullName);

                    DBA.ExecSP("[SP_AttendanceShiftGroup_FActive]", hst);

                    hst.Clear();
                    hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                    hst.Add("@BranchID", BranchID);
                    hst.Add("@Key", String.Empty);
                    dt = DBA.GetDataTables("[SP_AttendanceShiftGroupHeader_View]", hst);
                    ShiftHeader = dt.DataTableToList<DMGroupShiftHeader>();
                    dt.Clear();
                    hst.Clear();
                    for (int i = 0; i <= ShiftHeader.Count - 1; i++)
                    {
                        hst.Clear();
                        hst.Add("@ShiftID", ShiftHeader[i].ID);
                        dt = DBA.GetDataTables("[SP_AttendanceShiftGroupDetails_View]", hst);
                        for (int y = 0; y <= dt.Rows.Count - 1; y++)
                        {
                            ShiftDetails.Add(new DMGroupShiftDetails { ShiftID = Convert.ToInt32(dt.Rows[y][0].ToString()), WorkingHourID = Convert.ToInt32(dt.Rows[y][1].ToString()), WorkingDay = Convert.ToInt32(dt.Rows[y][2].ToString()), OffDay = Convert.ToInt32(dt.Rows[y][3].ToString()), CurrentIn = Convert.ToDateTime(dt.Rows[y][4].ToString()), CurrentOut = Convert.ToDateTime(dt.Rows[y][5].ToString()) });

                        }

                    }
                    Data.GroupShiftHeader = ShiftHeader;
                    Data.GroupShiftDetails = ShiftDetails;

                    return PartialView("_GroupShiftList", Data);

                }
                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = string.Format("Failed!<br/>" + ex.Message.ToString()), url = "/" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        public ActionResult WorkingShiftGroupSearch(DMSearchField data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try
                {
                    DBA = new DBClass();
                    objTools = new Utility();
                    Hashtable hst = new Hashtable();
                    DataTable dt = new DataTable();
                    DMGroupShiftList Data = new DMGroupShiftList();
                    List<DMGroupShiftHeader> ShiftHeader = new List<DMGroupShiftHeader>();
                    List<DMGroupShiftDetails> ShiftDetails = new List<DMGroupShiftDetails>();
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                    hst.Add("@BranchID", data.Searching.Key2);
                    hst.Add("@Key", data.Searching.Key);
                    dt = DBA.GetDataTables("[SP_AttendanceShiftGroupHeader_View]", hst);
                    ShiftHeader = dt.DataTableToList<DMGroupShiftHeader>();
                    dt.Clear();
                    hst.Clear();
                    for(int i = 0; i <= ShiftHeader.Count-1; i++)
                    {
                        hst.Clear();
                        hst.Add("@ShiftID", ShiftHeader[i].ID);
                        dt = DBA.GetDataTables("[SP_AttendanceShiftGroupDetails_View]", hst);
                        for (int y = 0; y <= dt.Rows.Count - 1; y++) {
                            ShiftDetails.Add(new DMGroupShiftDetails { ShiftID = Convert.ToInt32(dt.Rows[y][0].ToString()),WorkingHourID= Convert.ToInt32(dt.Rows[y][1].ToString()),WorkingDay= Convert.ToInt32(dt.Rows[y][2].ToString()),OffDay= Convert.ToInt32(dt.Rows[y][3].ToString()), CurrentIn=Convert.ToDateTime(dt.Rows[y][4].ToString()), CurrentOut =Convert.ToDateTime(dt.Rows[y][5].ToString()) });

                        }

                    }
                    Data.GroupShiftHeader = ShiftHeader;
                    Data.GroupShiftDetails = ShiftDetails;

                    return PartialView("_GroupShiftList", Data);




                }
                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = string.Format("Failed!<br/>" + ex.Message.ToString()), url = "/" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectPermanent("/");
            }

        }
        public ActionResult NewGroupShift(int BranchID = 0)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                try {
                    objTools = new Utility();
                    DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                    List<DMGroupShiftDetails> ChaceGroupShift = HttpContext.Cache["ChaceGroupShift"] as List<DMGroupShiftDetails>;
                    

                    DMGroupShiftInsert AddNew = new DMGroupShiftInsert();
                    List<DMNewWorkingHour> TimeGroup = new List<DMNewWorkingHour>();
                    List<DMGroupShiftDetails> GroupShiftDetails = new List<DMGroupShiftDetails>();
                    DMGroupShiftHeader GroupShiftHeader = new DMGroupShiftHeader();
                    DataTable Dt = new DataTable();
                    Hashtable hst = new Hashtable();
                    DBA = new DBClass();

                    hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                    hst.Add("@BranchID", Convert.ToInt16(DL.BranchID));
                    Dt = DBA.GetDataTables("[SP_AttendanceTimeGroupToCombo]", hst);
                    TimeGroup = Dt.DataTableToList<DMNewWorkingHour>();
                    HttpContext.Cache.Insert("ChaceGroupShift", GroupShiftDetails, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                    GroupShiftDetails.Add(new DMGroupShiftDetails { OffDay = 0, WorkingDay = 0, WorkingHourID = 0,ID=1 });
                    HttpContext.Cache.Insert("ChaceGroupShift", GroupShiftDetails, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                    AddNew.TimeGroup = TimeGroup;
                    AddNew.GroupShiftDetails = GroupShiftDetails;

                    if (BranchID == 0)
                        {
                        GroupShiftHeader.BranchName = "All Branch";
                        }
                    GroupShiftHeader.BranchID = BranchID;
                    AddNew.GroupShiftHeader = GroupShiftHeader;
                        return PartialView("_GroupShiftNew", AddNew);


                } catch {
                    return PartialView("_GroupShiftNew");
                }
            }
            else
            {
                return RedirectPermanent("/");
            }
            
        }
        public ActionResult GroupShift()
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
                hst.Clear();
                hst.Add("@ClientID", DL.ClientID);
                DY.BranchOffice = objTools.FillCombo("[SP_OfficeToCombo]", hst);


                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }

        #endregion

        #region Employee

        public ActionResult NewEmployee()
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
                hst.Clear();
                hst.Add("@ClientID", DL.ClientID);
                DY.BranchOffice = objTools.FillCombo("[SP_OfficeToCombo]", hst);


                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        public ActionResult Employee()
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
                hst.Clear();
                hst.Add("@ClientID", DL.ClientID);
                DY.BranchOffice = objTools.FillCombo("[SP_OfficeToCombo]", hst);


                return View(DY);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }

        #endregion

        #region Holiday

        public ActionResult Holiday()
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

        [HttpPost]
        public ActionResult HolidaySearch(DMSearchField data)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                Hashtable hst = new Hashtable();
                List<DMHoliday> Holiday = new List<DMHoliday>();
                DataTable dt = new DataTable();
                DBA = new DBClass();
                objTools = new Utility();


                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                hst.Add("@Year", data.Searching.Year);
                hst.Add("@Month", data.Searching.Month);
                dt = DBA.GetDataTables("[SP_HolidayView]", hst);
                Holiday = dt.DataTableToList<DMHoliday>();


                return PartialView("_Holiday", Holiday);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }

        public ActionResult HolidaySearch(int Year,int Month)
        {
            HttpCookie myCookie = new HttpCookie("UInfo");
            myCookie = Request.Cookies["UInfo"];
            if (myCookie != null)
            {
                Hashtable hst = new Hashtable();
                List<DMHoliday> Holiday = new List<DMHoliday>();
                DataTable dt = new DataTable();
                DBA = new DBClass();
                objTools = new Utility();


                DataModel.DMUsersLoginDetails DL = objTools.GetClientLoginDetails(myCookie.Value.ToString());
                hst.Add("@ClientID", Convert.ToInt16(DL.ClientID));
                hst.Add("@Year", Year);
                hst.Add("@Month", Month);
                dt = DBA.GetDataTables("[SP_HolidayView]", hst);
                Holiday = dt.DataTableToList<DMHoliday>();


                return PartialView("_Holiday", Holiday);
            }
            else
            {
                return RedirectPermanent("/");
            }
        }
        #endregion
    }
}