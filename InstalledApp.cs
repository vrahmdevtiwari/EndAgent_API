using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class InstalledApp
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
        public DateTime CreatedAt { get; set; }
    }
}
