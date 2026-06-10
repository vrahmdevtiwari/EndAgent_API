namespace EndAgent_API.Models
{
    public class AgentLogDto
    {
        public string OrgId { get; set; }
        public string SystemId { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}
