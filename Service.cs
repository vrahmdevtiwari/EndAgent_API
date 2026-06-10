using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceStatus { get; set; }
        public string StartType { get; set; }
        public string PID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
