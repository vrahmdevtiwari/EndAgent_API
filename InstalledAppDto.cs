using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class InstalledAppDto
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        [Required]
        public string AppName { get; set; }
        public string Provider { get; set; }
        public string Size { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
        public string OrgId { get; set; }
    }
}
