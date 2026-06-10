namespace TEST_WebApiOsDetails.Models.Dto
{
    public class TaskManagerDTO
    {
        public string Id { get; set; }
        public double CPUUsage {  get; set; }
        public double MemoryUsage {  get; set; }
        public double DiskUsage { get; set; }
        public double NetworkUsage { get; set; }
        public string OrgId {  get; set; }
        public string ObjectId {  get; set; }
        public string SystemName {  get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
