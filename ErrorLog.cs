using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class ErrorLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string EndPointName { get; set; }
        public string Error { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
