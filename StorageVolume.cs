using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class StorageVolume
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string BootVolume { get; set; }
        public string Capacity { get; set; }
        public string DriveLetter { get; set; }
        public string FileSystem { get; set; }
        public string FreeSpace { get; set; }
        public string Label { get; set; }
        public string SystemVolume { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
