using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class ActivePort
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }

        public DateTime CreatedAt { get; set; } 

        public string Proto { get; set; }
        public string LocalAddress { get; set; }
        public string ForeignAddress { get; set; }
        public string State { get; set; }
        public string PID { get; set; }
        public string TaskName { get; set; }

    }
}
