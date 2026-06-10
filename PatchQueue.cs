using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TEST_WebApiOsDetails.Models
{
    // Dev: Viraj; Date: 06-04-2024
    public class PatchQueue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DeviceID { get; set; }

        [ForeignKey("DeviceID")]
        public Device Device { get; set; }

        public string CurrentVersion { get; set; }
        public string? InQueueVersion { get; set; }
        public bool InQueue { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
