using CommonModels;
using CommonServices;
using laptop_service.Models;
using System.Data;
using System.Text;

namespace laptop_service.Services
{
    public class SignupService
    {
        public static string GenerateActivationURLValidityToken(string Email)
        {
            // with in 15 days, user must activate the account after registration.
            string NotBefore = DateTime.Now.ToString("dd-MM-yyyy");
            string ExpireAt = DateTime.Now.AddDays(5).ToString("dd-MM-yyyy");

            string token = Email + "," + NotBefore + "," + ExpireAt;

            return CipherService.Encrypt(token);
        }

        public static string CheckActivationURLValidityToken(string Validity_Token)
        {
            Validity_Token = CipherService.Decrypt(Validity_Token);
            if (!string.IsNullOrWhiteSpace(Validity_Token))
            {
                string[] arr = Validity_Token.Split(',');

                if (arr.Length > 2)
                {
                    // with in 15 days, user must activate the account after registration.
                    string Email = arr[0].ToString();
                    string NotBefore = arr[1].ToString();
                    string ExpireAt = arr[2].ToString();

                    if (UtilityService.ExpiryDateValidate(NotBefore, ExpireAt))
                    {
                        return Email;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static SqlResponse SaveRegistrationMaster(string conStr, Signup signup)
        {
            StringBuilder sb = new StringBuilder();

            // Remove some special characters from their Organization_Name & Employee_Name
            // Because while activating account, calling store procedure failed
            if (!string.IsNullOrWhiteSpace(signup.Organization_Name))
            {
                signup.Organization_Name = signup.Organization_Name.Replace("\'", "");
                signup.Organization_Name = signup.Organization_Name.Replace(",", "");
                signup.Organization_Name = signup.Organization_Name.Replace("\\", "");
                signup.Organization_Name = signup.Organization_Name.Replace("/", "");
                signup.Organization_Name = signup.Organization_Name.Replace("`", "");
            }
            if (!string.IsNullOrWhiteSpace(signup.Employee_Name))
            {
                signup.Employee_Name = signup.Employee_Name.Replace("\'", "");
                signup.Employee_Name = signup.Employee_Name.Replace(",", "");
                signup.Employee_Name = signup.Employee_Name.Replace("\\", "");
                signup.Employee_Name = signup.Employee_Name.Replace("/", "");
                signup.Employee_Name = signup.Employee_Name.Replace("`", "");
            }

            sb.Append("CALL `pr_insert_Registration_Master`(");
            sb.Append(SqlService.AddStringSQLParameter(signup.Organization_Name, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Employee_Name, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Phone, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Email, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.City, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Country, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Sale_Channel, true, null));
            sb.Append(SqlService.AddStringSQLParameter(signup.Sale_Person, false, null));
            sb.Append(");");

            DataTable dataTable = SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
            if (dataTable.Rows.Count > 0)
            {
                string responseMessage = "";
                if (UtilityService.CheckInsertUpdateDeleteResponse(dataTable, out responseMessage))
                {
                    return new SqlResponse { status = true, message = responseMessage };
                }
                else
                {
                    return new SqlResponse { status = false, message = responseMessage };
                }
            }
            else
            {
                return new SqlResponse { status = false, message = "Empty response received." };
            }
        }

        public static DataTable ActivateRegistrationMaster(string conStr, string Email)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CALL `pr_activate_Registration_Master`(");
            sb.Append(SqlService.AddStringSQLParameter(Email, false, null));
            sb.Append(");");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

    }
}
