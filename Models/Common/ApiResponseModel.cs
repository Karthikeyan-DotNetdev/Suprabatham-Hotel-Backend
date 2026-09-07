using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CommonModels
{

    /// <summary>
    /// Suppresses the default ApiController behaviour of automatically creating error 400 responses
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SuppressModelStateInvalidFilterAttribute : Attribute, IActionModelConvention
    {
        private static readonly Type ModelStateInvalidFilterFactory = typeof(ModelStateInvalidFilter).Assembly.GetType("Microsoft.AspNetCore.Mvc.Infrastructure.ModelStateInvalidFilterFactory");

        public void Apply(ActionModel action)
        {
            for (var i = 0; i < action.Filters.Count; i++)
             {
                if (action.Filters[i] is ModelStateInvalidFilter || action.Filters[i].GetType() == ModelStateInvalidFilterFactory)
                {
                    action.Filters.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public class ApiResponse
    {
        public ApiResponse()
        {
            error = new ErrorResponse();
        }

        [Required, DefaultValue(false)]
        public bool status { get; set; }


        [DefaultValue(null), StringLength(10)]
        public string? status_code { get; set; }


        [DefaultValue(null)]
        public object? data { get; set; }


        [DefaultValue(null)]
        public string? message { get; set; }


        public ErrorResponse error { get; set; }


        [DefaultValue(null)]
        public object? log { get; set; }
    }

    public class ErrorResponse
    {
        [DefaultValue(null), StringLength(10)]
        public string? code { get; set; }

        public string? message { get; set; }
    }
}
