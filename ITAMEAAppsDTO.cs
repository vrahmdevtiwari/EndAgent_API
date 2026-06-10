using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class ITAMEAAppsDTO
    {
        public string AppName { get; set; }
        public string Provider { get; set; }
        public string Size { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
    }
}
