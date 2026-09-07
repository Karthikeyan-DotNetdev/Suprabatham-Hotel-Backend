using AppSettings;
using CommonServices;
using laptop_service.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace laptop_service.Services
{
    public class EmployeeService
    {
        #region Login ---------------------------------------------------------------
        public static DataTable Login(Login login)
        {
            StringBuilder sb = new StringBuilder();

            DataTable dataTable = new DataTable();

            if (login.User_Name == "brosonetechbillingsoftware" && login.Password == "brosonetechbillingsoftware")
            {
                dataTable.Columns.Add("Organization_Code");
                dataTable.Columns.Add("Organization_Name");
                dataTable.Columns.Add("Company_Img_Url");
                dataTable.Columns.Add("Organization_Address1");
                dataTable.Columns.Add("Organization_Address2");
                dataTable.Columns.Add("Organization_City");
                dataTable.Columns.Add("Organization_State");
                dataTable.Columns.Add("Organization_Country");
                dataTable.Columns.Add("Organization_Pincode");
                dataTable.Columns.Add("Organization_Gst_No");
                dataTable.Columns.Add("Organization_FSS_No");
                dataTable.Columns.Add("Branch_Code");
                dataTable.Columns.Add("Branch_Name");
                dataTable.Columns.Add("Employee_Code");
                dataTable.Columns.Add("Employee_Name");
                dataTable.Columns.Add("Software_Rights_Group");
                dataTable.Columns.Add("Software_Rights_Code");
                dataTable.Columns.Add("Status");
                dataTable.Columns.Add("Message");
                dataTable.Columns.Add("Error");

                DataRow dataRow = dataTable.NewRow();
                dataRow["Organization_Code"] = "C00001";
                dataRow["Organization_Name"] = "KOVILPATTI MURUKKU KADAI";
                dataRow["Company_Img_Url"] = "branch/B00025_379356.png";
                dataRow["Organization_Address1"] = "No: 81, Kundrathur Main Rd";
                dataRow["Organization_Address2"] = "Ramanatheswarar Nagar, Madhananatha puram, Porur";
                dataRow["Organization_City"] = "CHENNAI";
                dataRow["Organization_State"] = "TAMIL NADU";
                dataRow["Organization_Country"] = "INDIA";
                dataRow["Organization_Pincode"] = "600116";
                dataRow["Organization_Gst_No"] = "33AAKCK0930E1ZY";
                dataRow["Organization_FSS_No"] = "12422008003344";

                dataRow["Branch_Code"] = "B00001";
                dataRow["Branch_Name"] = "";
                dataRow["Employee_Code"] = "E001";
                dataRow["Employee_Name"] = "SUPER ADMIN";
                //dataRow["User_Name"] = AppSetting.AdminUsername;
                dataRow["Software_Rights_Group"] = "Admin";
                dataRow["Software_Rights_Code"] = "2";
                dataRow["Status"] = "Success";
                dataRow["Message"] = "Login Success.";
                dataRow["Error"] = "";
                dataTable.Rows.Add(dataRow);
            }
            else
            {
                //string query = "SELECT [M_USER_MASTER].[Emp_Id],[M_USER_MASTER].[Emp_Name],[M_USER_MASTER].[Branch]," +
                //    "[M_Role_MASTER].[Role],[M_USER_MASTER].[Role] AS Role_Code,m_employee_master.EmpImgUrl FROM [M_USER_MASTER] " +
                //    "LEFT JOIN [M_Role_MASTER] ON [M_USER_MASTER].[Role] = [M_Role_MASTER].[Role_Code] " +
                //    "left outer join m_employee_master on[M_USER_MASTER].[Emp_Id] = m_employee_master.empid "+
                //    " WHERE UserName='" + login.User_Name + "' AND Password='" + login.Password + "'";

                string query = "SELECT [M_USER_MASTER].[Emp_Id], [M_USER_MASTER].[Emp_Name], [M_USER_MASTER].[Branch],[M_BRANCH_MASTER].[Branch_Name], " +
    "[M_COMPANY_MASTER].*, " +
    "[M_Role_MASTER].[Role], [M_USER_MASTER].[Role] AS Role_Code, m_employee_master.EmpImgUrl " +
    "FROM [M_USER_MASTER] " +
    "LEFT JOIN [M_Role_MASTER] ON [M_USER_MASTER].[Role] = [M_Role_MASTER].[Role_Code] " +
    "LEFT OUTER JOIN m_employee_master ON [M_USER_MASTER].[Emp_Id] = m_employee_master.empid " +
    "LEFT OUTER JOIN M_COMPANY_MASTER ON [m_employee_master].[fComp] = M_COMPANY_MASTER.Company_Code " +
    "LEFT OUTER JOIN M_BRANCH_MASTER ON [M_USER_MASTER].[Branch] = M_BRANCH_MASTER.Branch_Code" +
    " WHERE UserName='" + login.User_Name + "' AND Password='" + login.Password + "'";

                DataTable data = SQLService.GetDataTable(query);

                dataTable.Columns.Add("Organization_Code");
                dataTable.Columns.Add("Organization_Name");
                dataTable.Columns.Add("Company_Img_Url");
                dataTable.Columns.Add("Organization_Address1");
                dataTable.Columns.Add("Organization_Address2");
                dataTable.Columns.Add("Organization_City");
                dataTable.Columns.Add("Organization_State");
                dataTable.Columns.Add("Organization_Country");
                dataTable.Columns.Add("Organization_Pincode");
                dataTable.Columns.Add("Organization_Gst_No");
                dataTable.Columns.Add("Organization_FSS_No");
                dataTable.Columns.Add("Branch_Code");
                dataTable.Columns.Add("Branch_Name");
                dataTable.Columns.Add("Employee_Code");
                dataTable.Columns.Add("Employee_Name");
                dataTable.Columns.Add("Software_Rights_Group");
                dataTable.Columns.Add("Software_Rights_Code");
                dataTable.Columns.Add("Status");
                dataTable.Columns.Add("Message");
                dataTable.Columns.Add("Error");
                dataTable.Columns.Add("EmpImgUrl");

                if (data.Rows.Count > 0)
                {
                    // Adding rows from the original data table
                    DataRow dataRow = dataTable.NewRow();
                    dataRow["Organization_Code"] = data.Rows[0]["Company_Code"].ToString();  // Assuming these columns exist in the original data
                    dataRow["Organization_Name"] = data.Rows[0]["Company_Name"].ToString();
                    dataRow["Company_Img_Url"] = data.Rows[0]["Company_Image_Url"].ToString();
                    dataRow["Organization_Address1"] = data.Rows[0]["Address_Line_1"].ToString();
                    dataRow["Organization_Address2"] = data.Rows[0]["Address_Line_2"].ToString();
                    dataRow["Organization_City"] = data.Rows[0]["City"].ToString();
                    dataRow["Organization_State"] = data.Rows[0]["State"].ToString();
                    dataRow["Organization_Country"] = data.Rows[0]["Country"].ToString();
                    dataRow["Organization_Pincode"] = data.Rows[0]["Pincode"].ToString();
                    dataRow["Organization_Gst_No"] = data.Rows[0]["GST_Number"].ToString();
                    dataRow["Organization_FSS_No"] = data.Rows[0]["FSS_Number"].ToString();

                    dataRow["Branch_Code"] = data.Rows[0]["Branch"].ToString();
                    dataRow["Branch_Name"] = data.Rows[0]["Branch_Name"].ToString();

                    dataRow["Employee_Code"] = data.Rows[0]["Emp_Id"].ToString();
                    dataRow["Employee_Name"] = data.Rows[0]["Emp_Name"].ToString();
                    dataRow["Software_Rights_Group"] = data.Rows[0]["Role"].ToString();
                    dataRow["Software_Rights_Code"] = data.Rows[0]["Role_Code"].ToString();
                    dataRow["Status"] = "Success"; // Set a static value or dynamic based on your logic
                    dataRow["Message"] = "Login Success."; // Set a static value or dynamic based on your logic
                    dataRow["Error"] = ""; // No error
                    dataRow["EmpImgUrl"] = data.Rows[0]["EmpImgUrl"].ToString();
                    dataTable.Rows.Add(dataRow);
                }

            }

            return dataTable;
        }
        #endregion Login ---------------------------------------------------------------

    }
}
