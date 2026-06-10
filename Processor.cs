using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class Processor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string DeviceID { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string MaxClockSpeed { get; set; }
        public string Cores { get; set; }
        public string LogicalProcessors { get; set; }
        public string ProcessorId { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
