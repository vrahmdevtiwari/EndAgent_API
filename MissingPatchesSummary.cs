namespace EndAgent_API.Models.ViewModel
{
    // ── Fixed Model ───────────────────────────────────────────────────────────────
    public class MissingPatchesSummary
    {
        public List<GroupCount> PlatformList { get; set; } = new();
        public List<GroupCount> VendorList { get; set; } = new();
        public List<GroupCount> CategoryList { get; set; } = new();
        public List<GroupCount> OSVersionList { get; set; } = new();
        public List<GroupCount> ArchitectureList { get; set; } = new();

    }

    public class GroupCount
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }
}
