using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace laptop_service.Models
{
    public class Employee
    {
        public string? EmpIdInt { get; set; }
        public string? EmpName { get; set; }
        public string? EmpId { get; set; }
        public string? DOJ { get; set; }
        public string? MobileNo { get; set; }
        public string? EmailId { get; set; }
        public string? Designation { get; set; }
        public string? DepName { get; set; }
        public string? Qualification { get; set; }
        public string? FatherOrHusband { get; set; }
        public string? Gender { get; set; }
        public string? DOB { get; set; }
        public string? AnniversaryDate { get; set; }
        public string? Age { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }
        public string? BankName { get; set; }
        public string? AccNo { get; set; }
        public string? IFSCCode { get; set; }
        public string? PanNo { get; set; }
        public string? UANNo { get; set; }
        public string? ESIOrInsNo { get; set; }
        public string? AadharNo { get; set; }
        public string? ResAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? EmpImg { get; set; }
        public string? AadharImg { get; set; }
        public string? EmpImgUrl { get; set; }
        public string? AadharImgUrl { get; set; }
        public string? CTCPerMonth { get; set; }
        public string? CTCPerYear { get; set; }
        public string? TotalCTCMonthly { get; set; }
        public string? TotalCTCYearly { get; set; }
        public string? BasicSalPer { get; set; }
        public string? BasicSalMonth { get; set; }
        public string? BasicSalYear { get; set; }
        public string? HouseRentAllowancePerc { get; set; }
        public string? HouseRentPerMonth { get; set; }
        public string? HouseRentPerYear { get; set; }
        public string? ConveyancePerMonth { get; set; }
        public string? ConveyancePerYear { get; set; }
        public string? OtherAllowPerMonth { get; set; }
        public string? OtherAllowPerYear { get; set; }
        public string? PFPercent { get; set; }
        public string? PFPerMonth { get; set; }
        public string? PFPerYear { get; set; }
        public string? ESIPercent { get; set; }
        public string? ESIPerMonth { get; set; }
        public string? ESIPerYear { get; set; }
        public string? TotDeductionMonthly { get; set; }
        public string? TotDeductionYearly { get; set; }
        public string? Is_Active { get; set; }
        public string? fBranch { get; set; }
        public string? fComp { get; set; }
        public string? Fy { get; set; }
        public string? CreateDate { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? User_Code { get; set; }
        public string? User_Code_Int { get; set; }
        public string? User_Branch { get; set; }
    }
    public class Login
    {

        [Required, StringLength(100), DisplayName("User Name")]
        public string? User_Name { get; set; }

        [Required, StringLength(50), DisplayName("Password")]
        public string? Password { get; set; }
    }

    public class AWSFolderName
    {
        //Bros it AWS
        //public static string BucketName { get; set; } = "";
        //public static string AccessKeyId { get; set; } = "";
        //public static string SecretAccessKey { get; set; } = "";

        //ScangoldQR
        public static string BucketName { get; set; } = "kovilpatti";
        public static string AccessKeyId { get; set; } = "";
        public static string SecretAccessKey { get; set; } = "";
    }



}
