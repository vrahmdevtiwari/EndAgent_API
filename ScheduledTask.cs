using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class ScheduledTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string NextRunTime { get; set; }
        public string LastRunTime { get; set; }
        public string LastRunResult { get; set; }
        public string Author { get; set; }
        public string Path { get; set; }
        public string Trigger { get; set; }
        public string CreatedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
