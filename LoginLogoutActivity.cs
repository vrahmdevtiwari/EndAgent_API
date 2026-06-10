namespace EndAgent_API.Models.Dto__Data_Tranfer_Objects_
{
    public class LoginLogoutActivity
    {
        public string ActivityType { get; set; }
        public DateTime ActivityTime { get; set; }
        public string LoginUser { get; set; }
        public string SystemName { get; set; }
        public string ObjectId { get; set; }
        public string OrgId { get; set; }
    }
}
