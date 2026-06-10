using TEST_WebApiOsDetails.Models;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class PatchResponseDto
    {
        public List<CentralOSPatches> OSPatches { get; set; } = new();
        public List<CentralSoftwarePatches> SoftwarePatches { get; set; } = new();
    }
}
