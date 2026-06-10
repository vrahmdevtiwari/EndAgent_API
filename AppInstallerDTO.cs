using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EndAgent_API.Models.Dto__Data_Tranfer_Objects_
{
    public class AppInstallerDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string AppName { get; set; }

        // Primary lookup key (vlc.exe, chrome.msi)
        public string ExecutableName { get; set; }

        // exe | msi
        public string InstallerType { get; set; }

        // Silent install args (/S, /qn /norestart)
        public string SilentArgs { get; set; }

        // Silent uninstall args
        public string UninstallArgs { get; set; }

        // file | registry | msi
        public string DetectionType { get; set; }

        // Path / registry key / product code
        public string DetectionValue { get; set; }

        // Some EXEs fail in SYSTEM context
        public bool RequiresUserSession { get; set; }

        // Expected reboot requirement
        public bool RebootRequired { get; set; }

        // Per-app timeout
        public int InstallTimeoutMinutes { get; set; } = 10;

        // Kill switch
        public bool Enabled { get; set; }

        // Metadata
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
