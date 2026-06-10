using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class GraphicCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string AdapterCompatibility { get; set; }
        public string AdapterRAM { get; set; }
        public string Caption { get; set; }
        public string DeviceID { get; set; }
        public string VideoProcessor { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
