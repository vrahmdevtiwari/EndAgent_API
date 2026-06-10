using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TEST_WebApiOsDetails.Models
{
    public class DiskDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string Index { get; set; }
        public string DeviceID { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; } //
        public string MediaType { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareRevision { get; set; } //
        public string Capacity { get; set; }
        public string Partitions { get; set; } //
        public string InterfaceType { get; set; } //
        public string Status { get; set; }
        public string InstallDate { get; set; }
        public virtual List<PartitionDetail> PartitionDetails { get; set; }
        public DateTime CreatedAt { get; set; }

        
    }
}
