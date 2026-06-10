namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class EPTPatchDataDTO
    {
        public string PatchID { get; set; }
        public string PatchName { get; set; }
        public string PatchTitle { get; set; } = string.Empty;       // UI Display
        public string PatchFileName { get; set; } = string.Empty;    // Actual file name
        public string PatchFilePath { get; set; } = string.Empty;    // Full file path
        public string? PatchOS {  get; set; }
        public string? PatchBitRate {  get; set; }
        public string KBNumber { get; set; }
        public string? KBNumberDescription { get; set; }
        public string? PatchStatus { get; set; }

    }
}
