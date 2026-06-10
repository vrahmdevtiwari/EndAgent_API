using TEST_WebApiOsDetails.Models.Dto;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;

namespace EndAgent_API.Models.Dto__Data_Tranfer_Objects_
{
    // Dev : Srikanth Erukulla - 02-02-2026
    public class OSPatches
    {
        public string SystemID { get; set; } = string.Empty;
        public string OrgID { get; set; } = string.Empty;

        public OSPatchesCount OSPatchesCount { get; set; }
        public List<EPTPatchDataDTO> AvailablePatches { get; set; }
        public List<UpdateDto> InstalledPatches { get; set; }
        public List<InstalledAppDto> InstalledApps { get; set; }

        public string DeviceName { get; set; } = string.Empty;
        public string DeviceBIOS { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;

        public OSPatches()
        {
            AvailablePatches = new List<EPTPatchDataDTO>();
            InstalledPatches = new List<UpdateDto>();
            InstalledApps = new List<InstalledAppDto>();
        }
    }
    public class OSPatchesCount
    {
        public int AvailablePatchesCount { get; set; }
        public int InstalledPatchesCount { get; set; }
    }
}
