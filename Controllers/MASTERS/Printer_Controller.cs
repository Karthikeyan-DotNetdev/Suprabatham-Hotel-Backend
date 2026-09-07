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
    [Route("api/printer")]
    [ApiController]
    public class Printer_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddPrinter")]
        public async Task<IActionResult> AddPrinter(PRINTER_MASTER printer)
        {
            try
            {
                JObject jobject = new JObject();

                printer.Printer_Name = printer.Printer_Name?.Trim();

                if (string.IsNullOrWhiteSpace(printer.Printer_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Printer Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Printer_Master
                WHERE UPPER(Printer_Name)=UPPER('{printer.Printer_Name}')
                AND UPPER(Printer_Interface)=UPPER('{printer.Printer_Interface}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Printer with this Name and Interface already exists.");
                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
                DELETE FROM Printer_Master
                WHERE UPPER(Printer_Name)=UPPER('{printer.Printer_Name}')
                AND UPPER(Printer_Interface)=UPPER('{printer.Printer_Interface}')
                AND Is_Active='D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                printer = await LoadPrinterMaxCode(printer);

                var data = new PRINTER_MASTER
                {
                    Printer_Code = printer.Printer_Code,
                    Printer_Name = printer.Printer_Name.ToUpper(),
                    Printer_Interface = printer.Printer_Interface,
                    IP_Address = printer.IP_Address,
                    Port_Number = printer.Port_Number,
                    Bluetooth_Name = printer.Bluetooth_Name,
                    Bluetooth_Address = printer.Bluetooth_Address,
                    Paper_Size = printer.Paper_Size ?? "80MM",
                    Is_Active = printer.Is_Active ?? "A",
                    Created_By = printer.Created_By,
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
                    "Printer_Master",
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
        [Route("UpdatePrinter/{Printer_Code}")]
        public IActionResult UpdatePrinter(string Printer_Code, PRINTER_MASTER printer)
        {
            try
            {
                JObject jobject = new JObject();

                printer.Printer_Name = printer.Printer_Name?.Trim();

                if (string.IsNullOrWhiteSpace(printer.Printer_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Printer Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Printer_Master
                WHERE UPPER(Printer_Name)=UPPER('{printer.Printer_Name}')
                AND UPPER(Printer_Interface)=UPPER('{printer.Printer_Interface}')
                AND Printer_Code<>'{Printer_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Printer with this Name and Interface already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new PRINTER_MASTER
                {
                    Printer_Code = Printer_Code,
                    Printer_Name = printer.Printer_Name.ToUpper(),
                    Printer_Interface = printer.Printer_Interface,
                    IP_Address = printer.IP_Address,
                    Port_Number = printer.Port_Number,
                    Bluetooth_Name = printer.Bluetooth_Name,
                    Bluetooth_Address = printer.Bluetooth_Address,
                    Paper_Size = printer.Paper_Size,
                    Is_Active = printer.Is_Active,
                    Updated_By = printer.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Printer_Master",
                    "Printer_Code",
                    Printer_Code,
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
        [Route("DeletePrinter/{Printer_Code}")]
        public IActionResult DeletePrinter(string Printer_Code)
        {
            JObject jobject = new JObject();

            // Soft delete - set status to 'D' (Deactive)
            string query = $@"
            UPDATE Printer_Master
            SET Is_Active='D'
            WHERE Printer_Code='{Printer_Code}'";

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
        [Route("PrinterList")]
        public IActionResult PrinterList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_PrinterList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<PRINTER_MASTER> LoadPrinterMaxCode(PRINTER_MASTER printer)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Printer_Code,'P','') AS INT)),0)+1
            FROM Printer_Master
            WHERE Printer_Code LIKE 'P%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                printer.Printer_Code = "P" + code.ToString().PadLeft(5, '0');
            }

            return printer;
        }
    }
}
