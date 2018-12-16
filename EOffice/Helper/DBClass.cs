using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Helper
{
    public class DBClass
    {
        String MyConn = string.Empty;
        
        SqlConnection SqlCon;
        SqlTransaction SqlTrans;
        string strCon = System.Configuration.ConfigurationManager.ConnectionStrings["conDB"].ToString();

        #region SQLConnetcion

        public void OpenSQL()
        {
            
            try
            {
                
                SqlCon = new SqlConnection(strCon);
                SqlCon.Open();
                
                SqlTrans  =SqlCon.BeginTransaction();
            }
            catch (Exception ex) { throw new Exception(ex.Message.ToString()); }

        }

        public void CommitTransaction()
        {
            SqlTrans.Commit();
            SqlTrans.Dispose();
            SqlCon.Close();
            SqlCon.Dispose();
        }
        public void RollBackTransaction()
        {
            SqlTrans.Rollback();
            SqlTrans.Dispose();
            SqlCon.Close();
            SqlCon.Dispose();
        }
        #endregion

        #region Function
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public string GetVals(string sp, Hashtable hash)
        {
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

        public void ExecSP(string sp, Hashtable hash)
        {

            OpenSQL();
            SqlCommand objCmd = new SqlCommand(sp, SqlCon, SqlTrans);
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
                RollBackTransaction();
                throw new Exception(sqlEx.Message);

            }
            catch (Exception ex)
            {
                RollBackTransaction();
                throw new Exception("Execute Query Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
                CommitTransaction();
            }
        }

        public string  ExecSPReturnVals(string sp, Hashtable hash)
        {

            if (SqlCon.State != ConnectionState.Open)
            {
                OpenSQL();
            }
            SqlCommand objCmd = new SqlCommand(sp, SqlCon, SqlTrans);
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
                throw new Exception("Execute Query Error : " + ex.Message);

            }
            finally
            {
                objDa.Dispose();
                objCmd.Dispose();
            }
        }

        public DataTable GetDataTables(string sp, Hashtable hash)
        {
            
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
        #endregion


    }
}
