using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class OtherSpecification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string CPUName { get; set; }
        public string OSVersion { get; set; }
        public string SystemUptime { get; set; }
        public string SystemModel { get; set; }
        public string SystemManufacturer { get; set; }
        public string SerialNumber { get; set; }
        public string InstalledRAM { get; set; }
        public string MACAAddress { get; set; }
        public string BIOSVersion { get; set; }
        public string Antivirus { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
