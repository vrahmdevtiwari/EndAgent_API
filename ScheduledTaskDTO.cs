namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class ScheduledTaskDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string NextRunTime { get; set; }
        public string LastRunTime { get; set; }
        public string LastRunResult { get; set; }
        public string Author { get; set; }
        public string Path { get; set; }
        public string Trigger { get; set; }
        public string CreatedDate { get; set; }
    }
}
