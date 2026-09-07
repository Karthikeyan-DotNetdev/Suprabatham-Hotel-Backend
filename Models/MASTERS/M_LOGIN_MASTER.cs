namespace laptop_service.Models.MASTERS
{
    public class M_EMPLOYEE_MASTER
    {
        public int? EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }   // EMP0001

        public string? Name { get; set; }

        public string? MobileNo { get; set; }

        public string? PasswordHash { get; set; }

        public string? BranchCode { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }

        public string? DeviceId { get; set; }

        public string? IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}