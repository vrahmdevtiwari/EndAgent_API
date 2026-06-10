using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class AppLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string AppName { get; set; }
        public string Provider { get; set; }
        public string InstalledOn { get; set; }
        public string Size { get; set; }
        public string Version { get; set; }
        public DateTime LogDate { get; set; }
    }
}
