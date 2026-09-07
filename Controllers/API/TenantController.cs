using AppSettings;
using CommonModels;
using CommonServices;
using laptop_service.Models;
using laptop_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;

namespace laptop_service.Controllers.API
{
    [Authorize]
    [Route("api/tenants")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        #region Tenant Database Connection Usage

        [HttpGet]
        [Route("sync-database-server-master")]
        public IActionResult SyncDatabaseServerMaster()
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            AppSetting.TenantDatabaseServerMaster = TenantService.GetTenantDatabaseServerMaster(ConStr);
            if (AppSetting.TenantDatabaseServerMaster.Rows.Count > 0)
            {
                // Form the API return object
                JArray jArray = UtilityService.DataTableToJArray(AppSetting.TenantDatabaseServerMaster);

                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.data = jArray;
                apiResponse.message = "Success : Reading Tenant_Database_Server_Master List Done.";
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.data = null;
                apiResponse.message = "Fail : Reading Tenant_Database_Server_Master List Failed.";
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        #endregion Tenant Database Connection Usage


        [HttpGet]
        [Route("list/{From_Date}/{To_Date}")]
        public IActionResult List(string From_Date, string To_Date)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            DataTable dataTable = TenantService.List(ConStr, From_Date, To_Date);

            // Form the API return object
            JArray jArray = UtilityService.DataTableToJArray(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [HttpGet]
        [Route("search/{Search_Data}")]
        public IActionResult SearchList(string Search_Data)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            DataTable dataTable = TenantService.SearchList(ConStr, Search_Data);

            // Form the API return object
            JArray jArray = UtilityService.DataTableToJArray(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [HttpGet]
        [Route("{Email}")]
        public IActionResult Get(string Email)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            DataTable dataTable = TenantService.Read(ConStr, Email);

            // Form the API return object
            JObject jObject = UtilityService.DataTableToJObject(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jObject;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }



        [SuppressModelStateInvalidFilter]
        [HttpPut]
        public IActionResult UpdateMasterDetail([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Organization_Name) || string.IsNullOrWhiteSpace(tenant.Employee_Name)
                || string.IsNullOrWhiteSpace(tenant.Phone) || string.IsNullOrWhiteSpace(tenant.Email)
                || string.IsNullOrWhiteSpace(tenant.City) || string.IsNullOrWhiteSpace(tenant.Country))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Master Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateMasterDetail(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("accountid")]
        public IActionResult UpdateAccountId([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Account_Id) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Account Id Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateAccountId(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("subdomainurl")]
        public IActionResult UpdateSubDomainURL([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Sub_Domain_URL) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Sub Domain URL Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateSubDomainURL(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("subscriptionplan")]
        public IActionResult UpdateSubscriptionPlan([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Subscription_Plan) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Subscription Plan Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateSubscriptionPlan(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("subscriptionexpiry")]
        public IActionResult UpdateSubscriptionExpiry([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Subscription_Expiry) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Subscription Expiry Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateSubscriptionExpiry(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("subscriptionnotes")]
        public IActionResult UpdateSubscriptionNotes([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Subscription_Notes) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Subscription Notes Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateSubscriptionNotes(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("isactive")]
        public IActionResult UpdateIsActive([FromBody] Tenant tenant)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Data Validataion
            if (!ModelState.IsValid)
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                //apiResponse.data = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
                foreach (KeyValuePair<string, string[]> keyValuePairs in ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()))
                {
                    if (keyValuePairs.Value.Length > 0)
                    {
                        //apiResponse.data += "[" + keyValuePairs.Key + ":" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                        apiResponse.data += "[" + keyValuePairs.Value[0]?.ToString() + "]<br/>";
                    }
                }
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Additional Data Validataion
            if (string.IsNullOrWhiteSpace(tenant.Is_Active) || string.IsNullOrWhiteSpace(tenant.Email))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Is Active Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = TenantService.UpdateIsActive(ConStr, tenant);

            // Form the API return object
            if (sqlResponse.status)
            {
                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = sqlResponse.message;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Update Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

    }
}
