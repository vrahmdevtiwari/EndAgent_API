using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Notifications
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AssetID { get; set; }
        public string DevType { get; set; }
        public string OrgID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? User {  get; set; }
        public string? Header {  get; set; } 
        public string? Body { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Message { get; set; }


    }
}
