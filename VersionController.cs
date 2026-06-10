using EndAgent_API.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using TEST_WebApiOsDetails.data;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Dto;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_.ITAM_EA;

namespace TEST_WebApiOsDetails.Controllers
{
    // Dev: Viraj; Date: 06-04-2024
    [Route("api/[controller]/[action]")]
    public class VersionController : Controller
    {
        private readonly ApplicationDbContext db;
        //private readonly ITAMContext idb;
        private readonly ILogger<VersionController> logger;
        private readonly IConfiguration configuration;
        private string endAgentPath;
        private string endAgentFileName;
        private MongoDAL mongoDAL { get; set; }

        public VersionController(ApplicationDbContext _db, ILogger<VersionController> _logger, IConfiguration Configuration)
        {
            db = _db;
            logger = _logger;
            configuration = Configuration;
            endAgentPath = configuration.GetValue<string>("Paths:EndAgentInstall");
            endAgentFileName = configuration.GetValue<string>("Paths:EndAgentFileName");
            //MongoDB Database 
            string _mongoDBConnection = configuration.GetValue<string>("ConnectionStrings:mongoDBConnection");
            string _mongoDBDatabase = configuration.GetValue<string>("ConnectionStrings:mongoDBDatabase");
            //creating object for MongoDAL
            mongoDAL = new MongoDAL(_mongoDBConnection, _mongoDBDatabase);
        }


        [HttpGet]
        [Route("{orgid}/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Invalid File Name");
                }

                string filePath = "";

