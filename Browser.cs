namespace EndAgent_API.Models.ViewModel
{
    public class BrowserHistyoryViewModel
    {
        public string MachineName { get; set; }
        public List<BrowserViewModel> Browsers { get; set; }
    }

    public class BrowserViewModel
    {
        public List<BrowserHistory> BrowserHistories { get; set; }
        public string BrowserName { get; set; }
    }

    public class BrowserHistory
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string LastVisitTime { get; set; }
    }
}
