using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class LocationDTO
    {
        public int ID { get; set; }
        [Required]
        public string AssetId { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public string OrgId { get; set; }

    }
}
