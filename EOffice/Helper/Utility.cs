using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using Security;
using System.Data;
using System.Web.Caching;
using System.Data.SqlClient;
using System.Globalization;

namespace Helper
{
    public class Utility
    {
        DBClass DBA;
        SqlConnection SC;
        SqlTransaction ST;

        #region Public
        public List<DataModel.DMEnum> GetMonths()
        {
            List<DataModel.DMEnum> DE = new List<DataModel.DMEnum>();
            
            for(int i = 1; i <= 12; i++)
            {
                DataModel.DMEnum DNum = new DataModel.DMEnum();
                DNum.ID = i;
                DNum.TXT = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i - 1];
                DE.Add(DNum);
            }
            return DE;
        }

        public List<DataModel.DMEnum> GetYears()
        {
            List<DataModel.DMEnum> DE = new List<DataModel.DMEnum>();

            for (int i = 0; i <= (DateTime.Today.Year-2018) ; i++)
            {
                DataModel.DMEnum DNum = new DataModel.DMEnum();
                DNum.ID =Convert.ToInt32(DateTime.Today.AddYears(i).ToString("yyyy"));
                DNum.TXT = DateTime.Today.AddYears(i).ToString("yyyy");
                DE.Add(DNum);
            }
            return DE;
        }

        public List<DataModel.DMEnum> FillCombo(string SP,Hashtable hst)
        {
            List<DataModel.DMEnum> DE = new List<DataModel.DMEnum>();
            DataTable dt = new DataTable();
            try
            {
                DBA = new DBClass();
                dt = DBA.GetDataTables(SP, hst);
                DE = dt.DataTableToList<DataModel.DMEnum>();
                return DE;


            }
            catch (Exception ex) { throw new Exception(ex.Message.ToString()); }


            
        }
        public string getIP()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
               
        #endregion

