using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class RAMDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string BankLabel { get; set; }
        public string Capacity { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string MemoryType { get; set; }
        public string PartNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Speed { get; set; }
        public string SMBIOSMemoryType { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
