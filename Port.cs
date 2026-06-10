using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class Port
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string PortNumber { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
