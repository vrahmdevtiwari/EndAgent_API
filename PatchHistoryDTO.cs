namespace EndAgent_API.Models.Dto__Data_Tranfer_Objects_
{
    public class PatchHistoryDTO
    {
        public int OrgId { get; set; } // Organization Code
        public string? User { get; set; }  // System Name i.e MDS-HYD-D00
        public string System_Id {  get; set; } // ObjectId
        public string KBNumber {  get; set; }
        public string? KBNumberDescription { get; set; }
        public string Status { get; set; }
        public string? StatusRemarks { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
