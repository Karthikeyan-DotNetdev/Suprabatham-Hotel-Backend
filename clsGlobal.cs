using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WIN_BOT
{
    internal class clsGlobal
    {        
        public static DataTable getPurchaseDatatable()
        {
            DataTable ProductData = new DataTable();
            ProductData.Columns.Add("sno", typeof(int));
            ProductData.Columns.Add("barcode", typeof(string));
            ProductData.Columns.Add("product_name", typeof(string));
            ProductData.Columns.Add("uom", typeof(string));
            ProductData.Columns.Add("hsn", typeof(string));
            ProductData.Columns.Add("bag_qty", typeof(string));
            ProductData.Columns.Add("loose_qty", typeof(string));
            ProductData.Columns.Add("qty", typeof(string));
            ProductData.Columns.Add("free_qty", typeof(string));
            ProductData.Columns.Add("mrp", typeof(string));
            ProductData.Columns.Add("trade_disc", typeof(string));
            ProductData.Columns.Add("cost", typeof(string));
            ProductData.Columns.Add("charges", typeof(string));
            ProductData.Columns.Add("disc_per", typeof(string));
            ProductData.Columns.Add("less_amount", typeof(string));
            ProductData.Columns.Add("gst", typeof(string));
            ProductData.Columns.Add("cess", typeof(string));
            ProductData.Columns.Add("landing_cost", typeof(string));
            ProductData.Columns.Add("profit_per", typeof(string));
            ProductData.Columns.Add("profit_amount", typeof(string));
            ProductData.Columns.Add("selling_rate", typeof(string));
            ProductData.Columns.Add("retail_rate", typeof(string));
            ProductData.Columns.Add("wholesale_rate", typeof(string));
            ProductData.Columns.Add("sale_disc", typeof(string));
            ProductData.Columns.Add("sale_less", typeof(string));
            ProductData.Columns.Add("rack", typeof(string));
            ProductData.Columns.Add("tray", typeof(string));
            ProductData.Columns.Add("lot_number", typeof(string));
            ProductData.Columns.Add("mfd_date", typeof(string));
            ProductData.Columns.Add("expiry_in_days", typeof(string));
            ProductData.Columns.Add("expiry_date", typeof(string));
            ProductData.Columns.Add("serial_no", typeof(string));
            ProductData.Columns.Add("model_no", typeof(string));
            ProductData.Columns.Add("imei_no_one", typeof(string));
            ProductData.Columns.Add("imei_no_two", typeof(string));
            ProductData.Columns.Add("size_name", typeof(string));
            ProductData.Columns.Add("color_name", typeof(string));
            ProductData.Columns.Add("fit_name", typeof(string));
            ProductData.Columns.Add("prev_cost", typeof(string));
            ProductData.Columns.Add("prev_mrp", typeof(string));
            ProductData.Columns.Add("amount", typeof(string));
            ProductData.Columns.Add("remove", typeof(string));
            ProductData.Columns.Add("sub_total", typeof(string));
            ProductData.Columns.Add("disc_value", typeof(string));
            ProductData.Columns.Add("taxable", typeof(string));
            ProductData.Columns.Add("gst_value", typeof(string));
            ProductData.Columns.Add("cgst_per", typeof(string));
            ProductData.Columns.Add("sgst_per", typeof(string));
            ProductData.Columns.Add("igst_per", typeof(string));
            ProductData.Columns.Add("cgst_value", typeof(string));
            ProductData.Columns.Add("sgst_value", typeof(string));
            ProductData.Columns.Add("igst_value", typeof(string));
            ProductData.Columns.Add("cess_value", typeof(string));
            ProductData.Columns.Add("size_code", typeof(string));
            ProductData.Columns.Add("color_code", typeof(string));
            ProductData.Columns.Add("fit_code", typeof(string));
            ProductData.Columns.Add("mrp_total", typeof(string));
            ProductData.Columns.Add("selling_total", typeof(string));
            ProductData.Columns.Add("retail_total", typeof(string));
            ProductData.Columns.Add("wholesale_total", typeof(string));
            ProductData.Columns.Add("landing_cost_total", typeof(string));
            ProductData.Columns.Add("unit_range", typeof(string));
            ProductData.Columns.Add("converted_qty", typeof(string));
            ProductData.Columns.Add("gst_type", typeof(string));
            ProductData.Columns.Add("product_code", typeof(string));
            //For Adding New Row
            DataRow newRow = ProductData.NewRow();
            newRow["sno"] = ProductData.Rows.Count + 1;
            ProductData.Rows.Add(newRow);
            ProductData.AcceptChanges();
            return ProductData;
        }
        public static DataTable getPurchaseAdditionalChargeDatatable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ad_sno", typeof(int));
            dt.Columns.Add("ad_acc_code", typeof(string));
            dt.Columns.Add("ad_acc_name", typeof(string));
            dt.Columns.Add("ad_hsn_code", typeof(string));
            dt.Columns.Add("ad_amount", typeof(string));
            dt.Columns.Add("ad_gst_per", typeof(string));
            dt.Columns.Add("ad_gst_value", typeof(string));
            dt.Columns.Add("ad_total_amount", typeof(string));
            dt.Columns.Add("ad_cgst_per", typeof(string));
            dt.Columns.Add("ad_cgst_value", typeof(string));
            dt.Columns.Add("ad_sgst_per", typeof(string));
            dt.Columns.Add("ad_sgst_value", typeof(string));
            dt.Columns.Add("ad_igst_per", typeof(string));
            dt.Columns.Add("ad_igst_value", typeof(string));
            dt.Columns.Add("ad_cess_per", typeof(string));
            dt.Columns.Add("ad_cess_value", typeof(string));
            //For Adding New Row
            //DataRow newRow = dt.NewRow();
            //newRow["ad_sno"] = dt.Rows.Count + 1;
            //dt.Rows.Add(newRow);
            //dt.AcceptChanges();
            return dt;
        }
        public static DataTable getSalesDatatable()
        {
            DataTable ProductData = new DataTable();
            ProductData.Columns.Add("sno", typeof(int));
            ProductData.Columns.Add("barcode", typeof(string));
            ProductData.Columns.Add("product_name", typeof(string));
            ProductData.Columns.Add("uom_name", typeof(string));
            ProductData.Columns.Add("uom_code", typeof(string));
            ProductData.Columns.Add("hsn", typeof(string));
            ProductData.Columns.Add("qty", typeof(string));
            ProductData.Columns.Add("mrp", typeof(string));
            ProductData.Columns.Add("cost", typeof(string));
            ProductData.Columns.Add("rate", typeof(string));
            ProductData.Columns.Add("charges", typeof(string));
            ProductData.Columns.Add("disc_per", typeof(string));
            ProductData.Columns.Add("less_amount", typeof(string));
            ProductData.Columns.Add("gst", typeof(string));
            ProductData.Columns.Add("cess", typeof(string));
            ProductData.Columns.Add("landing_cost", typeof(string));
            ProductData.Columns.Add("rack", typeof(string));
            ProductData.Columns.Add("tray", typeof(string));
            ProductData.Columns.Add("lot_number", typeof(string));
            ProductData.Columns.Add("mfd_date", typeof(string));
            ProductData.Columns.Add("expiry_in_days", typeof(string));
            ProductData.Columns.Add("expiry_date", typeof(string));
            ProductData.Columns.Add("serial_no", typeof(string));
            ProductData.Columns.Add("model_no", typeof(string));
            ProductData.Columns.Add("imei_no_one", typeof(string));
            ProductData.Columns.Add("imei_no_two", typeof(string));
            ProductData.Columns.Add("size_name", typeof(string));
            ProductData.Columns.Add("color_name", typeof(string));
            ProductData.Columns.Add("fit_name", typeof(string));
            ProductData.Columns.Add("amount", typeof(string));
            ProductData.Columns.Add("remove", typeof(string));
            ProductData.Columns.Add("notes", typeof(string));
            ProductData.Columns.Add("sub_total", typeof(string));
            ProductData.Columns.Add("disc_value", typeof(string));
            ProductData.Columns.Add("taxable", typeof(string));
            ProductData.Columns.Add("gst_value", typeof(string));
            ProductData.Columns.Add("cgst_per", typeof(string));
            ProductData.Columns.Add("sgst_per", typeof(string));
            ProductData.Columns.Add("igst_per", typeof(string));
            ProductData.Columns.Add("cgst_value", typeof(string));
            ProductData.Columns.Add("sgst_value", typeof(string));
            ProductData.Columns.Add("igst_value", typeof(string));
            ProductData.Columns.Add("cess_value", typeof(string));
            ProductData.Columns.Add("sales_man", typeof(string));
            ProductData.Columns.Add("size_code", typeof(string));
            ProductData.Columns.Add("color_code", typeof(string));
            ProductData.Columns.Add("fit_code", typeof(string));
            ProductData.Columns.Add("mrp_total", typeof(string));
            ProductData.Columns.Add("cost_total", typeof(string));
            ProductData.Columns.Add("rate_total", typeof(string));
            ProductData.Columns.Add("landing_cost_total", typeof(string));
            ProductData.Columns.Add("rate_type", typeof(string));
            ProductData.Columns.Add("unit_range", typeof(string));
            ProductData.Columns.Add("converted_qty", typeof(string));
            ProductData.Columns.Add("gst_type", typeof(string));
            ProductData.Columns.Add("product_code", typeof(string));
            ProductData.Columns.Add("row_status", typeof(string));
            //For Adding New Row
            //DataRow newRow = ProductData.NewRow();
            //newRow["sno"] = ProductData.Rows.Count + 1;
            //ProductData.Rows.Add(newRow);
            //ProductData.AcceptChanges();
            return ProductData;
        }
        public static (DateTime First, DateTime Last) Get_First_Last_Dateof_Month()
        {
            //Get First Day of Month
            DateTime First = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // Get the Current Date
            DateTime now = DateTime.Now;
            // Get the last day of the current month
            int lastDay = DateTime.DaysInMonth(now.Year, now.Month);
            //Get Last day of Month
            DateTime Last = new DateTime(now.Year, now.Month, lastDay);
            return (First, Last);
        }
        public static string DisplayIndianCurrency(string Amount)
        {
            decimal parsed = decimal.Parse(Amount, CultureInfo.InvariantCulture);
            CultureInfo hindi = new CultureInfo("hi-IN");
            string text = string.Format(hindi, "{0:c}", parsed);
            return text;
        }
        public static string Encrypt(string textToEncrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "dhanasek";
                string secretkey = "dhanaprk";
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
        public static string Decrypt(string textToDecrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "dhanasek";
                string privatekey = "dhanaprk";
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(privatekey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }
        public static T RemoveNullProperties<T>(T obj)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Where(prop => prop.CanWrite && prop.CanRead);

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (value == null)
                {
                    var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                    property.SetValue(obj, defaultValue);
                }
            }
            return obj;
        }
        //public static void InsertUserTracking()
        //{
        //    try
        //    {
        //        if (connectionString.State == ConnectionState.Closed) { connectionString.Open(); }
        //        SqlCommand cmdu = new SqlCommand() { CommandText = "sp_insertusertracking", CommandType = CommandType.StoredProcedure, Connection = connectionString };
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@date", SqlDbType = SqlDbType.DateTime, Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt") });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@usercode", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.LoggedUserCode });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@username", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.LoggedUserName });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@module", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.module });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@operation", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.operation });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@notes", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.msg });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@branchcode", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.BranchCode });
        //        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@branchname", SqlDbType = SqlDbType.VarChar, Value = clsGlobal.BranchName });
        //        cmdu.ExecuteNonQuery();
        //    }
        //    catch (Exception Ex) { Console.WriteLine(Ex.ToString()); }
        //}           

    }
}
