namespace TEST_WebApiOsDetails.Models
{
    public class FileUploadDTO
    {
        public string? Id { get; set; }
        public string UpdateName { get; set; }
        public string UpdateOS { get; set; }
        public string UpdateBitRate { get; set; }
        public string UpdateID { get; set; }
        public string KBNumber { get; set; }
        public string? KBNumberDescription { get; set; }
        public string? Parameters { get; set; }
        public string UploadedFileBase64 { get; set; }
        public string OrgID { get; set; }

    }
}
