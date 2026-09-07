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
    [Route("api/signup")]
    [ApiController]
    public class SignupController : ControllerBase
    {
        [AllowAnonymous]
        [SuppressModelStateInvalidFilter]
        [HttpPost]
        public IActionResult Signup([FromBody] Signup signup)
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

            if (string.IsNullOrWhiteSpace(signup.Organization_Name))
            {
                signup.Organization_Name = "Dear Company";
            }

            // Connect with master database
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

            // Fetch the data from SQL database
            SqlResponse sqlResponse = SignupService.SaveRegistrationMaster(ConStr, signup);

            // Form the API return object
            if (sqlResponse.status)
            {
                string email_seding_status = "";
                StringBuilder emailBody = new StringBuilder();
                List<string> toEmailId = new List<string>();

                #region Send Signup Account Registration Email to Registered User

                if (toEmailId.Count > 0)
                {
                    toEmailId.Clear();
                }
                toEmailId.Add(signup.Email);

                emailBody.Append(FileService.EmailTemplate(EmailTemplates.AccountRegistration));
                if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                {
                    emailBody.Replace("@Employee_Name@", signup.Employee_Name);

                    emailBody = emailBody.Replace("@Organization_Name@", signup.Organization_Name);

                    email_seding_status = EmailService.SendEmail(toEmailId, EmailSubjects.AccountRegistration, emailBody.ToString());
                }

                #endregion Send Signup Account Registration Email to Registered User


                #region Send Signup Notification Email to App Admin

                if (!string.IsNullOrWhiteSpace(AppSetting.SignupNotificationMailID))
                {
                    string[] toMailId = AppSetting.SignupNotificationMailID.Split(',');

                    if (toMailId.Length > 0)
                    {
                        if (toEmailId.Count > 0)
                        {
                            toEmailId.Clear();
                        }

                        foreach (string str in toMailId)
                        {
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                toEmailId.Add(str);
                            }
                        }

                        emailBody.Clear();
                        emailBody.Append(FileService.EmailTemplate(EmailTemplates.NewRegistration));

                        if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                        {
                            emailBody.Replace("@Organization_Name@", signup.Organization_Name);
                            emailBody.Replace("@Employee_Name@", signup.Employee_Name);
                            emailBody.Replace("@Phone@", signup.Phone);
                            emailBody.Replace("@Email@", signup.Email);
                            emailBody.Replace("@City@", signup.City);
                            emailBody.Replace("@Country@", signup.Country);
                            emailBody.Replace("@email_seding_status@", email_seding_status);

                            EmailService.SendEmail(toEmailId, EmailSubjects.NewRegistration, emailBody.ToString());
                        }
                    }
                }

                #endregion Send Signup Notification Email to App Admin


                // Add the return data in response object
                apiResponse.status = true;
                //apiResponse.message = sqlResponse.message;

                if (email_seding_status == "Email Sent")
                {
                    email_seding_status = "Yes";
                    apiResponse.message = sqlResponse.message + "Our team will verify your details, then you will receive the activation link by email.";
                }
                else
                {
                    email_seding_status = "No";
                    apiResponse.message = sqlResponse.message + "Activation link sending failed, contact our support team.";
                }
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Cannot Save Record.";
                apiResponse.error.code = "1002";
                apiResponse.error.message = sqlResponse.message;
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }


        [HttpPost]
        [Route("activation")]
        public IActionResult Activation()
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Get the registered email from the AuthToken
            string RegisteredEmail = JwtService.GetClaimIdentityNameFromJSONWebToken(HttpContext);
            if (!string.IsNullOrWhiteSpace(RegisteredEmail))
            {
                // Connect with master database
                string ConStr = SqlService.GetMasterDatabaseConnectionStirng();

                DataTable dataTable = SignupService.ActivateRegistrationMaster(ConStr, RegisteredEmail);
                if (dataTable.Rows.Count > 0)
                {
                    string responseMessage = "";
                    if (UtilityService.CheckInsertUpdateDeleteResponse(dataTable, out responseMessage))
                    {
                        string email_seding_status = "";
                        StringBuilder emailBody = new StringBuilder();
                        List<string> toEmailId = new List<string>();

                        JObject jObject = new JObject();
                        jObject.Add("1.Registration_Status", "Activated");

                        Tenant tenant = new Tenant();
                        TenantMaster tenantMaster = new TenantMaster();

                        tenant.Account_Id = dataTable.Rows[0]["Account_Id"]?.ToString();
                        tenant.Organization_Code = dataTable.Rows[0]["Organization_Code"]?.ToString();
                        tenant.Database_Server = dataTable.Rows[0]["Database_Server"]?.ToString();
                        tenant.Database_Name = dataTable.Rows[0]["Database_Name"]?.ToString();
                        tenant.Email = dataTable.Rows[0]["Email"]?.ToString();

                        if (DatabaseService.GetDatabaseAvailability(tenant.Database_Server, tenant.Database_Name) == false)
                        {
                            jObject.Add("2.Database_Check", "Success");

                            DataTable dT = TenantService.Read(ConStr, RegisteredEmail);
                            if (dT.Rows.Count > 0)
                            {
                                jObject.Add("3.Tenant_Data_Read", "Success");

                                tenantMaster.Account_Id = dT.Rows[0]["Account_Id"].ToString();
                                tenantMaster.Organization_Code = dT.Rows[0]["Organization_Code"].ToString();
                                tenantMaster.Sub_Domain_URL = dT.Rows[0]["Sub_Domain_URL"].ToString();
                                tenantMaster.Database_Server = dT.Rows[0]["Database_Server"].ToString();
                                tenantMaster.Database_Name = dT.Rows[0]["Database_Name"].ToString();
                                tenantMaster.Organization_Name = dT.Rows[0]["Organization_Name"].ToString();
                                tenantMaster.Employee_Name = dT.Rows[0]["Employee_Name"].ToString();
                                tenantMaster.Phone = dT.Rows[0]["Phone"].ToString();
                                tenantMaster.Email = dT.Rows[0]["Email"].ToString();
                                tenantMaster.City = dT.Rows[0]["City"].ToString();
                                tenantMaster.Country = dT.Rows[0]["Country"].ToString();

                                string masterPassword = UtilityService.GenerateRandomPassword();

                                string ret = DatabaseService.CreateDatabase(tenantMaster, masterPassword);
                                jObject.Add("4.Database_Message", ret);
                                if (ret.Contains("Fail"))
                                {
                                    jObject.Add("4.Database_Create", "Failed");

                                    // Add the return data in response object
                                    apiResponse.status = false;
                                    apiResponse.message = "Cannot Create Database.";
                                    apiResponse.data = jObject;
                                }
                                else
                                {
                                    jObject.Add("4.Database_Create", "Success");

                                    #region Send Account Activation Confirmation Email to Registered User

                                    toEmailId.Add(tenantMaster.Email);

                                    emailBody.Append(FileService.EmailTemplate(EmailTemplates.AccountConfirmation));
                                    if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                                    {
                                        emailBody = emailBody.Replace("@Employee_Name@", tenantMaster.Employee_Name);
                                        emailBody = emailBody.Replace("@Account_Id@", tenantMaster.Account_Id);
                                        emailBody = emailBody.Replace("@Organization_Code@", tenantMaster.Organization_Code);
                                        emailBody = emailBody.Replace("@Email@", tenantMaster.Email);
                                        emailBody = emailBody.Replace("@Password@", masterPassword);

                                        email_seding_status = EmailService.SendEmail(toEmailId, EmailSubjects.AccountConfirmation + " " + tenantMaster.Employee_Name, emailBody.ToString());

                                        // send copy of email to admin
                                        if (!string.IsNullOrWhiteSpace(AppSetting.AdminMailID))
                                        {
                                            if (toEmailId.Count > 0)
                                            {
                                                toEmailId.Clear();
                                            }

                                            toEmailId.Add(AppSetting.AdminMailID); // send copy of email to admin
                                            EmailService.SendEmail(toEmailId, EmailSubjects.AccountConfirmation + " " + tenantMaster.Employee_Name, emailBody.ToString());
                                        }
                                    }

                                    #endregion Send Account Activation Confirmation Email to Registered User

                                    #region Send Account Activation Notification Email to ALLPOS Admin

                                    if (!string.IsNullOrWhiteSpace(AppSetting.SignupNotificationMailID))
                                    {
                                        string[] toMailId = AppSetting.SignupNotificationMailID.Split(',');

                                        if (toMailId.Length > 0)
                                        {
                                            if (toEmailId.Count > 0)
                                            {
                                                toEmailId.Clear();
                                            }

                                            foreach (string str in toMailId)
                                            {
                                                if (!string.IsNullOrWhiteSpace(str))
                                                {
                                                    toEmailId.Add(str);
                                                }
                                            }

                                            emailBody.Clear();
                                            emailBody.Append(FileService.EmailTemplate(EmailTemplates.NewActivation));

                                            if (!string.IsNullOrWhiteSpace(emailBody.ToString()))
                                            {
                                                emailBody.Replace("@Organization_Code@", tenantMaster.Organization_Code);
                                                emailBody.Replace("@Organization_Name@", tenantMaster.Organization_Name);
                                                emailBody.Replace("@Account_Id@", tenantMaster.Account_Id);
                                                emailBody.Replace("@Employee_Name@", tenantMaster.Employee_Name);
                                                emailBody.Replace("@Phone@", tenantMaster.Phone);
                                                emailBody.Replace("@Email@", tenantMaster.Email);
                                                emailBody.Replace("@City@", tenantMaster.City);
                                                emailBody.Replace("@Country@", tenantMaster.Country);
                                                emailBody.Replace("@email_seding_status@", email_seding_status);

                                                EmailService.SendEmail(toEmailId, EmailSubjects.NewActivation, emailBody.ToString());
                                            }
                                        }
                                    }

                                    #endregion Send Account Activation Notification Email to ALLPOS Admin

                                    if (email_seding_status == "Email Sent")
                                    {
                                        email_seding_status = "Yes";
                                        jObject.Add("5.Activation_Email_Sent", "Success");
                                    }
                                    else
                                    {
                                        email_seding_status = "No";
                                        jObject.Add("5.Activation_Email_Sent", "Failed");
                                    }

                                    // Add the return data in response object
                                    apiResponse.status = true;
                                    apiResponse.message = responseMessage;
                                    apiResponse.data = jObject;
                                }
                            }
                            else
                            {
                                jObject.Add("3.Tenant_Data_Read", "Failed");

                                // Add the return data in response object
                                apiResponse.status = false;
                                apiResponse.message = "Tenant Data Not Found.";
                                apiResponse.data = jObject;
                            }
                        }
                        else
                        {
                            jObject.Add("2.Database_Check", "Failed");

                            // Add the return data in response object
                            apiResponse.status = false;
                            apiResponse.message = "Database Already Exists.";
                            apiResponse.data = jObject;
                        }
                    }
                    else
                    {
                        // Add the return data in response object
                        apiResponse.status = false;
                        apiResponse.message = "Cannot Save Record.";
                        apiResponse.error.code = "1003";
                        apiResponse.error.message = responseMessage;
                    }
                }
                else
                {
                    // Add the return data in response object
                    apiResponse.status = false;
                    apiResponse.message = "Cannot Save Record.";
                    apiResponse.error.code = "1002";
                    apiResponse.error.message = "Empty response received.";
                }
            }
            else
            {
                // Add the return data in response object
                apiResponse.status = false;
                apiResponse.message = "Invalid Access.";
                apiResponse.error.code = "1001";
                apiResponse.error.message = "Validity_Token Invalid.";
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }
    }
}
