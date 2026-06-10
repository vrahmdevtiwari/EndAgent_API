using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class Update
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string Patch { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set;}
        public DateTime CreatedOn { get; set; }
    }
}
