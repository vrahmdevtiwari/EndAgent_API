namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class DiskDetailDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string OrgId { get; set; }
        public string Index { get; set; }
        public string DeviceID { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; } //
        public string MediaType { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareRevision { get; set; } //
        public string Capacity { get; set; }
        public string Partitions { get; set; } //
        public string InterfaceType { get; set; } //
        public string Status { get; set; }
        public string InstallDate { get; set; }
        public virtual List<PartitionDetailDTO> PartitionDetails { get; set; }
    }
}
