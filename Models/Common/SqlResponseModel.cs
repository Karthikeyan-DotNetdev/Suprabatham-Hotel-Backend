using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CommonModels
{
    public class SqlResponse
    {
        [DefaultValue(false)]
        public bool status { get; set; }


        [DefaultValue(0)]
        public long count { get; set; }


        [DefaultValue(null)]
        public string? message { get; set; }


        [DefaultValue(null), StringLength(10)]
        public string? code { get; set; }


        [DefaultValue(null)]
        public object? data { get; set; }
    }
}
