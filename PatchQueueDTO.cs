using System.ComponentModel.DataAnnotations.Schema;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class PatchQueueDTO
    {
        public string ObjID { get; set; }
        public string DeviceID { get; set; }
        public string CurrentVersion { get; set; }
        public string? InQueueVersion { get; set; }
        public bool InQueue { get; set; } = false;
        public string Status { get; set; }
        public string OrgId { get; set; }
    }
}
