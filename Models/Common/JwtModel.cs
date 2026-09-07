namespace CommonModels
{
    public class JwtModel
    {
        public string? Database_Server { get; set; } // SID
        public string? Database_Name { get; set; } // TID
        public string? Organization_Code { get; set; } // OID
        public string? Company_Img_Url { get; set; } // OID
        public string? Organization_Name { get; set; } // ONM

        public string? Organization_Address1 { get; set; } // ONM
        public string? Organization_Address2 { get; set; } // ONM
        public string? Organization_City { get; set; } // ONM
        public string? Organization_State { get; set; } // ONM
        public string? Organization_Country { get; set; } // ONM
        public string? Organization_Pincode { get; set; } // ONM
        public string? Organization_Gst_No { get; set; } // ONM
        public string? Organization_FSS_No { get; set; } // ONM

        public string? Branch_Code { get; set; } // BID
        public string? Branch_Name { get; set; } // BNM
        public string? Employee_Code { get; set; } // EID
        public string? Employee_Name { get; set; } // ENM
        public string? User_Name { get; set; } // User_Name
        public string? Password { get; set; } // User_Name
        public string? Software_Rights_Group { get; set; } // Software_Rights_Group
        public string? Login_Type { get; set; } // Web/Device
    }
    public class JwtClaim
    {
        public string ClaimIdentityName { get; set; } // Organization_Code,Branch_Code,Employee_Code
        public string SID { get; set; } // Database_Server
        public string TID { get; set; } // Database_Name
        public string OID { get; set; } // Organization_Code
        public string ONM { get; set; } // Organization_Name
        public string BID { get; set; } // Branch_Code
        public string BNM { get; set; } // Branch_Name
        public string EID { get; set; } // Employee_Code
        public string ENM { get; set; } // Employee_Name
        public string User_Name { get; set; } // User_Name
        public string User_Role { get; set; } // Software_Rights_Group
        public string Login_Type { get; set; } // Web/Device/PasswordReset/SignupActivation/OnlineOrdering
    }
}
