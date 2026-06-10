using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class ITAMEAUpdatesDTO
    {
        public string Patch { get; set; }
        public string Title { get; set; }
        [MaxLength(256)] 
        public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
    }
}
