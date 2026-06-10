namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class ResourceUtilDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string OrgId { get; set; }
        public float CPUUsage { get; set; }
        public float PhysicalDiskUsage { get; set; }
        public float MemoryUsage { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public float GPUUsage { get; set; }

    }
}
