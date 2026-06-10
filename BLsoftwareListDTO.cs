using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class BLsoftwareListDTO
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string AssetId { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
