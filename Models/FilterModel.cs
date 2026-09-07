using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace laptop_service.Models
{
    public class FilterModel
    {
        [Required, StringLength(10), DisplayName("From Date")]
        public string? From_Date { get; set; }

        [Required, StringLength(10), DisplayName("To Date")]
        public string? To_Date { get; set; }
    }
}
