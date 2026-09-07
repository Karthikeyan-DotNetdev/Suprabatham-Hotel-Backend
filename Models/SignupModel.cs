using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace laptop_service.Models
{
    public class Signup
    {
        [Required, StringLength(100), DisplayName("Organization Name")]
        public string? Organization_Name { get; set; }

        [Required, StringLength(100), DisplayName("Employee Name")]
        public string? Employee_Name { get; set; }

        [Required, StringLength(20), DisplayName("Phone")]
        public string? Phone { get; set; }

        [Required, StringLength(100), DisplayName("Email")]
        public string? Email { get; set; }

        [Required, StringLength(100), DisplayName("City")]
        public string? City { get; set; }

        [Required, StringLength(100), DisplayName("Country")]
        public string? Country { get; set; }

        [StringLength(100), DisplayName("Sale Channel")]
        public string? Sale_Channel { get; set; }

        [StringLength(100), DisplayName("Sale Person")]
        public string? Sale_Person { get; set; }

    }
}
