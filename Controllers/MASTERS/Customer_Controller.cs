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
    [Route("api/customer")]
    [ApiController]
    public class Customer_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddCustomer")]
        public async Task<IActionResult> AddCustomer(CUSTOMER_MASTER customer)
        {
            try
            {
                JObject jobject = new JObject();

                customer.Customer_Name = customer.Customer_Name?.Trim();
                customer.Phone_No = customer.Phone_No?.Trim();

                if (string.IsNullOrWhiteSpace(customer.Customer_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Customer Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(customer.Phone_No))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Phone Number cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
SELECT COUNT(*)
FROM Customer_Master
WHERE Phone_No = '{customer.Phone_No}'
AND Is_Active = 'A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Phone Number already exists.");
                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
DELETE FROM Customer_Master
WHERE Phone_No = '{customer.Phone_No}'
AND Is_Active = 'D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                customer = await LoadCustomerMaxCode(customer);

                var data = new CUSTOMER_MASTER
                {
                    Customer_Code = customer.Customer_Code,
                    Customer_Name = customer.Customer_Name.ToUpper(),
                    Phone_No = customer.Phone_No,
                    Is_Active = customer.Is_Active ?? "A",
                    Created_By = customer.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>
                {
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "Customer_Master",
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Customer Saved Successfully");
                    jobject.Add("customer_code", customer.Customer_Code);
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save Customer");
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
        [Route("UpdateCustomer/{Customer_Code}")]
        public IActionResult UpdateCustomer(
            string Customer_Code,
            CUSTOMER_MASTER customer)
        {
            try
            {
                JObject jobject = new JObject();

                customer.Customer_Name = customer.Customer_Name?.Trim();
                customer.Phone_No = customer.Phone_No?.Trim();

                if (string.IsNullOrWhiteSpace(customer.Customer_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Customer Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(customer.Phone_No))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Phone Number cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string recordCheckQuery = $@"
                SELECT COUNT(*)
                FROM Customer_Master
                WHERE Customer_Code = '{Customer_Code}'
                AND Is_Active = 'A'";

                int recordCount =
                    SQLService.ExecuteScalarQuery(recordCheckQuery);

                if (recordCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Customer record not found.");
                    return Ok(jobject.ToString());
                }

                string duplicateCheckQuery = $@"
SELECT COUNT(*)
FROM Customer_Master
WHERE Phone_No = '{customer.Phone_No}'
AND Customer_Code <> '{Customer_Code}'
AND Is_Active = 'A'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateCheckQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Phone Number already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new CUSTOMER_MASTER
                {
                    Customer_Code = Customer_Code,
                    Customer_Name = customer.Customer_Name.ToUpper(),
                    Phone_No = customer.Phone_No,
                    Is_Active = customer.Is_Active ?? "A",
                    Updated_By = customer.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Customer_Master",
                    "Customer_Code",
                    Customer_Code,
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Customer Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update Customer");
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
        [Route("DeleteCustomer/{Customer_Code}")]
        public IActionResult DeleteCustomer(string Customer_Code)
        {
            try
            {
                JObject jobject = new JObject();

                string query = $@"
                UPDATE Customer_Master
                SET
                    Is_Active = 'D',
                    Updated_On = GETDATE()
                WHERE Customer_Code = '{Customer_Code}'
                AND Is_Active = 'A'";

                int result = SQLService.ExecuteNonQuery(query);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Customer Deleted Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Customer not found or already deleted."
                    );
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
        [Route("CustomerList")]
        public IActionResult CustomerList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt =
                    SQLService.GetDataTable("EXEC SP_CustomerList");

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetCustomerByCode/{Customer_Code}")]
        public IActionResult GetCustomerByCode(string Customer_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT *
                FROM Customer_Master
                WHERE Customer_Code = '{Customer_Code}'
                AND Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Customer not found."
                    });
                }

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetCustomerByPhone/{Phone_No}")]
        public IActionResult GetCustomerByPhone(string Phone_No)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT *
                FROM Customer_Master
                WHERE Phone_No = '{Phone_No}'
                AND Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Customer not found."
                    });
                }

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        private async Task<CUSTOMER_MASTER> LoadCustomerMaxCode(
            CUSTOMER_MASTER customer)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
                ISNULL(
                    MAX(
                        TRY_CAST(
                            REPLACE(Customer_Code, 'C', '') AS INT
                        )
                    ),
                    0
                ) + 1
            FROM Customer_Master
            WHERE Customer_Code LIKE 'C%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);

                customer.Customer_Code =
                    "C" + code.ToString().PadLeft(5, '0');
            }

            return await Task.FromResult(customer);
        }
    }
}