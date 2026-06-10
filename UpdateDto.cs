using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class UpdateDto
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string Patch { get; set; }
        public string Title { get; set; }
        [MaxLength(256)] public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
        public string OrgId { get; set; }

    }
}
