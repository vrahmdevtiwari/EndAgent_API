using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class ResourceUtil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public float CPUUsage { get; set; }
        public float PhysicalDiskUsage { get; set; }
        public float MemoryUsage { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public float GPUUsage { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
