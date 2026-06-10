using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class PhysicalDrive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }

        public string DeviceID { get; set; }
        public string FirmwareRevision { get; set; }
        public string Index { get; set; }
        public string InterfaceType { get; set; }
        public string MediaType { get; set; }
        public string Model { get; set; }
        public string Partitions { get; set; }
        public string SerialNumber { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
