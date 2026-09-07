using AppSettings;
using CommonModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommonServices
{
    public class JwtService
    {
        public static string GenerateJSONWebToken(JwtModel jwtModel)
        {
            var key = Encoding.UTF8.GetBytes(AppSetting.Jwt.Key);
            var securityKey = new SymmetricSecurityKey(key);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            string ClaimIdentityName = jwtModel.Organization_Code + "," + jwtModel.Branch_Code + "," + jwtModel.Employee_Code;

          //  jwtModel.Database_Server = CipherService.Encrypt(jwtModel.Database_Server);
         //   jwtModel.Database_Name = CipherService.Encrypt(jwtModel.Database_Name);
            jwtModel.Organization_Code = CipherService.Encrypt(jwtModel.Organization_Code);
            jwtModel.Organization_Name = CipherService.Encrypt(jwtModel.Organization_Name);
            jwtModel.Branch_Code = CipherService.Encrypt(jwtModel.Branch_Code);
            jwtModel.Branch_Name = CipherService.Encrypt(jwtModel.Branch_Name);
            jwtModel.Employee_Code = CipherService.Encrypt(jwtModel.Employee_Code);
            
            
            jwtModel.Employee_Name = CipherService.Encrypt(jwtModel.Employee_Name);
            jwtModel.User_Name = CipherService.Encrypt(jwtModel.User_Name);
            jwtModel.Software_Rights_Group = CipherService.Encrypt(jwtModel.Software_Rights_Group);

            var claims = new[] {
                new Claim(ClaimTypes.Name, ClaimIdentityName),
              //  new Claim("SID", jwtModel.Database_Server),
             //   new Claim("TID", jwtModel.Database_Name),
                new Claim("OID", jwtModel.Organization_Code),
                new Claim("ONM", jwtModel.Organization_Name),
                new Claim("BID", jwtModel.Branch_Code),
                new Claim("BNM", jwtModel.Branch_Name),
                new Claim("EID", jwtModel.Employee_Code),
                new Claim("ENM", jwtModel.Employee_Name),
                new Claim("User_Name", jwtModel.User_Name),
                new Claim("User_Role", jwtModel.Software_Rights_Group),
                new Claim("Login_Type", jwtModel.Login_Type)
            };

            int timeOutMinutes = 0;
            if (jwtModel.Login_Type == "Web")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutes);
            }
            else if (jwtModel.Login_Type == "Device")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesDevice);
            }
            else if (jwtModel.Login_Type == "Captain")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesCaptain);
            }
            else if (jwtModel.Login_Type == "PasswordReset")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesPasswordReset);
            }

            var token = new JwtSecurityToken(
              issuer: AppSetting.Jwt.Issuer,
              audience: AppSetting.Jwt.Audience,
              claims: claims,
              notBefore: DateTime.Now.AddMinutes(-1),
              expires: DateTime.Now.AddMinutes(timeOutMinutes),
              signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /*
        public static string GeneratePasswordResetJSONWebToken(string UserName, string DatabaseServer, string DatabaseName, string OrganizationCode, string EmployeeCode)
        {
            var key = Encoding.UTF8.GetBytes(AppSetting.Jwt.Key);
            var securityKey = new SymmetricSecurityKey(key);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            DatabaseServer = CipherService.Encrypt(DatabaseServer);
            DatabaseName = CipherService.Encrypt(DatabaseName);
            OrganizationCode = CipherService.Encrypt(OrganizationCode);
            EmployeeCode = CipherService.Encrypt(EmployeeCode);

            var claims = new[] {
                new Claim(ClaimTypes.Name, UserName),
                new Claim("SID", DatabaseServer),
                new Claim("TID", DatabaseName),
                new Claim("OID", OrganizationCode),
                new Claim("EID", EmployeeCode),
                new Claim("Login_Type", "PasswordReset")
            };

            var token = new JwtSecurityToken(
              issuer: AppSetting.Jwt.Issuer,
              audience: AppSetting.Jwt.Audience,
              claims: claims,
              notBefore: DateTime.Now.AddMinutes(-1),
              expires: DateTime.Now.AddMinutes(Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesPasswordReset)),
              signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        */
        public static string RefreshJSONWebToken(HttpContext httpContext)
        {
            var key = Encoding.UTF8.GetBytes(AppSetting.Jwt.Key);
            var securityKey = new SymmetricSecurityKey(key);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtClaim jwtClaim = GetRawJwtClaimFromJSONWebToken(httpContext);

            var claims = new[] {
                new Claim(ClaimTypes.Name, jwtClaim.ClaimIdentityName),
                new Claim("SID", jwtClaim.SID),
                new Claim("TID", jwtClaim.TID),
                new Claim("OID", jwtClaim.OID),
                new Claim("ONM", jwtClaim.ONM),
                new Claim("BID", jwtClaim.BID),
                new Claim("BNM", jwtClaim.BNM),
                new Claim("EID", jwtClaim.EID),
                new Claim("ENM", jwtClaim.ENM),
                new Claim("User_Name", jwtClaim.User_Name),
                new Claim("User_Role", jwtClaim.User_Role),
                new Claim("Login_Type", jwtClaim.Login_Type)
            };

            int timeOutMinutes = 0;
            if (jwtClaim.Login_Type == "Web")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutes);
            }
            else if (jwtClaim.Login_Type == "Device")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesDevice);
            }
            else if (jwtClaim.Login_Type == "Captain")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesCaptain);
            }
            else if (jwtClaim.Login_Type == "PasswordReset")
            {
                timeOutMinutes = Convert.ToInt32(AppSetting.Jwt.TimeOutMinutesPasswordReset);
            }

            var token = new JwtSecurityToken(
              issuer: AppSetting.Jwt.Issuer,
              audience: AppSetting.Jwt.Audience,
              claims: claims,
              notBefore: DateTime.Now.AddMinutes(-1),
              expires: DateTime.Now.AddMinutes(timeOutMinutes),
              signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GenerateSignupActivationJSONWebToken(string Email)
        {
            var key = Encoding.UTF8.GetBytes(AppSetting.Jwt.Key);
            var securityKey = new SymmetricSecurityKey(key);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim(ClaimTypes.Name, Email),
                new Claim("Login_Type", "SignupActivation")
            };

            var token = new JwtSecurityToken(
              issuer: AppSetting.Jwt.Issuer,
              audience: AppSetting.Jwt.Audience,
              claims: claims,
              notBefore: DateTime.Now.AddMinutes(-1),
              expires: DateTime.Now.AddDays(5),
              signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static JwtClaim GetRawJwtClaimFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;

            JwtClaim jwtClaim = new JwtClaim();

            try
            {
                jwtClaim.ClaimIdentityName = currentUser.Identity.Name.ToString();
            }
            catch { }

            // Database_Server
            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                jwtClaim.SID = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
            }

            // Database_Name
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                jwtClaim.TID = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
            }

            // Organization_Code
            if (currentUser.HasClaim(c => c.Type == "OID"))
            {
                jwtClaim.OID = currentUser.Claims.FirstOrDefault(c => c.Type == "OID").Value.ToString();
            }

            // Organization_Name
            if (currentUser.HasClaim(c => c.Type == "ONM"))
            {
                jwtClaim.ONM = currentUser.Claims.FirstOrDefault(c => c.Type == "ONM").Value.ToString();
            }

            // Branch_Code
            if (currentUser.HasClaim(c => c.Type == "BID"))
            {
                jwtClaim.BID = currentUser.Claims.FirstOrDefault(c => c.Type == "BID").Value.ToString();
            }

            // Branch_Name
            if (currentUser.HasClaim(c => c.Type == "BNM"))
            {
                jwtClaim.BNM = currentUser.Claims.FirstOrDefault(c => c.Type == "BNM").Value.ToString();
            }

            // Employee_Code
            if (currentUser.HasClaim(c => c.Type == "EID"))
            {
                jwtClaim.EID = currentUser.Claims.FirstOrDefault(c => c.Type == "EID").Value.ToString();
            }

            // Employee_Name
            if (currentUser.HasClaim(c => c.Type == "ENM"))
            {
                jwtClaim.ENM = currentUser.Claims.FirstOrDefault(c => c.Type == "ENM").Value.ToString();
            }

            // User_Name
            if (currentUser.HasClaim(c => c.Type == "User_Name"))
            {
                jwtClaim.User_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Name").Value.ToString();
            }

            // Software_Rights_Group
            if (currentUser.HasClaim(c => c.Type == "User_Role"))
            {
                jwtClaim.User_Role = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Role").Value.ToString();
            }

            // Login_Type : Web/Device
            if (currentUser.HasClaim(c => c.Type == "Login_Type"))
            {
                jwtClaim.Login_Type = currentUser.Claims.FirstOrDefault(c => c.Type == "Login_Type").Value.ToString();
            }

            return jwtClaim;
        }
        public static JwtClaim GetJwtClaimFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;

            JwtClaim jwtClaim = new JwtClaim();

            try
            {
                jwtClaim.ClaimIdentityName = currentUser.Identity.Name.ToString();
            }
            catch { }

            // Database_Server
            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                jwtClaim.SID = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
                jwtClaim.SID = CipherService.Decrypt(jwtClaim.SID);
            }

            // Database_Name
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                jwtClaim.TID = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
                jwtClaim.TID = CipherService.Decrypt(jwtClaim.TID);
            }

            // Organization_Code
            if (currentUser.HasClaim(c => c.Type == "OID"))
            {
                jwtClaim.OID = currentUser.Claims.FirstOrDefault(c => c.Type == "OID").Value.ToString();
                jwtClaim.OID = CipherService.Decrypt(jwtClaim.OID);
            }

            // Organization_Name
            if (currentUser.HasClaim(c => c.Type == "ONM"))
            {
                jwtClaim.ONM = currentUser.Claims.FirstOrDefault(c => c.Type == "ONM").Value.ToString();
                jwtClaim.ONM = CipherService.Decrypt(jwtClaim.ONM);
            }

            // Branch_Code
            if (currentUser.HasClaim(c => c.Type == "BID"))
            {
                jwtClaim.BID = currentUser.Claims.FirstOrDefault(c => c.Type == "BID").Value.ToString();
                jwtClaim.BID = CipherService.Decrypt(jwtClaim.BID);
            }

            // Branch_Name
            if (currentUser.HasClaim(c => c.Type == "BNM"))
            {
                jwtClaim.BNM = currentUser.Claims.FirstOrDefault(c => c.Type == "BNM").Value.ToString();
                jwtClaim.BNM = CipherService.Decrypt(jwtClaim.BNM);
            }

            // Employee_Code
            if (currentUser.HasClaim(c => c.Type == "EID"))
            {
                jwtClaim.EID = currentUser.Claims.FirstOrDefault(c => c.Type == "EID").Value.ToString();
                jwtClaim.EID = CipherService.Decrypt(jwtClaim.EID);
            }

            // Employee_Name
            if (currentUser.HasClaim(c => c.Type == "ENM"))
            {
                jwtClaim.ENM = currentUser.Claims.FirstOrDefault(c => c.Type == "ENM").Value.ToString();
                jwtClaim.ENM = CipherService.Decrypt(jwtClaim.ENM);
            }

            // User_Name
            if (currentUser.HasClaim(c => c.Type == "User_Name"))
            {
                jwtClaim.User_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Name").Value.ToString();
                jwtClaim.User_Name = CipherService.Decrypt(jwtClaim.User_Name);
            }

            // Software_Rights_Group
            if (currentUser.HasClaim(c => c.Type == "User_Role"))
            {
                jwtClaim.User_Role = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Role").Value.ToString();
                jwtClaim.User_Role = CipherService.Decrypt(jwtClaim.User_Role);
            }

            // Login_Type : Web/Device
            if (currentUser.HasClaim(c => c.Type == "Login_Type"))
            {
                jwtClaim.Login_Type = currentUser.Claims.FirstOrDefault(c => c.Type == "Login_Type").Value.ToString();
            }

            return jwtClaim;
        }
        public static JwtClaim GetJwtClaimFromJSONWebToken(ClaimsPrincipal claimsPrincipal)
        {
            var currentUser = claimsPrincipal;

            JwtClaim jwtClaim = new JwtClaim();

            try
            {
                jwtClaim.ClaimIdentityName = currentUser.Identity.Name.ToString();
            }
            catch { }

            // Database_Server
            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                jwtClaim.SID = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
                jwtClaim.SID = CipherService.Decrypt(jwtClaim.SID);
            }

            // Database_Name
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                jwtClaim.TID = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
                jwtClaim.TID = CipherService.Decrypt(jwtClaim.TID);
            }

            // Organization_Code
            if (currentUser.HasClaim(c => c.Type == "OID"))
            {
                jwtClaim.OID = currentUser.Claims.FirstOrDefault(c => c.Type == "OID").Value.ToString();
                jwtClaim.OID = CipherService.Decrypt(jwtClaim.OID);
            }

            // Organization_Name
            if (currentUser.HasClaim(c => c.Type == "ONM"))
            {
                jwtClaim.ONM = currentUser.Claims.FirstOrDefault(c => c.Type == "ONM").Value.ToString();
                jwtClaim.ONM = CipherService.Decrypt(jwtClaim.ONM);
            }

            // Branch_Code
            if (currentUser.HasClaim(c => c.Type == "BID"))
            {
                jwtClaim.BID = currentUser.Claims.FirstOrDefault(c => c.Type == "BID").Value.ToString();
                jwtClaim.BID = CipherService.Decrypt(jwtClaim.BID);
            }

            // Branch_Name
            if (currentUser.HasClaim(c => c.Type == "BNM"))
            {
                jwtClaim.BNM = currentUser.Claims.FirstOrDefault(c => c.Type == "BNM").Value.ToString();
                jwtClaim.BNM = CipherService.Decrypt(jwtClaim.BNM);
            }

            // Employee_Code
            if (currentUser.HasClaim(c => c.Type == "EID"))
            {
                jwtClaim.EID = currentUser.Claims.FirstOrDefault(c => c.Type == "EID").Value.ToString();
                jwtClaim.EID = CipherService.Decrypt(jwtClaim.EID);
            }

            // Employee_Name
            if (currentUser.HasClaim(c => c.Type == "ENM"))
            {
                jwtClaim.ENM = currentUser.Claims.FirstOrDefault(c => c.Type == "ENM").Value.ToString();
                jwtClaim.ENM = CipherService.Decrypt(jwtClaim.ENM);
            }

            // User_Name
            if (currentUser.HasClaim(c => c.Type == "User_Name"))
            {
                jwtClaim.User_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Name").Value.ToString();
                jwtClaim.User_Name = CipherService.Decrypt(jwtClaim.User_Name);
            }

            // Software_Rights_Group
            if (currentUser.HasClaim(c => c.Type == "User_Role"))
            {
                jwtClaim.User_Role = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Role").Value.ToString();
                jwtClaim.User_Role = CipherService.Decrypt(jwtClaim.User_Role);
            }

            // Login_Type : Web/Device
            if (currentUser.HasClaim(c => c.Type == "Login_Type"))
            {
                jwtClaim.Login_Type = currentUser.Claims.FirstOrDefault(c => c.Type == "Login_Type").Value.ToString();
            }

            return jwtClaim;
        }
        public static JwtClaim GetJwtClaimFromJSONWebToken(TokenValidatedContext tokenValidatedContext)
        {
            var currentUser = tokenValidatedContext.Principal;

            JwtClaim jwtClaim = new JwtClaim();

            try
            {
                jwtClaim.ClaimIdentityName = currentUser.Identity.Name.ToString();
            }
            catch { }

            // Database_Server
            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                jwtClaim.SID = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
                jwtClaim.SID = CipherService.Decrypt(jwtClaim.SID);
            }

            // Database_Name
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                jwtClaim.TID = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
                jwtClaim.TID = CipherService.Decrypt(jwtClaim.TID);
            }

            // Organization_Code
            if (currentUser.HasClaim(c => c.Type == "OID"))
            {
                jwtClaim.OID = currentUser.Claims.FirstOrDefault(c => c.Type == "OID").Value.ToString();
                jwtClaim.OID = CipherService.Decrypt(jwtClaim.OID);
            }

            // Organization_Name
            if (currentUser.HasClaim(c => c.Type == "ONM"))
            {
                jwtClaim.ONM = currentUser.Claims.FirstOrDefault(c => c.Type == "ONM").Value.ToString();
                jwtClaim.ONM = CipherService.Decrypt(jwtClaim.ONM);
            }

            // Branch_Code
            if (currentUser.HasClaim(c => c.Type == "BID"))
            {
                jwtClaim.BID = currentUser.Claims.FirstOrDefault(c => c.Type == "BID").Value.ToString();
                jwtClaim.BID = CipherService.Decrypt(jwtClaim.BID);
            }

            // Branch_Name
            if (currentUser.HasClaim(c => c.Type == "BNM"))
            {
                jwtClaim.BNM = currentUser.Claims.FirstOrDefault(c => c.Type == "BNM").Value.ToString();
                jwtClaim.BNM = CipherService.Decrypt(jwtClaim.BNM);
            }

            // Employee_Code
            if (currentUser.HasClaim(c => c.Type == "EID"))
            {
                jwtClaim.EID = currentUser.Claims.FirstOrDefault(c => c.Type == "EID").Value.ToString();
                jwtClaim.EID = CipherService.Decrypt(jwtClaim.EID);
            }

            // Employee_Name
            if (currentUser.HasClaim(c => c.Type == "ENM"))
            {
                jwtClaim.ENM = currentUser.Claims.FirstOrDefault(c => c.Type == "ENM").Value.ToString();
                jwtClaim.ENM = CipherService.Decrypt(jwtClaim.ENM);
            }

            // User_Name
            if (currentUser.HasClaim(c => c.Type == "User_Name"))
            {
                jwtClaim.User_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Name").Value.ToString();
                jwtClaim.User_Name = CipherService.Decrypt(jwtClaim.User_Name);
            }

            // Software_Rights_Group
            if (currentUser.HasClaim(c => c.Type == "User_Role"))
            {
                jwtClaim.User_Role = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Role").Value.ToString();
                jwtClaim.User_Role = CipherService.Decrypt(jwtClaim.User_Role);
            }

            // Login_Type : Web/Device
            if (currentUser.HasClaim(c => c.Type == "Login_Type"))
            {
                jwtClaim.Login_Type = currentUser.Claims.FirstOrDefault(c => c.Type == "Login_Type").Value.ToString();
            }

            return jwtClaim;
        }
        public static string GetClaimIdentityNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string ClaimIdentityName = "";

            try
            {
                ClaimIdentityName = currentUser.Identity.Name.ToString();
            }
            catch { }

            return ClaimIdentityName;
        }
        public static string GetDatabaseServerFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Database_Server = "";

            // Database_Server
            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                Database_Server = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
                Database_Server = CipherService.Decrypt(Database_Server);
            }

            return Database_Server;
        }
        public static string GetDatabaseNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Database_Name = "";

            // Database_Name
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                Database_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
                Database_Name = CipherService.Decrypt(Database_Name);
            }

            return Database_Name;
        }
        public static string GetOrganizationCodeFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Organization_Code = "";

            // Organization_Code
            if (currentUser.HasClaim(c => c.Type == "OID"))
            {
                Organization_Code = currentUser.Claims.FirstOrDefault(c => c.Type == "OID").Value.ToString();
                Organization_Code = CipherService.Decrypt(Organization_Code);
            }

            return Organization_Code;
        }
        public static string GetOrganizationNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Organization_Name = "";

            // Organization_Name
            if (currentUser.HasClaim(c => c.Type == "ONM"))
            {
                Organization_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "ONM").Value.ToString();
                Organization_Name = CipherService.Decrypt(Organization_Name);
            }

            return Organization_Name;
        }
        public static string GetBranchCodeFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Branch_Code = "";

            // Branch_Code
            if (currentUser.HasClaim(c => c.Type == "BID"))
            {
                Branch_Code = currentUser.Claims.FirstOrDefault(c => c.Type == "BID").Value.ToString();
                Branch_Code = CipherService.Decrypt(Branch_Code);
            }

            return Branch_Code;
        }
        public static string GetBranchNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Branch_Name = "";

            // Branch_Name
            if (currentUser.HasClaim(c => c.Type == "BNM"))
            {
                Branch_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "BNM").Value.ToString();
                Branch_Name = CipherService.Decrypt(Branch_Name);
            }

            return Branch_Name;
        }
        public static string GetEmployeeCodeFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Employee_Code = "";

            // Employee_Code
            if (currentUser.HasClaim(c => c.Type == "EID"))
            {
                Employee_Code = currentUser.Claims.FirstOrDefault(c => c.Type == "EID").Value.ToString();
                Employee_Code = CipherService.Decrypt(Employee_Code);
            }

            return Employee_Code;
        }
        public static string GetEmployeeNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Employee_Name = "";

            // Employee_Name
            if (currentUser.HasClaim(c => c.Type == "ENM"))
            {
                Employee_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "ENM").Value.ToString();
                Employee_Name = CipherService.Decrypt(Employee_Name);
            }

            return Employee_Name;
        }
        public static string GetUserNameFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string User_Name = "";

            // User_Name
            if (currentUser.HasClaim(c => c.Type == "User_Name"))
            {
                User_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Name").Value.ToString();
                User_Name = CipherService.Decrypt(User_Name);
            }

            return User_Name;
        }
        public static string GetSoftwareRightsGroupFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Software_Rights_Group = "";

            // Software_Rights_Group
            if (currentUser.HasClaim(c => c.Type == "User_Role"))
            {
                Software_Rights_Group = currentUser.Claims.FirstOrDefault(c => c.Type == "User_Role").Value.ToString();
                Software_Rights_Group = CipherService.Decrypt(Software_Rights_Group);
            }

            return Software_Rights_Group;
        }
        public static string GetLoginTypeFromJSONWebToken(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            string Login_Type = "";

            // Login_Type : Web/Device
            if (currentUser.HasClaim(c => c.Type == "Login_Type"))
            {
                Login_Type = currentUser.Claims.FirstOrDefault(c => c.Type == "Login_Type").Value.ToString();
            }

            return Login_Type;
        }

        public static bool GetJwtClaimIsValid(TokenValidatedContext tokenValidatedContext)
        {
            bool tokenValidateStatus = false;

            JwtClaim jwtClaim = new JwtClaim();
            jwtClaim = GetJwtClaimFromJSONWebToken(tokenValidatedContext);

            if (jwtClaim.Login_Type != "PasswordReset" && jwtClaim.Login_Type != "SignupActivation")
            {
                if (tokenValidatedContext.Request.Headers.TryGetValue("OID", out var oid))
                {
                    if (jwtClaim.OID == oid)
                    {
                        if (jwtClaim.Login_Type == "Device")
                        {
                            tokenValidateStatus = true; // to avoid Android/iOS app token validation.
                        }
                        else if (tokenValidatedContext.Request.Headers.TryGetValue("EID", out var eid))
                        {
                            if (jwtClaim.EID == eid)
                            {
                                tokenValidateStatus = true;
                            }
                        }
                    }
                }
            }
            else
            {
                tokenValidateStatus = true;
            }

            return tokenValidateStatus;
        }

    }
}
