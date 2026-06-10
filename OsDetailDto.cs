using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class OsDetailDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "OS Name is Missing")]
        public string OsName { get; set; }
        [Required(ErrorMessage = "OS Version is Missing")]
        public string OsVersion { get; set; }
        [Required(ErrorMessage = "OS Architecture is Missing")]
        public string OsArchitecture { get; set; }  
    }
}
