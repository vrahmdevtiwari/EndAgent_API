namespace EndAgent_API.Models
{
    public class EndpointGroup
    {
        public List<string> DeviceIds { get; set; }
        public string GroupName { get; set; }   // ← added
        public string OrgId {  get; set; }
    }
    public class UpdateEndpointGroup
    {
        public List<string> DeviceIds { get; set; }
        public string GroupId { get; set; }   // ← added
        public string OrgId { get; set; }
    }

    public class DeviceNameForEndpointGroup
    {
        public string ID { get; set; }
        public string SystemName { get; set; }
        public string LoginUser { get; set; }
        public string PublicIP { get; set; }
        public string? GroupName { get; set; }
    }

    public class EndpointGroupVM
    {
        public string Id { get; set; } // GroupId
        public string GroupName { get; set; }
        public int? TotalDeviceCount { get; set; }
        public string GroupPatchScheduledTimeId { get; set; }
        public bool IsGroupPatchEnabled { get; set; }
        public DateTime? ScheduledTime { get; set; }

    }

    

}
