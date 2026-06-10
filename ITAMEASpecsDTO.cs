using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class ITAMEASpecsDTO
    {
        
        public string SystemName { get; set; }
        
        public string SystemStatus { get; set; }
        
        public string OperatingSystem { get; set; }
        
        public string LoginUser { get; set; }
        
        public string LastActive { get; set; }
        
        public string Domain { get; set; }
        
        public string Privileges { get; set; }
        
        public string NetworkAdapter { get; set; }
        
        public string IPv4Address { get; set; }
        
        public string IPv6Address { get; set; }
        
        public string Gateway { get; set; }
        
        public string SubnetMask { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
