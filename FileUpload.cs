using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    // Used for both Patches and Apps
    public class FileUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UpdateName { get; set; } // Takes uploaded file name always
        public string? UpdateOS { get; set; }
        public string? UpdateBitRate { get; set; }
        public string? FilePath { get; set; }
        public string? UpdateID { get; set; } // Is App Name for apos
        public string? KBNumber { get; set; } // is "app" for Apps
        public string OrgID { get; set; }
    }
}
