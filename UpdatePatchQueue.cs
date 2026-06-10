using System;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class UpdatePatchQueue
    {
        [Key]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        // Patch reference from central repo
        public string PatchId { get; set; } = string.Empty;

        // System / Asset
        public string SystemID { get; set; } = string.Empty;

        // Patch display title (from central repo)
        public string UpdateName { get; set; } = string.Empty;

        // Actual file name on disk (ex: windows11.0-kb5077373-x64_8c2eeb32cace3652438ea1e53f15beccd7b1f70e.msu)
        public string PatchFileName { get; set; } = string.Empty;

        // Full file path for the agent to download
        public string PatchFilePath { get; set; } = string.Empty;

        // KB Number (ex: KB5077373)
        public string UpdateKBNumber { get; set; } = string.Empty;

        // Status (0 = queued, 1 = installed, etc.)
        public string Status { get; set; } = "0";

        // Optional reason / message
        public string? Reason { get; set; }

        // Queue creation time
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledTime { get; set; } //ScheduledForInstallation
        public string? OrgId {  get; set; } = string.Empty;
    }
}