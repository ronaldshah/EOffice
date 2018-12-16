using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;


namespace Security
{
    public class DBAccess
    {
        String MyConn = string.Empty;
        SqlConnection SQLCon = new SqlConnection();
        SqlConnection SC;
        SqlTransaction ST;
        string cnStr = System.Configuration.ConfigurationManager.ConnectionStrings["conDB"].ToString();


        public SqlConnection OpenSQL()
        {
            SqlConnection objCon;
            try
            {


                string res = string.Empty;
                objCon = new SqlConnection(cnStr);
                objCon.Open();
                return objCon;

            }
            catch (Exception ex) { throw new Exception(ex.Message.ToString()); }

        }

        public void CommitSP(SqlTransaction ST, SqlConnection SC)
        {
            try
            {

                ST.Commit();
                SC.Close();
                ST.Dispose();
                SC.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

        }
        public void CancelSP(SqlTransaction ST, SqlConnection SC)
        {
            try
            {

                ST.Rollback();
                SC.Close();
                ST.Dispose();
                SC.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public string GetVals(string sp, Hashtable hash, string strCon = "")
        {
            //strCon = Encryptions.Decrypt(strCon, false);
            if (strCon == "") { strCon = cnStr; }
            string res = string.Empty;
            SqlConnection objCon = new SqlConnection(strCon);
            SqlCommand objCmd = new SqlCommand(sp, objCon);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCon.Open();
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);
                        objCmd.Parameters[k].Value = e.Value;
                    }
                }
                var rest = objCmd.ExecuteScalar();
                return rest.ToString();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {
                throw new Exception("Get DataTable Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
                objCon.Close();
                objCon.Dispose();
            }
        }

        public string GetVals(string sp, Hashtable hash, SqlConnection SC, SqlTransaction ST)
        {
            //strCon = Encryptions.Decrypt(strCon, false);

            string res = string.Empty;

            SqlCommand objCmd = new SqlCommand(sp, SC, ST);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);
                        objCmd.Parameters[k].Value = e.Value;
                    }
                }
                var rest = objCmd.ExecuteScalar();
                return rest.ToString();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {
                throw new Exception("Get DataTable Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
            }
        }


       
       
        public void ExecSP(string sp, Hashtable hash, string strCon = "")
        {
            if (strCon == "") { strCon = cnStr; }
            //strCon = Encryptions.Decrypt(strCon, false);
            SqlConnection objCon = new SqlConnection(strCon);
            SqlCommand objCmd = new SqlCommand(sp, objCon);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCon.Open();
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);

                        objCmd.Parameters[k].Value = e.Value;
                    }

                }
                objCmd.ExecuteNonQuery();

            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {
                throw new Exception("Get DataTable Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
                objCon.Close();
                objCon.Dispose();
            }
        }

        public void ExecSP(string sp, Hashtable hash, SqlConnection SC, SqlTransaction ST)
        {

            //strCon = Encryptions.Decrypt(strCon, false);
            SqlCommand objCmd = new SqlCommand(sp, SC, ST);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);

                        objCmd.Parameters[k].Value = e.Value;
                    }

                }
                objCmd.ExecuteNonQuery();

            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {
                throw new Exception("Get DataTable Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
            }
        }


        public DataTable GetDataTables(string sp, Hashtable hash, string strCon = "")
        {
            if (strCon == "") { strCon = cnStr; }
            //strCon = Encryptions.Decrypt(strCon, false);
            SqlConnection objCon = new SqlConnection(strCon);
            SqlCommand objCmd = new SqlCommand(sp, objCon);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCon.Open();
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);

                        objCmd.Parameters[k].Value = e.Value;
                    }

                }
                objDt.Clear();
                objDa.SelectCommand = objCmd;
                objDa.Fill(objDt);
                return objDt;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception(sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Get DataTable Error : " + ex.Message);
            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
                objCon.Close();
                objCon.Dispose();
            }

        }

        public string InsertGetValues(string sp, Hashtable hash, SqlConnection SC, SqlTransaction ST)
        {
            SqlCommand objCmd = new SqlCommand(sp, SC, ST);
            SqlDataAdapter objDa = new SqlDataAdapter();
            DataTable objDt = new DataTable("Data");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                if ((hash != null))
                {
                    SqlCommandBuilder.DeriveParameters(objCmd);
                    foreach (DictionaryEntry e in hash)
                    {
                        string k = Convert.ToString(e.Key);
                        objCmd.Parameters[k].Value = e.Value;
                    }
                }
                var rest = objCmd.ExecuteScalar();
                if (rest == null)
                {
                    return string.Empty;
                }
                else
                {
                    return rest.ToString();
                }

            }
            catch (SqlException sqlEx)
            {

                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {

                throw new Exception("Get DataTable Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();

            }
        }

    }
}
