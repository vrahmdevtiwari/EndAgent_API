using MongoDB.Bson;

namespace EndAgent_API.Models.ViewModel
{
    public class WindowsPatchGraphViewModel
    {
        /// <summary>All patches with status = "0" (pending / not yet run)</summary>
        public List<BsonDocument> MissingPatches { get; set; } = new();

        /// <summary>Windows-only patches with status = "0" (KB number present, not "app")</summary>
        public List<BsonDocument> WindowsMissingPatches { get; set; } = new();

        /// <summary>All patches with status != "0" and != "1" (e.g. status = "2")</summary>
        public List<BsonDocument> FailedInstalls { get; set; } = new();

        /// <summary>Windows-only failed patches (KB number present, not "app")</summary>
        public List<BsonDocument> WindowsFailedInstalls { get; set; } = new();

        /// <summary>Patches with status = "1" and reason does NOT contain "already installed"</summary>
        public List<BsonDocument> SuccessfulInstalls { get; set; } = new();

        /// <summary>Patches with status = "1" and reason contains "already installed"</summary>
        public List<BsonDocument> AlreadyInstalledPatches { get; set; } = new();

        /// <summary>Aggregated counts for graph rendering</summary>
        public PatchStatusSummary StatusSummary { get; set; } = new();
    }

    public class PatchStatusSummary
    {
        public int Total { get; set; }
        public int Failed { get; set; }
        public int Success { get; set; }
        public int AlreadyInstalled { get; set; }
        public int Missing { get; set; }

        public MissingPatchesSummary MissingPatches { get; set; }

        public int ReportedDevices { get; set; }
        public int ApprovedDevices { get; set; }
        public int UnApprovedDevices { get; set; }
    }
}
