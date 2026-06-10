namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class OtherSpecificationDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string OrgId { get; set; }
        public string CPUName { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildVersion { get; set; } = "0";
        public string SystemUptime { get; set; }
        public string SystemModel { get; set; }
        public string SystemManufacturer { get; set; }
        public string SerialNumber { get; set; }
        public string InstalledRAM { get; set; }
        public string MACAAddress { get; set; }
        public string BIOSVersion { get; set; }
        public string Antivirus { get; set; }

    }
}
