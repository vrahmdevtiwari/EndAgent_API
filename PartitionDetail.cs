using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class PartitionDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string DeviceID { get; set; }
        public string Index { get; set; }
        public string DiskIndex { get; set; }
        public string Bootable { get; set; }
        public string BootPartition { get; set; }
        public string PrimaryPartition { get; set; }
        public string Size { get; set; }
        public string State { get; set; }
        public string DriveLetter { get; set; }
        public string FileSystem { get; set; }
        public string FreeSpace { get; set; }
        public string UsedSpace { get; set; }
        public string Description { get; set; }
        public string VolumeName { get; set; }
        public int DiskDetailId { get; set; }
        
        [ForeignKey("DiskDetailId")]
        public virtual DiskDetail DiskDetail { get; set; }
    }
}
