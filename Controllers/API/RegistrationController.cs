using AppSettings;
using CommonModels;
using CommonServices;
using laptop_service.Models;
using laptop_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace laptop_service.Controllers.API
{
    [Authorize]
    [Route("api/registrations")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpGet]
        [Route("list/{Status?}")]
        public IActionResult List(string? Status)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            DataTable dataTable = RegistrationService.List(ConStr, Status);

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
            DataTable dataTable = RegistrationService.SearchList(ConStr, Search_Data);

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
            DataTable dataTable = RegistrationService.Read(ConStr, Email);

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
        [Route("status")]
        public IActionResult UpdateStatus([FromBody] Registration registration)
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
            if (string.IsNullOrWhiteSpace(registration.Email) || string.IsNullOrWhiteSpace(registration.Status))
            {
                apiResponse.status = false;
                apiResponse.status_code = "400";
                apiResponse.data = "Email and Status Data Required.";
                apiResponse.message = "Parameter Missing.";
                apiResponse.error.code = "1001";
                apiResponse.error.code = "Invalid Parameter.";

                // Return Action Result
                return Ok(UtilityService.ModelToJsonString(apiResponse));
                //return BadRequest(UtilityService.ModelToJsonString(apiResponse));
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();
            string UpdatedBy = JwtService.GetEmployeeCodeFromJSONWebToken(HttpContext) + "/" + JwtService.GetEmployeeNameFromJSONWebToken(HttpContext);

            // Fetch the data from SQL database
            DataTable dataTable = RegistrationService.UpdateStatus(ConStr, UpdatedBy, registration);

            // Form the API return object
            if (dataTable.Rows.Count > 0)
            {
                string responseMessage = "";
                if (UtilityService.CheckInsertUpdateDeleteResponse(dataTable, out responseMessage))
                {
                    // Send activation link email to end user
                    #region Send Signup Account Verification Email to Registered User

                    string email_seding_status = "";
                    StringBuilder emailBody = new StringBuilder();
                    List<string> toEmailId = new List<string>();

                    string Organization_Name = "";
                    string Employee_Name = "";
                    string Phone = "";
                    string Email = "";
                    string City = "";
                    string Country = "";

                    try { Organization_Name = dataTable.Rows[0]["Organization_Name"]?.ToString(); } catch { }
                    try { Employee_Name = dataTable.Rows[0]["Employee_Name"]?.ToString(); } catch { }
                    try { Phone = dataTable.Rows[0]["Phone"]?.ToString(); } catch { }
                    try { Email = dataTable.Rows[0]["Email"]?.ToString(); } catch { }
                    try { City = dataTable.Rows[0]["City"]?.ToString(); } catch { }
                    try { Country = dataTable.Rows[0]["Country"]?.ToString(); } catch { }

                    if (registration.Status == "Approved")
                    {
                        string Validity_Token = JwtService.GenerateSignupActivationJSONWebToken(Email);
                        string activation_url = string.Format(AppSetting.AccountActivationUrl + "?Validity_Token={0}", System.Web.HttpUtility.UrlEncode(Validity_Token));

                        if (toEmailId.Count > 0)
                        {
                            toEmailId.Clear();
                        }
                        toEmailId.Add(registration.Email);

                        emailBody.Append(FileService.EmailTemplate(EmailTemplates.AccountVerification));
                        if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                        {
                            emailBody.Replace("@Organization_Name@", Organization_Name);
                            emailBody.Replace("@Employee_Name@", Employee_Name);
                            emailBody.Replace("@Phone@", Phone);
                            emailBody.Replace("@Email@", Email);
                            emailBody.Replace("@City@", City);
                            emailBody.Replace("@Country@", Country);
                            emailBody = emailBody.Replace("@Activation_URL@", activation_url);

                            email_seding_status = EmailService.SendEmail(toEmailId, EmailSubjects.AccountVerification, emailBody.ToString());
                        }
                    }
                    else if (registration.Status == "Rejected")
                    {
                        if (toEmailId.Count > 0)
                        {
                            toEmailId.Clear();
                        }
                        toEmailId.Add(registration.Email);

                        emailBody.Append(FileService.EmailTemplate(EmailTemplates.AccountRejection));
                        if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                        {
                            emailBody.Replace("@Organization_Name@", Organization_Name);
                            emailBody.Replace("@Employee_Name@", Employee_Name);
                            emailBody.Replace("@Phone@", Phone);
                            emailBody.Replace("@Email@", Email);
                            emailBody.Replace("@City@", City);
                            emailBody.Replace("@Country@", Country);

                            email_seding_status = EmailService.SendEmail(toEmailId, EmailSubjects.AccountRejection, emailBody.ToString());
                        }
                    }
                    #endregion Send Signup Account Verification Email to Registered User

                    // Add the return data in response object
                    apiResponse.status = true;
                    apiResponse.message = responseMessage + " - Activation Email : " + email_seding_status;
                }
                else
                {
                    // Add the return data in response object
                    apiResponse.status = false;
                    apiResponse.message = registration.Status + " - Failed.";
                    apiResponse.error.code = "1003";
                    apiResponse.error.message = responseMessage;
                }
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = registration.Status + " - Failed.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = "Invalid Record.";
            }


            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [SuppressModelStateInvalidFilter]
        [HttpPut]
        [Route("remarks")]
        public IActionResult UpdateRemarks([FromBody] Registration registration)
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

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = RegistrationService.UpdateRemarks(ConStr, registration);

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
        [HttpPost]
        [Route("send-activation-link")]
        public IActionResult SendActivationLink(Registration registration)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            DataTable dataTable = RegistrationService.Read(ConStr, registration.Email);

            // Send activation link email to end user
            #region Send Signup Account Verification Email to Registered User

            string email_seding_status = "";
            StringBuilder emailBody = new StringBuilder();
            List<string> toEmailId = new List<string>();

            string Organization_Name = "";
            string Employee_Name = "";
            string Phone = "";
            string Email = "";
            string City = "";
            string Country = "";
            string Status = "";

            try { Organization_Name = dataTable.Rows[0]["Organization_Name"]?.ToString(); } catch { }
            try { Employee_Name = dataTable.Rows[0]["Employee_Name"]?.ToString(); } catch { }
            try { Phone = dataTable.Rows[0]["Phone"]?.ToString(); } catch { }
            try { Email = dataTable.Rows[0]["Email"]?.ToString(); } catch { }
            try { City = dataTable.Rows[0]["City"]?.ToString(); } catch { }
            try { Country = dataTable.Rows[0]["Country"]?.ToString(); } catch { }
            try { Status = dataTable.Rows[0]["Status"]?.ToString(); } catch { }

            if (Status == "Approved")
            {
                string Validity_Token = JwtService.GenerateSignupActivationJSONWebToken(Email);
                string activation_url = string.Format(AppSetting.AccountActivationUrl + "?Validity_Token={0}", System.Web.HttpUtility.UrlEncode(Validity_Token));

                if (toEmailId.Count > 0)
                {
                    toEmailId.Clear();
                }
                toEmailId.Add(Email);

                emailBody.Append(FileService.EmailTemplate(EmailTemplates.AccountVerification));
                if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                {
                    emailBody.Replace("@Organization_Name@", Organization_Name);
                    emailBody.Replace("@Employee_Name@", Employee_Name);
                    emailBody.Replace("@Phone@", Phone);
                    emailBody.Replace("@Email@", Email);
                    emailBody.Replace("@City@", City);
                    emailBody.Replace("@Country@", Country);
                    emailBody = emailBody.Replace("@Activation_URL@", activation_url);

                    email_seding_status = EmailService.SendEmail(toEmailId, EmailSubjects.AccountVerification, emailBody.ToString());

                    // send copy of email to admin
                    if (!string.IsNullOrWhiteSpace(AppSetting.AdminMailID))
                    {
                        if (toEmailId.Count > 0)
                        {
                            toEmailId.Clear();
                        }
                        toEmailId.Add(AppSetting.AdminMailID); // send copy of email to admin

                        EmailService.SendEmail(toEmailId, EmailSubjects.AccountVerification, emailBody.ToString());
                    }
                }

                // Add the return data in response object
                apiResponse.status = true;
                apiResponse.message = "Activation Email : " + email_seding_status;
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Invalid Status.";
            }
            #endregion Send Signup Account Verification Email to Registered User

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

    }
}
