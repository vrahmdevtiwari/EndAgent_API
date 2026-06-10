using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models
{
    public class EASpecification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string AssetID { get; set; }
        public string ObjectID { get; set; }
        public string OrgID { get; set; }
        public string  SystemName { get; set; }
        public string SystemStatus { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildVersion { get; set; }
        public string LoginUser { get; set; }
        public string LastActive { get; set; }
        public string Domain { get; set; }
        public string Privileges { get; set; }
        public string NetworkAdapter { get; set; }
        public string IPv4Address { get; set; }
        public string IPv6Address { get; set; }
        public string Gateway { get; set;}
        public string SubnetMask { get; set; }
        public string? ITAMObjID { get; set; }
        public DateTime CreatedAt { get; set;}

 
    }
}
