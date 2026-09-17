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
        // =========================================================================
        // 1. POST: /api/Login (User Authentication + Shift Session Start)
        // =========================================================================
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

                string userNameEscaped = EscapeSql(model.UserName.Trim());
                string passwordEscaped = EscapeSql(model.Password.Trim());

                // 1. Check user credentials in Employee_Master
                // 1. Check user credentials in Employee_Master
                string userQuery = $@"
                    SELECT TOP 1
                        E.Employee_Code AS userno,
                        E.Login_Id AS username,
                        0 AS roleId,
                        ISNULL(E.Role, 'CASHIER') AS RoleName,
                        ISNULL(E.Phone_No, '') AS mobile,
                        '' AS email,
                        ISNULL(E.Employee_Name, E.Login_Id) AS EmployeeName,
                        ISNULL((SELECT TOP 1 Branch_Code FROM Branch_Master WHERE Is_Active = 'A'), '') AS Branch_Code,
                        E.Is_Active AS status
                    FROM Employee_Master E
                    WHERE UPPER(TRIM(E.Login_Id)) = UPPER('{userNameEscaped}')
                      AND TRIM(E.Password) = '{passwordEscaped}'
                      AND (E.Is_Active = 'A' OR E.Is_Active IS NULL OR E.Is_Active = '');
                ";


                DataTable dt = SQLService.GetDataTable(userQuery);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new
                    {
                        status = false,
                        message = "Invalid Username or Password."
                    });
                }

                DataRow row = dt.Rows[0];
                string userNo = row["userno"].ToString() ?? "";
                string username = row["username"].ToString() ?? "";
                string roleName = (row["RoleName"].ToString() ?? "CASHIER").Trim().ToUpper();
                string employeeName = row["EmployeeName"].ToString() ?? "";
                string branchCode = row["Branch_Code"].ToString() ?? "";
                string deviceType = !string.IsNullOrWhiteSpace(model.DeviceType) ? model.DeviceType.Trim() : "DESKTOP_POS";

                // 2. Define Role-Based Permissions Matrix
                // CASHIER / ADMIN gets full access; CAPTAIN / WAITER gets ONLY /pos
                bool isCaptainOrWaiter = (roleName == "CAPTAIN" || roleName == "WAITER" || roleName == "CAPTAIN / WAITER");
                bool canAccessMasters = !isCaptainOrWaiter;
                bool canAccessReports = !isCaptainOrWaiter;
                bool canAccessSettings = !isCaptainOrWaiter;
                string landingRoute = "/pos";
                string[] allowedRoutes = isCaptainOrWaiter
                    ? new string[] { "/pos" }
                    : new string[] { "*" }; // "*" means all routes allowed

                // 3. Create Shift Session Log in Database
                long sessionId = 0;
                string loginTimeFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                try
                {
                    // Step 1: Auto-close any previously ACTIVE sessions for this employee
                    // Power cut / browser crash aanaal varum Ghost Sessions-ai ithu thadukkum
                    string closeOldSessionsQuery = $@"
                        UPDATE dbo.User_Shift_Session_Log
                        SET Shift_Status  = 'CLOSED',
                            Logout_Time   = GETDATE(),
                            Total_Minutes = DATEDIFF(MINUTE, Login_Time, GETDATE())
                        WHERE User_Code    = '{EscapeSql(userNo)}'
                          AND Shift_Status = 'ACTIVE';
                    ";
                    SQLService.ExecuteNonQuery(closeOldSessionsQuery);

                    // Step 2: Insert new fresh session row
                    string sessionQuery = $@"
                        INSERT INTO dbo.User_Shift_Session_Log 
                            (User_Code, User_Name, Role_Name, Branch_Code, Device_Type, Login_Time, Shift_Status)
                        VALUES 
                            ('{EscapeSql(userNo)}', '{EscapeSql(employeeName)}', '{EscapeSql(roleName)}', '{EscapeSql(branchCode)}', '{EscapeSql(deviceType)}', GETDATE(), 'ACTIVE');
                        SELECT SCOPE_IDENTITY() AS Session_Id;
                    ";

                    DataTable sessionDt = SQLService.GetDataTable(sessionQuery);
                    if (sessionDt != null && sessionDt.Rows.Count > 0 && sessionDt.Rows[0]["Session_Id"] != DBNull.Value)
                    {
                        sessionId = Convert.ToInt64(sessionDt.Rows[0]["Session_Id"]);
                    }
                }
                catch
                {
                    sessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }


                return Ok(new
                {
                    status = true,
                    message = "Login Success",
                    user = new
                    {
                        UserNo = userNo,
                        UserName = username,
                        EmployeeName = employeeName,
                        RoleName = roleName,
                        BranchCode = branchCode,
                        SessionId = sessionId,
                        LoginTime = loginTimeFormatted,
                        Permissions = new
                        {
                            LandingRoute = landingRoute,
                            AllowedRoutes = allowedRoutes,
                            CanAccessMasters = canAccessMasters,
                            CanAccessReports = canAccessReports,
                            CanAccessSettings = canAccessSettings
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================================
        // 2. POST: /api/Logout (Close Shift Session & Record Logout Time)
        // =========================================================================
        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout([FromBody] LogoutModel model)
        {
            try
            {
                if (model == null || model.SessionId <= 0)
                {
                    return Ok(new { status = true, message = "Logged out successfully" });
                }

                string logoutTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string updateQuery = $@"
                    UPDATE dbo.User_Shift_Session_Log
                    SET Logout_Time = GETDATE(),
                        Shift_Status = 'CLOSED',
                        Total_Minutes = DATEDIFF(MINUTE, Login_Time, GETDATE())
                    WHERE Session_Id = {model.SessionId};
                ";

                SQLService.ExecuteNonQuery(updateQuery);

                return Ok(new
                {
                    status = true,
                    message = "Shift Closed and Logged Out Successfully.",
                    logoutTime = logoutTime
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================================
        // 3. GET: /api/ShiftSummary (Real-Time Live Shift Statistics from Tender_Master)
        // =========================================================================
        [HttpGet]
        [Route("ShiftSummary")]
        public IActionResult GetShiftSummary([FromQuery] long sessionId, [FromQuery] string userCode, [FromQuery] string fromTime)
        {
            try
            {
                string safeUserCode = EscapeSql(userCode ?? "");
                DateTime shiftStart;

                if (!DateTime.TryParse(fromTime, out shiftStart))
                {
                    shiftStart = DateTime.Today; // default today 00:00
                }

                string fromTimeStr = shiftStart.ToString("yyyy-MM-dd HH:mm:ss");

                // 1. KOT Activity (Specific to the logged-in Captain / Waiter)
                string kotQuery = $@"
                    SELECT 
                        COUNT(KM.KOT_Id) AS Total_KOTs,
                        ISNULL(SUM(KM.Total_Qty), 0) AS Total_Items_Qty,
                        ISNULL(SUM(KM.Total_Amount), 0) AS Total_KOT_Amount
                    FROM dbo.M_KOT_Main KM
                    WHERE KM.Created_On >= '{fromTimeStr}'
                      AND (
                          KM.Waiter_Code = '{safeUserCode}' 
                          OR KM.Created_By = '{safeUserCode}'
                          OR '{safeUserCode}' = ''
                          OR '{safeUserCode}' = 'EMP00001'
                      )
                      AND KM.Is_Active = 'A';
                ";

                DataTable kotDt = SQLService.GetDataTable(kotQuery);

                // 2. Total Bills & Net Amount
                string billQuery = $@"
                    SELECT 
                        COUNT(B.Bill_Id) AS Total_Bills,
                        ISNULL(SUM(B.Net_Amount), 0) AS Total_Billed_Amount,
                        ISNULL(SUM(CASE WHEN UPPER(TRIM(B.Payment_Mode)) = 'CASH' THEN B.Net_Amount ELSE 0 END), 0) AS Cash_Amount,
                        ISNULL(SUM(CASE WHEN UPPER(TRIM(B.Payment_Mode)) = 'UPI' THEN B.Net_Amount ELSE 0 END), 0) AS UPI_Amount,
                        ISNULL(SUM(CASE WHEN UPPER(TRIM(B.Payment_Mode)) = 'SPLIT' THEN B.Net_Amount ELSE 0 END), 0) AS Split_Amount,
                        ISNULL(SUM(CASE WHEN UPPER(TRIM(B.Payment_Mode)) IN ('CARD','CREDIT CARD','DEBIT CARD') THEN B.Net_Amount ELSE 0 END), 0) AS Card_Amount
                    FROM dbo.Bill_Main B
                    WHERE B.Created_On >= '{fromTimeStr}'
                      AND B.Is_Active = 'A'
                      AND B.Bill_Status = 'BILLED';
                ";
                DataTable billDt = SQLService.GetDataTable(billQuery);

                // 3. Dynamic Tender Breakdown from Tender_Master
                string tenderQuery = $@"
                    SELECT 
                        TM.Tender_Code,
                        TM.Tender_Name,
                        ISNULL(SUM(B.Net_Amount), 0) AS Total_Amount,
                        COUNT(B.Bill_Id) AS Bills_Count
                    FROM dbo.Tender_Master TM
                    LEFT JOIN dbo.Bill_Main B 
                        ON UPPER(TRIM(B.Payment_Mode)) = UPPER(TRIM(TM.Tender_Name))
                       AND B.Created_On >= '{fromTimeStr}'
                       AND B.Is_Active = 'A'
                       AND B.Bill_Status = 'BILLED'
                    WHERE TM.Is_Active = 'A'
                    GROUP BY TM.Tender_Code, TM.Tender_Name, TM.Display_Order
                    ORDER BY ISNULL(TM.Display_Order, 999), TM.Tender_Name;
                ";
                DataTable tenderDt = SQLService.GetDataTable(tenderQuery);

                var tenderList = new List<object>();

                if (tenderDt != null && tenderDt.Rows.Count > 0)
                {
                    foreach (DataRow row in tenderDt.Rows)
                    {
                        tenderList.Add(new
                        {
                            Tender_Code = row["Tender_Code"].ToString(),
                            Tender_Name = row["Tender_Name"].ToString(),
                            Total_Amount = row["Total_Amount"] != DBNull.Value ? Convert.ToDecimal(row["Total_Amount"]) : 0,
                            Bills_Count = row["Bills_Count"] != DBNull.Value ? Convert.ToInt32(row["Bills_Count"]) : 0
                        });
                    }
                }

                // Check for SPLIT bills if any
                string splitQuery = $@"
                    SELECT ISNULL(SUM(Net_Amount), 0) AS Split_Total, COUNT(Bill_Id) AS Split_Count
                    FROM dbo.Bill_Main
                    WHERE UPPER(TRIM(Payment_Mode)) = 'SPLIT'
                      AND Created_On >= '{fromTimeStr}'
                      AND Is_Active = 'A'
                      AND Bill_Status = 'BILLED';
                ";
                DataTable splitDt = SQLService.GetDataTable(splitQuery);
                if (splitDt != null && splitDt.Rows.Count > 0 && Convert.ToDecimal(splitDt.Rows[0]["Split_Total"]) > 0)
                {
                    tenderList.Add(new
                    {
                        Tender_Code = "SPLIT",
                        Tender_Name = "SPLIT TENDER",
                        Total_Amount = Convert.ToDecimal(splitDt.Rows[0]["Split_Total"]),
                        Bills_Count = Convert.ToInt32(splitDt.Rows[0]["Split_Count"])
                    });
                }

                int totalKots = (kotDt != null && kotDt.Rows.Count > 0) ? Convert.ToInt32(kotDt.Rows[0]["Total_KOTs"]) : 0;
                int totalItems = (kotDt != null && kotDt.Rows.Count > 0) ? Convert.ToInt32(kotDt.Rows[0]["Total_Items_Qty"]) : 0;
                decimal totalKotAmount = (kotDt != null && kotDt.Rows.Count > 0) ? Convert.ToDecimal(kotDt.Rows[0]["Total_KOT_Amount"]) : 0;

                int totalBills = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToInt32(billDt.Rows[0]["Total_Bills"]) : 0;
                decimal cashAmount = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToDecimal(billDt.Rows[0]["Cash_Amount"]) : 0;
                decimal upiAmount = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToDecimal(billDt.Rows[0]["UPI_Amount"]) : 0;
                decimal splitAmount = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToDecimal(billDt.Rows[0]["Split_Amount"]) : 0;
                decimal cardAmount = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToDecimal(billDt.Rows[0]["Card_Amount"]) : 0;
                decimal totalBilled = (billDt != null && billDt.Rows.Count > 0) ? Convert.ToDecimal(billDt.Rows[0]["Total_Billed_Amount"]) : 0;

                int durationMinutes = (int)(DateTime.Now - shiftStart).TotalMinutes;
                if (durationMinutes < 0) durationMinutes = 0;

                return Ok(new
                {
                    status = true,
                    shiftStats = new
                    {
                        UserCode = userCode,
                        LoginTime = fromTimeStr,
                        CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        DurationMinutes = durationMinutes,
                        DurationText = $"{durationMinutes / 60}h {durationMinutes % 60}m",
                        TotalKots = totalKots,
                        TotalItemsQty = totalItems,
                        TotalKotAmount = totalKotAmount,
                        TotalBills = totalBills,
                        CashAmount = cashAmount,
                        UpiAmount = upiAmount,
                        SplitAmount = splitAmount,
                        CardAmount = cardAmount,
                        TotalBilledAmount = totalBilled,
                        TenderBreakdown = tenderList
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
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
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DeviceType { get; set; } = "DESKTOP_POS";
    }

    public class LogoutModel
    {
        public long SessionId { get; set; }
        public string? UserCode { get; set; }
    }
}