        #region MENU
        public string CreateMenu(Hashtable hst,string sp)
        {
            try {
                DBA = new DBClass();
                string txt = string.Empty;
                DataTable Dt = new DataTable();
                
                List<DataModel.DMMenuList> MasterMenu = new List<DataModel.DMMenuList>();
                List<DataModel.DMMenuList> ChaceMenu = HttpContext.Current.Cache["ChaceMenu"] as List<DataModel.DMMenuList>;

                if (ChaceMenu == null || ChaceMenu.Count == 0)
                {
                    Dt = DBA.GetDataTables(sp, hst);
                    MasterMenu = Dt.DataTableToList<DataModel.DMMenuList>();
                    HttpContext.Current.Cache.Insert("ChaceMenu", MasterMenu, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                }
                else
                {
                    foreach(DataModel.DMMenuList ML in ChaceMenu)
                    {
                        DataModel.DMMenuList DL = new DataModel.DMMenuList();
                        DL.MenuID = ML.MenuID;
                        DL.MenuText = ML.MenuText;
                        DL.ParentID = ML.ParentID;
                        DL.Urls = ML.Urls;
                        DL.Icons = ML.Icons;
                        DL.DepartmentName = ML.DepartmentName;
                        DL.CanDelete = ML.CanDelete;
                        DL.CanEdit = ML.CanEdit;
                        DL.CanNew = ML.CanNew;
                        DL.CanProcess = ML.CanProcess;
                        DL.isParent = ML.isParent;
                        MasterMenu.Add(DL);


                        
                        
                    }
                }
                
                var ChilMenu = MasterMenu.Where(x => x.ParentID != 0);
                return CreateMenuString(MasterMenu.ToList());
            }
            catch(Exception ex) { throw new Exception(ex.Message.ToString()); }
        }

        private string CreateMenuString(List<DataModel.DMMenuList> MainMenu)
        {
            StringBuilder sb = new StringBuilder();

            string LastDepartment = string.Empty;
            foreach (DataModel.DMMenuList MM in MainMenu.Where(x=>x.isParent==1))
            {
                if (LastDepartment != MM.DepartmentName)
                {
                    LastDepartment = MM.DepartmentName;
                    sb.AppendLine("<li class='header-menu collapsed' data-toggle='collapse' data-target='#" + LastDepartment + "'>");
                    sb.AppendLine("<span><a href='#'><i class='fa " + MM.Icons + " fa-lg'></i>" + LastDepartment + "</a></span>");
                    sb.AppendLine("</li>");
                }
                var MP = MainMenu.Where(x => x.ParentID == MM.MenuID && x.isParent==0 );
                if(MP != null)
                {
                    sb.AppendLine("<ul class='sub-menu collapse' id='" + LastDepartment + "'>");
                        foreach (DataModel.DMMenuList DRSubChild in MP)
                        {
                            
                            var SubSubMenu = MainMenu.Where(x => x.ParentID  == DRSubChild.MenuID && x.isParent == 0);
                            if(SubSubMenu.Count() != 0)
                            {
                                sb.AppendLine("<li style='margin-left:40px' class='header-menu collapsed' data-toggle='collapse' data-target='#" + DRSubChild.MenuText + "'><a href='#'><i class='fa " + MM.Icons + " fa-lg'></i>" + DRSubChild.MenuText.ToString() + "</a></li>");
                                sb.AppendLine("<ul class='sub-menu collapse' id='" + DRSubChild.MenuText + "'>");
                                foreach (DataModel.DMMenuList DRSubSubChild in SubSubMenu)
                                {
                                sb.AppendLine("<li style='margin-left:80px'><a href='" + DRSubSubChild.Urls.ToString() + "'>" + DRSubSubChild.MenuText.ToString() + "</a></li>");
                                }
                                sb.AppendLine("</ul>");
                            }
                            else
                            {
                                sb.AppendLine("<li style='margin-left:40px'><a href='" + DRSubChild.Urls.ToString() + "'>" + DRSubChild.MenuText.ToString() + "</a></li>");
                                //sb.AppendLine("<li class='sub-sub-menu collapse' id='" + DRSubChild.MenuText + "'><a href='" + DRSubChild.Urls.ToString() + "'>" + DRSubChild.MenuText.ToString() + "</a></li>");
                                //foreach (DataModel.DMMenuList DRSubSubChild in SubSubMenu)
                                //{
                                //    sb.AppendLine("<li><a href='" + DRSubSubChild.Urls.ToString() + "'>" + DRSubSubChild.MenuText.ToString() + "</a></li>");
                                //}
                            }

                        }
                    sb.AppendLine("</ul>");
                }
                else
                {
                    foreach (DataModel.DMMenuList P in MP)
                    {
                        sb.AppendLine("<li class='sub-menu collapse' id='" + LastDepartment + "'>");
                        sb.AppendLine("<a href='" + P.Urls + "'>");
                        sb.AppendLine("<i class='fa " + P.Icons + " fa - lg'></i>" + P.MenuText);
                        sb.AppendLine("</a>");
                        sb.AppendLine("</li>");
                    }
                }

            }
            return sb.ToString();
        }
        private string LoopMenu(List<DataModel.DMMenuList> MenuParent,List<DataModel.DMMenuList>ChildMenu, bool isSub = false)
        {
            StringBuilder sb = new StringBuilder();
            if (MenuParent.Count > 0)
            {
                string LastDepartment = string.Empty;
                foreach (DataModel.DMMenuList DC in MenuParent)
                {
                    if(LastDepartment != DC.DepartmentName)
                    {
                        LastDepartment = DC.DepartmentName;
                        sb.AppendLine("<li class='header-menu collapsed' data-toggle='collapse' data-target='#" + LastDepartment +"'>");
                        sb.AppendLine("<span><a href='#'><i class='fa "+ DC.Icons +" fa-lg'></i>" + LastDepartment +"</a></span>");
                        sb.AppendLine("</li>");
                    }
                    var DR = ChildMenu.Where(x => x.ParentID == DC.MenuID && x.DepartmentName== DC.DepartmentName);
                    //List<DataModel.DMMenuList> ChMenu = new List<DataModel.DMMenuList>();
                    
                    if (DR.Count()== 0)
                    {
                       
                        sb.AppendLine("<li class='sub-menu collapse' id='" + LastDepartment + "'>");
                        sb.AppendLine("<a href='" + DC.Urls + "'>");
                        sb.AppendLine("<i class='fa " + DC.Icons + " fa - lg'></i>" + DC.MenuText);
                        sb.AppendLine("</a>");
                        sb.AppendLine("</li>");

                    }
                    else
                    {
                        //sb.AppendLine("<li class='sidebar-dropdown'>");
                        //sb.AppendLine("<a href='#'>");
                        //sb.AppendLine("<i class='fa " + DC.Icons + " fa - lg'></i>");
                        //sb.AppendLine("<span>"+ DC.MenuText +"</span></a>");
                        //sb.AppendLine("</li>");
                        sb.AppendLine("<ul class='sub-menu collapse' id='" + LastDepartment + "'>");
                        


                        foreach (DataModel.DMMenuList DChild in DR)
                        {
                            sb.AppendLine("<li><a href='" + DChild.Urls.ToString() + "'>" + DChild.MenuText.ToString() + "</a></li>");
                            var DRSubSubMenu = ChildMenu.Where(x => x.ParentID == DChild.MenuID);
                            if(DRSubSubMenu != null)
                            {
                                sb.AppendLine("<ul class='sub-menu collapse' id='" + DChild.MenuText + "'>");
                                foreach (DataModel.DMMenuList DRSubSubChild in DRSubSubMenu)
                                {
                                    sb.AppendLine("<li><a href='" + DRSubSubChild.Urls.ToString() + "'>" + DRSubSubChild.MenuText.ToString() + "</a></li>");
                                }

                                sb.AppendLine("</ul>");
                            }
                            
                        }

                        sb.AppendLine("</ul>");
                    }
                }
            }
            return sb.ToString();
        }

        #endregion

        #region Enum
        public List<DataModel.DMEnum> GetEnum(string ENum)
        {
            try {
                DBA = new DBClass();
                DataTable dt = new DataTable();
                Hashtable hst = new Hashtable();
                List<DataModel.DMEnum> Enum = new List<DataModel.DMEnum>();
                hst.Add("@EnumName", ENum);
                dt = DBA.GetDataTables("[SP_EnumLoad]", hst);
                Enum = dt.DataTableToList<DataModel.DMEnum>();
                return Enum;
                

            } catch(Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion

        #region AuthToken
        public string CreateAuthToken(List<DataModel.DMLoginDetails> data)
        {
            try {
                EncryptIT EnCrypt = new EncryptIT();
                return HttpUtility.HtmlEncode(EnCrypt.Encrypt(data[0].EmailAddress + "|||" + data[0].FullName + "|||" + data[0].DepartmentName + "|||" + data[0].LastLogin + "|||" +
                                                               data[0].Foto + "|||" + data[0].LastIP +"|||"+ data[0].RolesName +"|||"+ data[0].RoleID + "|||" + data[0].BranchID, true));
            } catch(Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public string CreateUserAuthToken(List<DataModel.DMUsersLoginDetails> data)
        {
            try
            {
                EncryptIT EnCrypt = new EncryptIT();
                return HttpUtility.HtmlEncode(EnCrypt.Encrypt(data[0].EmailAddress + "|||" + data[0].FullName + "|||" + data[0].ClientID + "|||" + data[0].LastLogin + "|||" +
                                                               data[0].Foto + "|||" + data[0].LastIP + "|||" +   data[0].RoleID + "|||" + data[0].BranchID, true));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public DataModel.DMLoginDetails GetLoginDetails(string Auth)
        {
            try {
                DataModel.DMLoginDetails DLog = new DataModel.DMLoginDetails();
                Security.EncryptIT EnCrypt = new EncryptIT();
                Auth = EnCrypt.Decrypt(HttpUtility.UrlDecode(Auth), true);
                Auth = Auth.Replace("\0", string.Empty);
                if (!string.IsNullOrEmpty(Auth))
                {
                    string[] arrToken = null;
                    if (Auth.IndexOf("|||") > 0)
                    {
                        arrToken = Auth.Split("|||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        DLog.EmailAddress= arrToken[0].ToString().Trim().Replace("NA", string.Empty);
                        DLog.FullName = arrToken[1].ToString().Trim().Replace("NA", string.Empty);
                        DLog.DepartmentName = arrToken[2].ToString().Trim().Replace("NA", string.Empty);
                        DLog.LastLogin =arrToken[3].ToString().Trim().Replace("NA", string.Empty);
                        DLog.Foto = arrToken[4].ToString().Trim().Replace("NA", string.Empty);
                        DLog.LastIP = arrToken[5].ToString().Trim().Replace("NA", string.Empty);
                        DLog.RolesName = arrToken[6].ToString().Trim().Replace("NA", string.Empty);
                        DLog.RoleID = arrToken[7].ToString().Trim().Replace("NA", string.Empty);
                        DLog.BranchID =Convert.ToInt16(arrToken[8].ToString().Trim().Replace("NA","0"));


                    }
                }
                return DLog;

            } catch(Exception ex) { throw new Exception(ex.Message.ToString()); }
        }

        #endregion

        #region GetProduct
        public List<DataModel.DMEnum> GetProducts()
        {
            try
            {
                DBA = new DBClass();
                DataTable dt = new DataTable();
                Hashtable hst = new Hashtable();
                List<DataModel.DMEnum> Enum = new List<DataModel.DMEnum>();
                
                dt = DBA.GetDataTables("[SP_T_SuperAdminProducts_Load]", hst);
                Enum = dt.DataTableToList<DataModel.DMEnum>();
                return Enum;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion

        //#region Client 



        //public string SaveClient(Hashtable hst,SqlTransaction ST, SqlConnection SC)
        //{
        //    DBA = new DBClass();

        //    try {
                
        //        return  DBA.ExecSPReturnVals("[SP_ClientMaster_Insert]", hst);
                

        //    } catch(Exception ex) { throw new Exception(ex.Message.ToString()); }
           
        //} 

        //public string CreateBilling(Hashtable hst, SqlTransaction ST, SqlConnection SC)
        //{
        //    DBA = new DBClass();

        //    try
        //    {

        //        return DBA.ExecSPReturnVals("[SP_T_BillingInsert]", hst;


        //    }
        //    catch (Exception ex) { throw new Exception(ex.Message.ToString()); }
        //}

        

        //#endregion

        #region Clear Chace
        public void ClearChace()
        {
            List<DataModel.DMMenuList> MasterMenu = new List<DataModel.DMMenuList>();
            HttpContext.Current.Cache.Insert("ChaceMenu", MasterMenu, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, 0));

            List<DataModel.DMClientMaster> CLList = new List<DataModel.DMClientMaster>();
            HttpContext.Current.Cache.Insert("ChaceClientList", CLList, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, 0));

        }
        #endregion

        #region users
        #region Client Login
        public DataModel.DMUsersLoginDetails GetClientLoginDetails(string Auth)
        {
            try
            {
                DataModel.DMUsersLoginDetails DLog = new DataModel.DMUsersLoginDetails();
                Security.EncryptIT EnCrypt = new EncryptIT();
                Auth = EnCrypt.Decrypt(HttpUtility.UrlDecode(Auth), true);
                Auth = Auth.Replace("\0", string.Empty);
                if (!string.IsNullOrEmpty(Auth))
                {
                    string[] arrToken = null;
                    if (Auth.IndexOf("|||") > 0)
                    {
                        arrToken = Auth.Split("|||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        DLog.EmailAddress = arrToken[0].ToString().Trim().Replace("NA", string.Empty);
                        DLog.FullName = arrToken[1].ToString().Trim().Replace("NA", string.Empty);
                        DLog.ClientID =Convert.ToInt32(arrToken[2].ToString().Trim().Replace("NA", string.Empty));
                        DLog.LastLogin = arrToken[3].ToString().Trim().Replace("NA", string.Empty);
                        DLog.Foto = arrToken[4].ToString().Trim().Replace("NA", string.Empty);
                        DLog.LastIP = arrToken[5].ToString().Trim().Replace("NA", string.Empty);
                        DLog.RoleID = arrToken[6].ToString().Trim().Replace("NA", string.Empty);
                        DLog.BranchID = Convert.ToInt16(arrToken[7].ToString().Trim().Replace("NA", "0"));


                    }
                }
                return DLog;

            }
            catch (Exception ex) { throw new Exception(ex.Message.ToString()); }

        }

        //public void NewCustomerInsert(Hashtable hst, SqlTransaction ST, SqlConnection SC)
        //{
        //    DBA = new DBAccess();

        //    try
        //    {

        //        DBA.ExecSP("[SP_UsersCustomerInsert]", hst, SC, ST);


        //    }
        //    catch (Exception ex) { throw new Exception(ex.Message.ToString()); }
        //}

        //public List<DataModel.DMUserCustomerList> GetCustomerList(int ClientID, string Keys)
        //{
        //    try {
        //        DBA = new DBAccess();
        //        string txt = string.Empty;
        //        DataTable Dt = new DataTable();

        //        List<DataModel.DMUserCustomerList> CL = new List<DataModel.DMUserCustomerList>();
        //        List<DataModel.DMUserCustomerList> ChaceCustomer = HttpContext.Current.Cache["ChaceUserCustomerList"] as List<DataModel.DMUserCustomerList>;

        //        if (ChaceCustomer == null || ChaceCustomer.Count == 0)
        //        {
        //            Hashtable hst = new Hashtable();
        //            hst.Add("@ClientID", ClientID);
        //            hst.Add("@Key", Keys);
        //            Dt = DBA.GetDataTables("[SP_UsersCustomerLoad]", hst);
        //            ChaceCustomer = Dt.DataTableToList<DataModel.DMUserCustomerList>();
        //            HttpContext.Current.Cache.Insert("ChaceUserCustomerList", ChaceCustomer, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
        //        }
        //        return ChaceCustomer;

        //    } catch(Exception ex) {
        //        throw new Exception("Error while loading Customer: " + ex.Message.ToString());
        //    }
        //}
        #endregion
        #endregion

        #region Invoice

        //public void ClearInvoice()
        //{
        //    List<DataModel.DMNewInvoice> ChaceInvoice =new List<DataModel.DMNewInvoice>();
        //    HttpContext.Current.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, 0));
        //}

        //public List<DataModel.DMNewInvoice> UpdateInvoiceItem(DataModel.DMNewInvoice data)
        //{
        //    List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Current.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
        //    var CI = ChaceInvoice.FirstOrDefault(x => x.NoID == data.NoID);
        //    CI.ItemName = data.ItemName;
        //    CI.ItemID = data.ItemID;
        //    CI.Descriptions = data.Descriptions;
        //    CI.Qty = data.Qty;
        //    CI.UOM = data.UOM;
        //    CI.Price = data.Price;
        //    CI.Total = data.Qty * data.Price;
        //    HttpContext.Current.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
        //    return ChaceInvoice;

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public List<DataModel.DMNewInvoice> AddItemInvoice (List<DataModel.DMNewInvoice> data)
        //{
            
        //    List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Current.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
        //    try {
        //        if (ChaceInvoice == null || ChaceInvoice.Count == 0)
        //        {
        //            DataModel.DMNewInvoice NV = new DataModel.DMNewInvoice();
        //            ChaceInvoice = new List<DataModel.DMNewInvoice>();
        //            ChaceInvoice.Add(new DataModel.DMNewInvoice() { NoID = data[0].NoID, ItemID = 0, ItemName = string.Empty, Descriptions = string.Empty, Price = 0, Qty = 1, Total = 0, UOM = string.Empty });

        //            HttpContext.Current.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
        //        }
        //        else
        //        {
        //            bool Addrow = false;
        //            foreach (DataModel.DMNewInvoice DI in data)
        //            {
        //                Addrow = false;
        //                if (DI.ItemName != null && DI.Qty!=0 && DI.Price !=0)
        //                {
        //                    Addrow = true;
        //                    var CI = ChaceInvoice.FirstOrDefault(x => x.NoID == DI.NoID);
        //                    CI.ItemName = DI.ItemName;
        //                    CI.ItemID = DI.ItemID;
        //                    CI.Descriptions = DI.Descriptions;
        //                    CI.Qty = DI.Qty;
        //                    CI.UOM = DI.UOM;
        //                    CI.Price = DI.Price;
        //                    CI.Total = DI.Qty * DI.Price;
        //                }
        //            }

        //            if (Addrow == true)
        //            {
        //                DataModel.DMNewInvoice NI = new DataModel.DMNewInvoice();
        //                NI.NoID = ChaceInvoice.Count() + 1;
        //                NI.Qty = 1;

        //                ChaceInvoice.Add(NI);
        //            }


        //            HttpContext.Current.Cache.Insert("ChaceInvoice", ChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));

        //        }

        //    } catch { }
            
        //    return ChaceInvoice;
        //}

        //public List<DataModel.DMNewInvoice> DeleteItemInvoice(int ID)
        //{
        //    int i = 1;
        //    List<DataModel.DMNewInvoice> ChaceInvoice = HttpContext.Current.Cache["ChaceInvoice"] as List<DataModel.DMNewInvoice>;
        //    List<DataModel.DMNewInvoice> TmpChaceInvoice = new List<DataModel.DMNewInvoice>();
        //    foreach(DataModel.DMNewInvoice DI in ChaceInvoice)
        //    {
        //        if(DI.NoID != ID)
        //        {
        //            DataModel.DMNewInvoice NI = new DataModel.DMNewInvoice();
        //            NI.NoID = i;
        //            NI.ItemID = DI.ItemID;
        //            NI.ItemName = DI.ItemName;
        //            NI.Descriptions = DI.Descriptions;
        //            NI.Qty = DI.Qty;
        //            NI.UOM = DI.UOM;
        //            NI.Price = DI.Price;
        //            NI.Total = DI.Qty * DI.Price;
        //            TmpChaceInvoice.Add(NI);
        //            i++;
        //        }
        //    }
        //    HttpContext.Current.Cache.Insert("ChaceInvoice", TmpChaceInvoice, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
        //    return TmpChaceInvoice;
        //}
        #endregion
    }
}
