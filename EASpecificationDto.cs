using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class EASpecificationDto
    {
        public string ID { get; set; }
        [Required]
        public string SystemName { get; set; }
        [Required]
        public string SystemStatus { get; set; }
        [Required]
        public string OperatingSystem { get; set; }
        public string? OSVersion { get; set; }
        public string? OSBuildVersion { get; set; }
        [Required]
        public string LoginUser { get; set; }
        [Required]
        public string LastActive { get; set; }
        [Required]
        public string Domain { get; set; }
        [Required]
        public string Privileges { get; set; }
        [Required]
        public string NetworkAdapter { get; set; }
        [Required]
        public string IPv4Address { get; set; }
        [Required]
        public string IPv6Address { get; set; }
        [Required]
        public string Gateway { get; set; }
        [Required]
        public string SubnetMask { get; set; }
        public DateTime CreatedAt { get; set; }

        public string AssetID { get; set; }
        public string OrgID { get; set; }

    }
}
