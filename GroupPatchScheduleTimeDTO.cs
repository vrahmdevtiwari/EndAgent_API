namespace EndAgent_API.Models.Dto__Data_Tranfer_Objects_
{
    public class GroupPatchScheduleTimeDTO
    {
        public string OrgId { get; set; }
        public string GroupId { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string GroupPatchScheduledTimeId { get; set; }
    }
}
