namespace EndAgent_API.Models
{
    public class ErrorLogRequest
    {
        public string OrgId { get; set; }
        public string SystemId { get; set; }
        public string FunctionalityName { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? StackTrace { get; set; }
    }
}