                if (fileName == "stable")
                {
                    fileName = fileName + ".zip";
                    filePath = Path.Combine(endAgentPath, fileName);
                }
                else if (fileName == "update")
                {
                    var files = Directory.GetFiles(endAgentPath, $"{endAgentFileName}*.zip")
                        .Select(Path.GetFileNameWithoutExtension)
                        .Where(name => name.StartsWith($"{endAgentFileName}"))
                        .ToList();

                    if (files.Any())
                    {
                        //var latestVersion = files.Select(name => new Version(name.Substring(endAgentFileName.Length))).Max();
                        var versions = files
                            .Select(name =>
                            {
                                var versionText = name.Substring(endAgentFileName.Length);

                                return Version.TryParse(versionText, out var version)
                                    ? version
                                    : null;
                            })
                            .Where(v => v != null);

                        Version? latestVersion = versions.Any()
                            ? versions.Max()
                            : null;
                        fileName = latestVersion != null ? $"{endAgentFileName}" + latestVersion.ToString() + ".zip" : $"{endAgentFileName}.zip";
                        filePath = Path.Combine(endAgentPath, fileName);
                    }
                    else
                    {
                        return NotFound("No update files found");
                    }
                }
                else
                {
                    return NotFound();
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetLatestVersion()
        {
            try
            {
                var files = Directory.GetFiles(endAgentPath, "EndAgent*.zip")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(name => name.StartsWith("EndAgent"))
                    .ToList();

                if (files.Any())
                {
                    var latestVersion = files.Select(name => new Version(name.Substring("EndAgent".Length) + ".0")).Max();
                    var latestFileName = "EndAgent" + latestVersion.ToString();
                    return Ok(latestFileName);
                }
                else
                {
                    return NotFound("No EndAgent files found");
                }
            }
            catch(Exception ex)
            {
                return NotFound("No EndAgent files found");
            }
        }

        [HttpGet]
        [Route("{objID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentVersion(string objID)
        {
            try
            {
                if (string.IsNullOrEmpty(objID))
                {
                    return BadRequest();
                }
                ITAMEADeviceDTO device = await mongoDAL.GetDeviceByAssetId(objID); // db.Devices.FirstOrDefault(d => d.ObjectID.ToString() == objID);
                if (device == null) { return NotFound("Device not found with te objID"); }
                PatchQueueDTO patchQueue = await mongoDAL.GetPatchQueueByDeviceId(device.ID);  //db.PatchQueue.FirstOrDefault(p => p.DeviceID == device.ID);
                if (patchQueue == null)
                    return Ok("none");

                return Ok(patchQueue.CurrentVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{DeviceID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentVersionWithDeviceID(string DeviceID)
        {
            try
            {
                if (string.IsNullOrEmpty(DeviceID))
                {
                    return BadRequest();
                }
                //Device? device = db.Devices.FirstOrDefault(d => d.Id.ToString() == DeviceID);
                ITAMEADeviceDTO device = await mongoDAL.GetDeviceByAssetId(DeviceID);
                if (device == null) { return NotFound("Device not found with te objID"); }
                //PatchQueue? patchQueue = db.PatchQueue.FirstOrDefault(p => p.DeviceID == device.ID);
                PatchQueueDTO patchQueue = await mongoDAL.GetPatchQueueByDeviceId(device.ID);
                if (patchQueue == null) { return Ok("none"); }
                return Ok(patchQueue.CurrentVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{DeviceID}")]
        [AllowAnonymous]
        public async Task<IActionResult> IsInQueueWithDeviceID(string DeviceID)
        {
            try
            {
                if (string.IsNullOrEmpty(DeviceID))
                {
                    return BadRequest();
                }
                //Device? device = db.Devices.FirstOrDefault(d => d.Id.ToString() == DeviceID);
                ITAMEADeviceDTO device = await mongoDAL.GetDeviceByAssetId(DeviceID);
                if (device == null) { return NotFound("Device not found with te objID"); }
                //PatchQueue? patchQueue = db.PatchQueue.FirstOrDefault(p => p.DeviceID == device.Id);
                PatchQueueDTO patchQueue = await mongoDAL.GetPatchQueueByDeviceId(device.ID);
                if (patchQueue == null) { return Ok("none"); }
                return Ok(patchQueue.InQueue);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{objID}")]
        [AllowAnonymous]
        public async Task<IActionResult> IsInQueue(string objID)
        {
            try
            {
                if (string.IsNullOrEmpty(objID))
                {
                    return BadRequest();
                }
                //Device? device = db.Devices.FirstOrDefault(d => d.ObjectID.ToString() == objID);
                ITAMEADeviceDTO device = await mongoDAL.GetDeviceByAssetId(objID);
                if (device == null) { return NotFound("Device not found with te objID"); }
                //PatchQueue? patchQueue = db.PatchQueue.FirstOrDefault(p => p.DeviceID == device.Id);
                PatchQueueDTO patchQueue = await mongoDAL.GetPatchQueueByDeviceId(device.ID);
                if (patchQueue == null) { return Ok("none"); }
                return Ok(patchQueue.InQueue);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest();
            }
        }


        [HttpPost]  
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> PostPatchQueue([FromBody] PatchQueueDTO value)
        {
            try
            {
                if (value == null) { return BadRequest("Invalid format input."); }
                //var device = await mongoDAL.GetDevice(value.ObjID, value.OrgId);
                //if (device == null)
                //{
                //    return NotFound("No device found.");
                //}

                var _result = await mongoDAL.PatchQueue(value, value.ObjID);
                if (!_result.Status)
                    return BadRequest();                

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error while posting in PostPatchQueue: {ex}");
                return BadRequest(ex);
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{ID}")]
        [AllowAnonymous]
        public async Task<IActionResult> AddDevicesInPatchQueue(string ID)
        {
            try
            {
                if (ID == null) { return BadRequest("Invalid format input."); }
                PatchQueue? patchQueue = db.PatchQueue.FirstOrDefault(p => p.DeviceID == Convert.ToInt32(ID));
                if (patchQueue == null)
                {
                    return NotFound();
                }
                else
                {
                    patchQueue.InQueue = true;
                    var files = Directory.GetFiles(endAgentPath, "EndAgent*.zip")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name.StartsWith("EndAgent"))
                .ToList();

                    if (files.Any())
                    {
                        var latestVersion = files.Select(name => new Version(name.Substring("EndAgent".Length))).Max();
                        var latestFileName = "EndAgent" + latestVersion.ToString();
                        patchQueue.InQueueVersion = latestFileName;
                    }
                }
                db.SaveChanges();   
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error while posting in AddDevicesInPatchQueue: {ex}");
                return BadRequest(ex);
            }

        }


    }
}
