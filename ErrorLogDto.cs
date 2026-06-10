using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class ErrorLogDto
    {
        public string Id { get; set; }
        public string EndPointName { get; set; }
        public string Error { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
