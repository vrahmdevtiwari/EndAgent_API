using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class NetworkAdapter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InterfaceIndex { get; set; }
        public string Status { get; set; }
        public string MACAddress { get; set; }
        public string Speed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
