using EndAgent_API.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace EndAgent_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrowserHistoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public BrowserHistoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [Route("createhistory")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddHistory()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync(); // Read form data
                var machineName = formCollection["MachineName"];
                var browserName = formCollection["BrowserName"];
                var files = Request.Form.Files;

                if (string.IsNullOrEmpty(machineName) || string.IsNullOrEmpty(browserName))
                {
                    return BadRequest("Machine Name and Browser Name are required.");
                }

                if (files.Count == 0)
                {
                    return BadRequest("No files were uploaded.");
                }

                // Define the base folder
                string baseDirectory = @"D:\BrowserHistory";
                string machineDirectory = Path.Combine(baseDirectory, machineName);
                string browserDirectory = Path.Combine(machineDirectory, browserName);

                // Ensure the MachineName folder exists
                if (!Directory.Exists(machineDirectory))
                {
                    Directory.CreateDirectory(machineDirectory);
                }

                // Ensure the BrowserName folder exists
                if (!Directory.Exists(browserDirectory))
                {
                    Directory.CreateDirectory(browserDirectory);
                }

                // Process each uploaded file
                foreach (var file in files)
                {
                    string filePath = Path.Combine(browserDirectory, file.FileName);

                    // If the file already exists, delete it
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Save the new file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                return Ok(new
                {
                    Message = "Files uploaded and stored successfully.",
                    MachineName = machineName,
                    BrowserName = browserName,
                    FileCount = files.Count,
                    StoragePath = browserDirectory
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult AddHistory([FromBody]BrowserHistyoryViewModel model)
        {
            return Ok();
        }

        [Route("gethistory")]
        [HttpGet]
        public IActionResult GetHistory()
        {
            return Ok("get history");

        }
    }
}
