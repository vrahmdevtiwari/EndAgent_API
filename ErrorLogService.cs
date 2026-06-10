using System.Text;

namespace EndAgent_API.data
{
    public class ErrorLogService
    {
        private readonly IConfiguration _configuration;

        public ErrorLogService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task WriteLogAsync(Models.ErrorLogRequest request)
        {
            try
            {
                string basePath = _configuration["EAServiceLogs:Path"];

                // =========================
                // Org Folder
                // =========================
                string orgFolder = Path.Combine(basePath, request.OrgId);

                if (!Directory.Exists(orgFolder))
                {
                    Directory.CreateDirectory(orgFolder);
                }

                // =========================
                // System Folder
                // =========================
                string systemFolder = Path.Combine(orgFolder, request.SystemId);

                if (!Directory.Exists(systemFolder))
                {
                    Directory.CreateDirectory(systemFolder);
                }

                // =========================
                // Date Folder
                // Example: 15-06-2026
                // =========================
                string dateFolderName = DateTime.Now.ToString("dd-MM-yyyy");

                string dateFolder = Path.Combine(systemFolder, dateFolderName);

                if (!Directory.Exists(dateFolder))
                {
                    Directory.CreateDirectory(dateFolder);
                }

                // =========================
                // Hour File
                // Example:
                // 9AM.txt
                // 10AM.txt
                // =========================
                int hour = DateTime.Now.Hour;

                string hourFileName;

                if (hour == 0)
                {
                    hourFileName = "12AM.txt";
                }
                else if (hour < 12)
                {
                    hourFileName = $"{hour}AM.txt";
                }
                else if (hour == 12)
                {
                    hourFileName = "12PM.txt";
                }
                else
                {
                    hourFileName = $"{hour - 12}PM.txt";
                }

                string filePath = Path.Combine(dateFolder, hourFileName);

                // =========================
                // Log Content
                // =========================
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("========================================");
                sb.AppendLine($"DateTime : {DateTime.Now:dd-MM-yyyy hh:mm tt}");
                sb.AppendLine($"Functionality Name : {request.FunctionalityName}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine(request.ErrorMessage);

                if (!string.IsNullOrWhiteSpace(request.StackTrace))
                {
                    sb.AppendLine("----------------------------------------");
                    sb.AppendLine("StackTrace:");
                    sb.AppendLine(request.StackTrace);
                }

                sb.AppendLine("========================================");
                sb.AppendLine();

                // =========================
                // Append Log
                // =========================
                await File.AppendAllTextAsync(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                // Optional fallback logging
                Console.WriteLine(ex.Message);
            }
        }
    }
}
