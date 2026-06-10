using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class RAIDController
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public string PNPDeviceID  { get; set; }
            public string Manufacturer { get; set; }
            public string Caption { get; set; }
            public string Description { get; set; }
            public string SystemCreationClassName { get; set; }
            public string SystemName { get; set; }
            public string ConfigManagerErrorCode { get; set; }
            public string ConfigManagerUserConfig { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
