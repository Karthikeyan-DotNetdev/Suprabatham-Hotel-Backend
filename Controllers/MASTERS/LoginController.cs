using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Mvc;
using CommonServices;
using System.Data;

namespace laptop_service.Controllers.MASTERS
{
    [Route("api/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
                {
                    return Ok(new
                    {
                        status = false,
                        message = "Username and Password are required."
                    });
                }

                string query = $@"
                    SELECT 
                        E.Employee_Code AS userno,
                        E.Login_Id AS username,
                        ISNULL(TRY_CAST(SUBSTRING(R.Role_Code, 4, 10) AS INT), 0) AS role,
                        E.Role AS RoleName,
                        E.Phone_No AS mobile,
                        '' AS email,
                        E.Employee_Name AS EmployeeName,
                        E.Is_Active AS status
                    FROM Employee_Master E
                    LEFT JOIN Role_Master R ON UPPER(TRIM(E.Role)) = UPPER(TRIM(R.Role_Name))
                    WHERE UPPER(TRIM(E.Login_Id)) = UPPER(TRIM('{EscapeSql(model.UserName)}'))
                    AND TRIM(E.Password) = TRIM('{EscapeSql(model.Password)}')
                    AND (E.Is_Active = 'A' OR E.Is_Active IS NULL OR E.Is_Active = '')
                ";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new
                    {
                        status = false,
                        message = "Invalid Username or Password"
                    });
                }

                DataRow row = dt.Rows[0];

                return Ok(new
                {
                    status = true,
                    message = "Login Success",
                    user = new
                    {
                        UserNo = row["userno"].ToString(),
                        UserName = row["username"].ToString(),
                        RoleId = Convert.ToInt32(row["role"]),
                        RoleName = row["RoleName"].ToString(),
                        Mobile = row["mobile"].ToString(),
                        Email = row["email"].ToString(),
                        EmployeeName = row["EmployeeName"].ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        private static string EscapeSql(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Replace("'", "''");
        }
    }
}


namespace laptop_service.Models.MASTERS
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}