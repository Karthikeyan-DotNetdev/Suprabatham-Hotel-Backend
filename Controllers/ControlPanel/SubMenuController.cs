using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using CommonModels;
using CommonServices;
using  laptop_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using WIN_BOT;

namespace laptop_service.Controllers.ControlPanel
{
    [Authorize]
    [Route("api/submenu")]
    [ApiController]
    public class SubMenuController : ControllerBase
    {
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> SaveUser(SubMenu subMenu)
        {
            JObject jobject = new JObject();

            //if (string.IsNullOrWhiteSpace(subMenu.Display_Order))
            //{
            //    jobject.Add("status", false);
            //    jobject.Add("message", "Display Order cannot be empty or whitespace only.");
            //    return Ok(jobject.ToString());
            //}

            //string checkDisplayOrderQuery = $"SELECT COUNT(*) FROM M_CONTROLPANEL_MODULES WHERE Display_Order = '{subMenu.Display_Order}'";
            //int DisplayOrderCount = SQLService.ExecuteScalarQuery(checkDisplayOrderQuery);
            string checkQuery = "SELECT COUNT(*) FROM M_CONTROLPANEL_MODULES WHERE Module='" + subMenu.Module + "'";
            int count = SQLService.ExecuteScalarQuery(checkQuery);
            if (count > 0)
            {
                TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                jobject.Add("status", false);
                jobject.Add("message", "" + textInfo.ToTitleCase(subMenu.Module?.ToLower() + " Menu Already Exist"));
            }
            //else if (DisplayOrderCount > 0)
            //{
            //    jobject.Add("status", false);
            //    jobject.Add("message", "Display Order already exists.");
            //    return Ok(jobject.ToString());
            //}
            else
            {

                //upload image in aws
                string orgb64String = subMenu?.SubMenuImg?.ToString()?.Replace("data:image/jpeg;base64,", "");
                orgb64String = orgb64String.Replace("data:image/jpg;base64,", "");
                orgb64String = orgb64String.Replace("data:image/png;base64,", "");

                var accessKeyId = AWSFolderName.AccessKeyId;
                var secretAccessKey = AWSFolderName.SecretAccessKey;
                var bucketName = AWSFolderName.BucketName;

                // Specify the AWS region and S3 bucket name
                var region = RegionEndpoint.APSouth1; // Change this to your desired region

                // Create an S3 clientn
                var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyId, secretAccessKey);
                var s3Client = new AmazonS3Client(credentials, region);

                var fileData = Convert.FromBase64String(orgb64String);
                // Specify a unique key for the object in S3 (e.g., filename)
                var folderName = "submenu";
                Random random = new Random();
                // Generate a random integer
                int randomNumber = random.Next(100000, 1000000); // Random number between 100000 and 999999
                var keyName = $"{folderName}/{subMenu.HomeMenu}_{randomNumber}.png"; // Ensure the file has a .pdf extension

                // Upload the base 64 to S3
                await UploadBytesAsync(s3Client, bucketName, keyName, fileData);

                subMenu.HomeMenu = subMenu.HomeMenu?.Trim();
                subMenu.Module = subMenu.Module?.Trim();
                subMenu.groupIndex = subMenu.groupIndex?.Trim();
                subMenu.Menu = subMenu.Menu?.Trim();
                subMenu.Display_Order = subMenu.Display_Order?.Trim();

                subMenu.groupIndex = await LoadGroupIndexMaxCode(subMenu);
                var controlPanel = new SubMenu
                {
                    HomeMenu = subMenu.HomeMenu?.ToUpper(),
                    Module = subMenu.Module?.ToUpper(),
                    groupIndex = subMenu.groupIndex,
                    RouterLink = subMenu.RouterLink,
                    Display_Order = subMenu.Display_Order,
                    SubMenuImg = keyName ?? "",
                    SubMenuIcon= subMenu.SubMenuIcon,
                    Is_Active = subMenu?.Is_Active?.ToString() ?? "0",
                    Menu = subMenu?.Menu?.ToUpper()

                };
                //*** Important *** //
                var ignoredColumns = new List<string> { };
                var InsertQuery = SQLHelper.BuildInsertQuery(controlPanel, "M_CONTROLPANEL_MODULES", ignoredColumns);
                int result = SQLService.ExecuteNonQuery(InsertQuery);
                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Saved Successfully");

                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Saved!");
                }
            }
            return Ok(jobject.ToString());
        }

        [HttpPost]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, SubMenu subMenu)
        {
            var keyName = "";
            if (!string.IsNullOrEmpty(subMenu.SubMenuImg))
            {

                //upload image in aws
                string orgb64String = subMenu?.SubMenuImg?.ToString()?.Replace("data:image/jpeg;base64,", "");
                orgb64String = orgb64String.Replace("data:image/jpg;base64,", "");
                orgb64String = orgb64String.Replace("data:image/png;base64,", "");

                var accessKeyId = AWSFolderName.AccessKeyId;
                var secretAccessKey = AWSFolderName.SecretAccessKey;
                var bucketName = AWSFolderName.BucketName;

                // Specify the AWS region and S3 bucket name
                var region = RegionEndpoint.APSouth1; // Change this to your desired region

                // Create an S3 clientn
                var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyId, secretAccessKey);
                var s3Client = new AmazonS3Client(credentials, region);

                var fileData = Convert.FromBase64String(orgb64String);
                // Specify a unique key for the object in S3 (e.g., filename)
                var folderName = "submenu";
                Random random = new Random();
                // Generate a random integer
                int randomNumber = random.Next(100000, 1000000); // Random number between 100000 and 999999
                 keyName = $"{folderName}/{subMenu.HomeMenu}_{randomNumber}.png"; // Ensure the file has a .pdf extension

                // Upload the base 64 to S3
                await UploadBytesAsync(s3Client, bucketName, keyName, fileData);
            }

            subMenu.HomeMenu = subMenu.HomeMenu?.Trim();
            subMenu.Module = subMenu.Module?.Trim();
            subMenu.groupIndex = subMenu.groupIndex?.Trim();
            subMenu.Menu = subMenu.Menu?.Trim();
            subMenu.Display_Order = subMenu.Display_Order?.Trim();



            JObject jobject = new JObject();

            //if (string.IsNullOrWhiteSpace(subMenu.Display_Order))
            //{
            //    jobject.Add("status", false);
            //    jobject.Add("message", "Display Order cannot be empty or whitespace only.");
            //    return Ok(jobject.ToString());
            //}


            //string checkDisplayOrderQuery = $"SELECT COUNT(*) FROM M_CONTROLPANEL_MODULES WHERE Display_Order = '{subMenu.Display_Order}'AND Display_Order != '{subMenu}'";
            //int DisplayOrderCount = SQLService.ExecuteScalarQuery(checkDisplayOrderQuery);


            //if (DisplayOrderCount > 0)
            //{
            //    jobject.Add("status", false);
            //    jobject.Add("message", "Display Order already exists.");
            //    return Ok(jobject.ToString());
            //}

            subMenu.groupIndex = await LoadGroupIndexMaxCode(subMenu);
            var controlPanel = new SubMenu
            {
                HomeMenu = subMenu.HomeMenu?.ToUpper(),
                Module = subMenu.Module?.ToUpper(),
                groupIndex = subMenu.groupIndex,
                RouterLink = subMenu.RouterLink,
                Display_Order = subMenu.Display_Order,
                SubMenuImg = keyName ?? "",
                SubMenuIcon = subMenu.SubMenuIcon,
                Is_Active = subMenu?.Is_Active?.ToString() ?? "0",
                Menu = subMenu?.Menu?.ToUpper()
            };

            var listIgnored = string.IsNullOrEmpty(subMenu?.SubMenuImg) ? new List<string> { "SubMenuImg" } : new List<string> { };
            //*** Important *** //
            var ignoredColumns = listIgnored;

            //*** Important *** //
            var UpdateQuery = SQLHelper.BuildUpdateQuery(controlPanel, "M_CONTROLPANEL_MODULES", "fid", id, ignoredColumns);
            int result = SQLService.ExecuteNonQuery(UpdateQuery);
            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Updated Successfully");

            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Updated!");
            }
            return Ok(jobject.ToString());
        }

        private async Task<string> LoadGroupIndexMaxCode(SubMenu subMenu)
        {
            DataTable dtCode = SQLService.GetDataTable("select Display_Order from M_CONTROLPANEL_MENUGROUP WHERE HomeMenu='" + subMenu.HomeMenu + "'");
            if (dtCode.Rows.Count > 0)
            {
                string code = dtCode.Rows[0][0].ToString();
                return code;
            }
            else
            {
                return null;
            }
        }

        private static async Task UploadBytesAsync(IAmazonS3 s3Client, string bucketName, string keyName, byte[] fileData)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(s3Client);

                // Upload the byte array to S3
                using (var stream = new MemoryStream(fileData))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        BucketName = bucketName,
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead // Set ACL to public-read
                    };

                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error uploading file to S3: {ex.Message}");
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("list")]
        public IActionResult List()
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            DataTable dtCode = SQLService.GetDataTable("get_ControlPanelSubMenu");

            JArray jArray = UtilityService.DataTableToJArray(dtCode); ;

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(string id)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Identify the tenant & form the SQL connection string
            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(HttpContext);

            // Fetch the data from SQL database
            DataTable dataTable = SQLService.GetDataTable("SELECT * FROM M_CONTROLPANEL_MODULES WHERE fid='" + id + "'");

            // Form the API return object
            JObject jObject = UtilityService.DataTableToJObject(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jObject;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpGet]
        [Route("delete/{id}")]
        public IActionResult Delete(string id)
        {
            // Define API response object
            JObject jobject = new JObject();
            // Fetch the data from SQL database
            string query = "update M_CONTROLPANEL_MODULES set Is_Active='D' where fid='" + id + "'";
            int result = SQLService.ExecuteNonQuery(query);

            // Form the API return object
            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Deleted Successfully");
            }
            else
            {
                // Add the return data in response object
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete!");
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(jobject));
        }
    }
}
