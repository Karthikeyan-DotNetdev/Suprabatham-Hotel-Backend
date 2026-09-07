using CommonModels;
using CommonServices;
using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using WIN_BOT;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/employee")]
    [ApiController]
    public class Employee_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee(EMPLOYEE_MASTER employee)
        {
            try
            {
                JObject jobject = new JObject();

                employee.Employee_Name = employee.Employee_Name?.Trim();
                employee.Phone_No = employee.Phone_No?.Trim();

                if (string.IsNullOrWhiteSpace(employee.Employee_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Employee Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Employee_Master
                WHERE UPPER(Employee_Name)=UPPER('{employee.Employee_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Employee Name already exists.");
                    return Ok(jobject.ToString());
                }

                employee = await LoadEmployeeMaxCode(employee);

                var data = new EMPLOYEE_MASTER
                {
                    Employee_Code = employee.Employee_Code,
                    Employee_Name = employee.Employee_Name.ToUpper(),
                    Phone_No = employee.Phone_No,
                    Role = employee.Role,
                    Is_Active = employee?.Is_Active ?? "A",
                    Login_Id = employee.Login_Id,
                    Password = employee.Password,
                    Created_By = employee.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>()
                {
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "Employee_Master",
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Saved Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save");
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        [HttpPost]
        [Route("UpdateEmployee/{Employee_Code}")]
        public IActionResult UpdateEmployee(string Employee_Code, EMPLOYEE_MASTER employee)
        {
            try
            {
                JObject jobject = new JObject();

                employee.Employee_Name = employee.Employee_Name?.Trim();
                employee.Phone_No = employee.Phone_No?.Trim();

                if (string.IsNullOrWhiteSpace(employee.Employee_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Employee Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Employee_Master
                WHERE UPPER(Employee_Name)=UPPER('{employee.Employee_Name}')
                AND Employee_Code<>'{Employee_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Employee Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new EMPLOYEE_MASTER
                {
                    Employee_Code = Employee_Code,
                    Employee_Name = employee.Employee_Name.ToUpper(),
                    Phone_No = employee.Phone_No,
                    Role = employee.Role,
                    Is_Active = employee.Is_Active,
                    Login_Id = employee.Login_Id,
                    Password = employee.Password,
                    Updated_By = employee.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Employee_Master",
                    "Employee_Code",
                    Employee_Code,
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update");
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        [HttpGet]
        [Route("DeleteEmployee/{Employee_Code}")]
        public IActionResult DeleteEmployee(string Employee_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Employee_Master
            SET Is_Active='D'
            WHERE Employee_Code='{Employee_Code}'";

            int result = SQLService.ExecuteNonQuery(query);

            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Deleted Successfully");
            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete");
            }

            return Ok(jobject.ToString());
        }

        [HttpGet]
        [Route("EmployeeList")]
        public IActionResult EmployeeList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_EmployeeList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<EMPLOYEE_MASTER> LoadEmployeeMaxCode(EMPLOYEE_MASTER employee)
        {
            DataTable dt = SQLService.GetDataTable(@"
    SELECT
    ISNULL(MAX(CAST(REPLACE(Employee_Code,'EMP','') AS INT)),0)+1
    FROM Employee_Master");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                employee.Employee_Code = "EMP" + code.ToString().PadLeft(5, '0');
            }

            return employee;
        }
    }
}