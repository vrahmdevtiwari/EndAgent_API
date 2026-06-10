using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TEST_WebApiOsDetails.data;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Dto;
using TEST_WebApiOsDetails.ITAM;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_.ITAM_EA;
using TEST_WebApiOsDetails.Models.Notifications;
using EndAgent_API.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Process = TEST_WebApiOsDetails.Models.Process;
using TEST_WebApiOsDetails.Migrations;
using ResourceUtil = TEST_WebApiOsDetails.Models.ResourceUtil;
using UpdatePatchQueue = TEST_WebApiOsDetails.Models.UpdatePatchQueue;
using EndAgent_API.MongoDB;
using EndAgent_API.Models.Dto__Data_Tranfer_Objects_;
using MongoDB.Driver;
using EndAgent_API.Models;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;
using System;
using System.IO.Compression;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TEST_WebApiOsDetails.Controllers
{

    class ServiceConfiguration
    {
        public const string Services = "Services";
        public string time { get; set; }
        public string period { get; set; }
    }

    public static class Functions
    {
        public static void AppendLinesToFile(string filePath, string[] lines)
        {

            // Create a StreamWriter to append lines to the file
            using StreamWriter writer = File.AppendText(filePath);
            // Append each line to the file
            foreach (string line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }



    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize("ApiScope")]
    public class EndAgentController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        //private readonly ITAMContext idb;
        private readonly ILogger<EndAgentController> logger;
        private readonly IConfiguration configuration;
        private readonly DateTime startTime;
        private string baseUrl;
        private string assetApprovalsEndPoint;
        private string objectIdEndPoint;
        private string eaDevicesEndPoint;
        private string deviceTypeEndPoint;
        private string addEaDeviceEndPoint;
        private string addEaApprovalEndPoint;
        private string itamApiKeyHeader;
        private string itamApiKey;
        DataSet ds = new DataSet();
        private readonly IMongoCollection<AgentLogDto> _logs;
        private readonly PatchSyncService _patchSyncService;

        private MongoDAL mongoDAL { get; set; }

        public EndAgentController(ApplicationDbContext _db, ILogger<EndAgentController> _logger, IConfiguration Configuration, PatchSyncService patchSyncService)
        {
            db = _db;
            logger = _logger;
            configuration = Configuration;
            startTime = DateTime.Now;
            baseUrl = configuration.GetValue<string>("ApiEndPoints:baseurl");
            assetApprovalsEndPoint = configuration.GetValue<string>("ApiEndPoints:assetapprovalsurl");
            objectIdEndPoint = configuration.GetValue<string>("ApiEndPoints:objectidurl");
            eaDevicesEndPoint = configuration.GetValue<string>("ApiEndPoints:eadevicesurl");
            deviceTypeEndPoint = configuration.GetValue<string>("ApiEndPoints:devicetypeurl");
            addEaDeviceEndPoint = configuration.GetValue<string>("ApiEndPoints:addeadeviceurl");
            addEaApprovalEndPoint = configuration.GetValue<string>("ApiEndPoints:addeaapprovalurl");
            itamApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:itamApiKeyHeader");
            itamApiKey = configuration.GetValue<string>("ApiEndPoints:itamApiKey");
            //MongoDB Database 
            string _mongoDBConnection = configuration.GetValue<string>("ConnectionStrings:mongoDBConnection");
            string _mongoDBDatabase = configuration.GetValue<string>("ConnectionStrings:mongoDBDatabase");
            //creating object for MongoDAL
            mongoDAL = new MongoDAL(_mongoDBConnection, _mongoDBDatabase);
            var mongoClient = new MongoClient(_mongoDBConnection);
            var mongoDatabase = mongoClient.GetDatabase(_mongoDBDatabase);

            _logs = mongoDatabase.GetCollection<AgentLogDto>("agent_logs");
            _patchSyncService = patchSyncService;
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return Ok("Yes");
            //try
            //{
            //    var serviceConfig = new ServiceConfiguration();
            //    configuration.GetSection("Services").Bind(serviceConfig);
            //    //Console.WriteLine(serviceConfig.time);
            //    //Console.WriteLine(serviceConfig.period);
            //    var currentTime = DateTime.Now;
            //    var seconds = currentTime.Second;
            //    var minutes = currentTime.Minute;
            //    var hours = currentTime.Hour;

            //    char time = serviceConfig.time[0];
            //    if (int.TryParse(serviceConfig.period, out int period))
            //    {
            //        // Parsing successful, period contains the parsed int
            //    }
            //    else
            //    {
            //        return BadRequest("Error in Time Period");
            //    }

            //    if (period > 59)
            //    {
            //        period = 59;
            //    }

            //    if (time == 's' || time == 'S')
            //    {
            //        // Get the number of seconds passed since the start of the minute

            //        if (seconds % period <= 1)
            //        {
            //            return Ok("Yes");
            //        }
            //        // For the next 55 seconds, return "No"
            //        else
            //        {
            //            return Ok(startTime);
            //        }
            //    }
            //    else if (time == 'm' || time == 'M')
            //    {
            //        if (minutes % period == 0 && seconds <= 5)
            //        {
            //            return Ok("Yes");
            //        }
            //        // For the next 55 seconds, return "No"
            //        else
            //        {
            //            return Ok("No");
            //        }

            //    }
            //    else if (time == 'h' || time == 'H')
            //    {
            //        if (period > 23)
            //        {
            //            period = 23;
            //        }
            //        if (hours % period == 0 && seconds <= 5)
            //        {
            //            return Ok("Yes");
            //        }
            //        else
            //        {
            //            return Ok("No");
            //        }

            //    }

            //    return Ok("Wrong Input Parameters");
            //}
            //catch (Exception ex)
            //{
            //    logger.LogInformation(ex, "Code Error for EADetails");
            //    return BadRequest(ex.ToString());
            //}
        }

        // Get All EndAgent Details
        // USED BY ITAM
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public ActionResult<ITAMEASpecsDTO> GetSpecifications(string userid, string orgid, string? ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetSpecifications\n");

                if (ID != null)
                {
                    var _result = mongoDAL.GetDevicesInformation(ID, orgid).GetAwaiter().GetResult();
                    if (_result != null)
                    {
                        return Ok(_result);
                    }

                    return NotFound();

                    //var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    //if (obj == null)
                    //{
                    //    return NotFound("No Device Found");
                    //}
                    //var objectID = obj.ObjectID.ToString();
                    //var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    //if (asset != null)
                    //{
                    //    var location = db.Locations.FirstOrDefault(u => u.SystemId == asset.ID);
                    //    ITAMEASpecsDTO model = new ITAMEASpecsDTO()
                    //    {
                    //        SystemName = asset.SystemName,
                    //        SystemStatus = ((DateTime.Now - asset.CreatedAt).TotalMinutes < 5) ? asset.SystemStatus : "Offline",
                    //        IPv4Address = asset.IPv4Address,
                    //        IPv6Address = asset.IPv6Address,
                    //        SubnetMask = asset.SubnetMask,
                    //        Domain = asset.Domain,
                    //        Privileges = asset.Privileges,
                    //        OperatingSystem = asset.OperatingSystem,
                    //        LastActive = asset.LastActive,
                    //        LoginUser = asset.LoginUser,
                    //        Gateway = asset.Gateway,
                    //        NetworkAdapter = asset.NetworkAdapter,
                    //        Latitude = (location != null) ? $"{location.Latitude}" : "",
                    //        Longitude = (location != null) ? $"{location.Longitude}" : ""
                    //    };
                    //    return Ok(model);
                    //}
                    //return NotFound();
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, $"Code Error for EADetails for user: {userid}");
                return BadRequest(ex.ToString());
            }

        }

        // Post EndAgent Specs/Details
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EASpecifications([FromBody] EASpecificationDto value)
        {
            string device = null;
            string inAssetApproval = null;
            string inEADevice = null;
            string isApproved = null;
            string objectId = null;
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for GetEADetails" +
                "\n======================================================================\n");

                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();
                }
                // CheckinAssetApproval (true, false)
                int orgId = int.Parse(value.OrgID);
                string assetId = value.AssetID;
                string assetapprovalsurl = $"{baseUrl}{assetApprovalsEndPoint}/{orgId}/{assetId}";
                Validations validations = new Validations();
                inAssetApproval = await validations.GetDataInOneString(assetapprovalsurl, itamApiKeyHeader, itamApiKey);
                if (inAssetApproval.Contains("*error*"))
                {
                    return BadRequest(new { message = inAssetApproval });
                }
                if (inAssetApproval != "false")
                {
                    // CheckIsApproved (true, false)
                    if (inAssetApproval == "approved")
                    {
                        // Get Object ID
                        string objectidurl = $"{baseUrl}{objectIdEndPoint}/{orgId}/{assetId}";
                        objectId = await validations.GetDataInOneString(objectidurl, itamApiKeyHeader, itamApiKey);
                        if (objectId.Contains("*error*"))
                        {
                            return BadRequest(new { message = inAssetApproval });
                        }
                        // Check if in EADevices 
                        string eadevicesurl = $"{baseUrl}{eaDevicesEndPoint}/{orgId}/{assetId}";
                        inEADevice = await validations.GetDataInOneString(eadevicesurl, itamApiKeyHeader, itamApiKey);
                        if (inEADevice.Contains("*error*"))
                        {
                            return BadRequest(new { message = inAssetApproval });
                        }
                        // Add specs if already in EADevices
                        if (inEADevice == "true")
                        {
                            //getDeviceType
                            // Get device (type)
                            string devicetypeurl = $"{baseUrl}{deviceTypeEndPoint}/{orgId}/{assetId}";
                            device = await validations.GetDataInOneString(devicetypeurl, itamApiKeyHeader, itamApiKey);
                            if (device.Contains("*error*"))
                            {
                                return BadRequest(new { message = inAssetApproval });
                            }
                            //EndAgent Database
                            var existingModelList = await mongoDAL.GetEASpecifications(value.OrgID); // db.EASpecifications.FirstOrDefault(e => e.SystemName == value.SystemName && e.OrgID == value.OrgID);
                            var existingModel = existingModelList.Where(e => e.SystemName == value.SystemName).Select(x => new EASpecification()
                            {
                                SystemStatus = x.SystemStatus,
                                OperatingSystem = x.OperatingSystem,
                                OSVersion=x.OSVersion,
                                OSBuildVersion=x.OSBuildVersion,
                                LoginUser = x.LoginUser,
                                LastActive = x.LastActive,
                                Domain = x.Domain,
                                Privileges = x.Privileges,
                                NetworkAdapter = x.NetworkAdapter,
                                IPv4Address = x.IPv4Address,
                                IPv6Address = x.IPv6Address,
                                Gateway = x.Gateway,
                                SubnetMask = x.SubnetMask,
                                CreatedAt = x.CreatedAt,
                                AssetID = x.AssetID,
                                OrgID = x.OrgID,
                                ObjectID = x.ID
                            }).FirstOrDefault();

                            if (existingModel != null)
                            {
                                existingModel.SystemStatus = value.SystemStatus;
                                existingModel.OperatingSystem = value.OperatingSystem;
                                existingModel.OSVersion = value.OSVersion;
                                existingModel.OSBuildVersion = value.OSBuildVersion;
                                existingModel.LoginUser = value.LoginUser;
                                existingModel.LastActive = value.LastActive;
                                existingModel.Domain = value.Domain;
                                existingModel.Privileges = value.Privileges;
                                existingModel.NetworkAdapter = value.NetworkAdapter;
                                existingModel.IPv4Address = value.IPv4Address;
                                existingModel.IPv6Address = value.IPv6Address;
                                existingModel.Gateway = value.Gateway;
                                existingModel.SubnetMask = value.SubnetMask;
                                existingModel.CreatedAt = DateTime.Now;
                                existingModel.AssetID = value.AssetID;
                                existingModel.OrgID = value.OrgID;
                                existingModel.ObjectID = objectId;

                                //db.EASpecifications.Update(existingModel);
                                var _result = await mongoDAL.UpdateEASpecification(value.AssetID, value);
                                if (_result.Status)
                                    return Ok(_result.Message);
                            }
                            else
                            {
                                EASpecificationDto newModel = new EASpecificationDto
                                {
                                    SystemName = value.SystemName,
                                    SystemStatus = value.SystemStatus,
                                    OperatingSystem = value.OperatingSystem,
                                    OSVersion=value.OSVersion,
                                    OSBuildVersion=value.OSBuildVersion,
                                    LoginUser = value.LoginUser,
                                    LastActive = value.LastActive,
                                    Domain = value.Domain,
                                    Privileges = value.Privileges,
                                    NetworkAdapter = value.NetworkAdapter,
                                    IPv4Address = value.IPv4Address,
                                    IPv6Address = value.IPv6Address,
                                    Gateway = value.Gateway,
                                    SubnetMask = value.SubnetMask,
                                    AssetID = value.AssetID,
                                    OrgID = value.OrgID,
                                    ID = objectId,
                                    CreatedAt = DateTime.Now
                                };

                                var _result = await mongoDAL.AddEASpecification(value.AssetID, newModel);
                                if (_result.Status)
                                    return Ok(_result.Message);
                            }
                        }
                        // If not in EADevice, then must be added as it is Approved.
                        else
                        {
                            // Get device (type)
                            string devicetypeurl = $"{baseUrl}{deviceTypeEndPoint}/{orgId}/{assetId}";
                            device = await validations.GetDataInOneString(devicetypeurl, itamApiKeyHeader, itamApiKey);
                            if (device.Contains("*error*"))
                            {
                                return BadRequest(new { message = inAssetApproval });
                            }
                            // Adding to EADevice
                            string addeadeviceurl = $"{baseUrl}{addEaDeviceEndPoint}/{orgId}/{assetId}/{device}";
                            string status = await validations.GetDataInOneString(addeadeviceurl, itamApiKeyHeader, itamApiKey);
                            if (status.Contains("*error*"))
                            {
                                return BadRequest(new { message = inAssetApproval });
                            }
                            if (status != "added")
                            {
                                return BadRequest("Error in adding to Monitoring Devices");
                            }
                        }
                    }
                    else if (inAssetApproval == "unapproved")
                    {
                        return BadRequest("Validated, not Approved Yet");
                    }
                }
                // If not in AssetApproval Table
                else
                {
                    // If in ITAM CheckITAMDevices (laptop,cpu,mobiledevice,server,none)
                    string devicetypeurl = $"{baseUrl}{deviceTypeEndPoint}/{orgId}/{assetId}";
                    device = await validations.GetDataInOneString(devicetypeurl, itamApiKeyHeader, itamApiKey);
                    if (device.Contains("*error*"))
                    {
                        return BadRequest(new { message = inAssetApproval });
                    }

                    // If not in ITAM BadRequest
                    if (device == "none")
                    {
                        return BadRequest("ITAM Validation Error. Asset with assetId " + assetId + " not found.");
                    }
                    // if in ITAM, add in AssetApproval
                    else
                    {
                        string addeaapprovalurl = $"{baseUrl}{addEaApprovalEndPoint}/{orgId}/{assetId}/{device}/{value.SystemName}";
                        string status = await validations.GetDataInOneString(addeaapprovalurl, itamApiKeyHeader, itamApiKey);
                        if (status.Contains("*error*"))
                        {
                            return BadRequest(new { message = inAssetApproval });
                        }
                        if (status != "added")
                        {
                            return BadRequest("Error in adding " + assetId + " to Approvals");
                        }
                    }
                }
                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EADetails");
                return (IActionResult)ex;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{orgid}/{id}")]
        public async Task<IActionResult> UpdateLoginStatus(string orgid, string id)
        {
            var isUpdated = await mongoDAL.UpdateLastActiveInEASpecificationAsync(orgid, id);

            if (isUpdated == null)
                return StatusCode(500, "An error occurred while updating.");  // null = exception in DAL

            if (!isUpdated.Value)
                return BadRequest("No matching document found.");

            return Ok("Last active updated successfully.");
        }

        // Any Errors in Posting to EASpecification end point 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLogDto>>> GetErrorLogs()
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEADetailsErrors\n");
                var _errorList = await mongoDAL.GetErrorLogs();
                return Ok(_errorList);
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EADetails");
                return BadRequest(ex.ToString());
            }
        }

        // Posting Errors from EndAgent side
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EAErrorLog([FromBody] ErrorLogDto value)
        {
            try
            {
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();
                }

                var _result = await mongoDAL.AddErrorLogs(value);
                if (_result.Status)
                {
                    logger.LogInformation(_result.Message);
                    return Ok(_result.Message);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAErrorLogs");
                return (IActionResult)ex;
            }
        }

        // Get Installed App list
        // USED BY ITAM
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEAInstalledApps(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAInstalledApps\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var apps = await db.Apps.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAAppsDTO> result = new List<ITAMEAAppsDTO>();
                    foreach (InstalledApp app in apps)
                    {
                        var model = new ITAMEAAppsDTO()
                        {
                            AppName = (app.AppName != "") ? app.AppName : "-",
                            Provider = (app.Provider != "") ? app.Provider : "-",
                            InstalledOn = (app.InstalledOn != "") ? app.InstalledOn : "-",
                            Size = (app.Size != "") ? app.Size : "-",
                            Version = (app.Version != "") ? app.Version : "-",
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EADetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }


        // Post Installed App list
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAInstalledApps([FromBody] List<InstalledAppDto> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EAInstalledApps" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var app in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == app.AssetId && e.OrgID == app.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM Apps WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var app in value)
                {
                    var id = db.EASpecifications.FirstOrDefault(e => e.AssetID == app.AssetId && e.OrgID == app.OrgId);
                    InstalledApp model = new()
                    {
                        SystemId = id.ID,
                        AppName = app.AppName,
                        Provider = app.Provider,
                        InstalledOn = app.InstalledOn,
                        Size = app.Size,
                        Version = app.Version,
                        CreatedAt = DateTime.Now
                    };
                    await db.Apps.AddAsync(model);
                }
                await db.SaveChangesAsync();


                logger.LogInformation("APPS SUCCESS");
                return Ok("Apps Success");
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAInstalledApps");
                return (IActionResult)ex;
            }
        }

        // USED BY ITAM
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEAUpdates(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAInstalledUpdates\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var updates = await db.Updates.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAUpdatesDTO> result = new List<ITAMEAUpdatesDTO>();
                    foreach (Update update in updates)
                    {
                        var model = new ITAMEAUpdatesDTO()
                        {
                            Description = update.Description,
                            Patch = (update.Patch != "") ? update.Patch : "-",
                            InstalledOn = update.InstalledOn,
                            Title = update.Title,
                            Version = update.Version,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EADetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAUpdates([FromBody] List<UpdateDto> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                " GET request triggered for EAUpdate" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();
                }


                foreach (var update in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == update.AssetId && e.OrgID == update.OrgId);
                    if (id == null)
                    {
                        return BadRequest("id not found");
                    }
                    var query = from u in db.Updates
                                where u.Title == update.Title && u.SystemId == id.ID
                                select u;
                    var temp = await query.FirstOrDefaultAsync();

                    if (temp is null)
                    {
                        Update model = new()
                        {
                            SystemId = id.ID,
                            Title = update.Title,
                            Patch = update.Patch,
                            Description = update.Description,
                            InstalledOn = update.InstalledOn,
                            Version = update.Version,
                            CreatedOn = DateTime.Now
                        };
                        await db.Updates.AddAsync(model);
                    }
                }
                await db.SaveChangesAsync();
                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error in EAUpdate");
                return (IActionResult)ex;
            }
        }

        // USED BY ITAM
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEAPorts(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAPorts\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var ports = await db.Ports.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAPortsDTO> result = new List<ITAMEAPortsDTO>();
                    foreach (Port port in ports)
                    {
                        var model = new ITAMEAPortsDTO()
                        {
                            PortNumber = port.PortNumber,
                            ProcessId = port.ProcessId,
                            ProcessName = port.ProcessName,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EADetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        // Post Installed App list
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAPortDetails([FromBody] List<PortDto> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for PortDetails" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var app in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == app.AssetId && e.OrgID == app.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.Ports.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM Ports WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var port in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == port.AssetId && e.OrgID == port.OrgId);
                    Port model = new()
                    {
                        SystemId = id.ID,
                        PortNumber = port.PortNumber,
                        ProcessId = port.ProcessId,
                        ProcessName = port.ProcessName,
                        CreatedAt = DateTime.Now
                    };
                    await db.Ports.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for PortDetails");
                return (IActionResult)ex;
            }
        }

        // USED BY ITAM
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetAssetStatus(string userid, string orgid, string ID)
        {
            try
            {
                if (ID == null)
                {
                    return BadRequest(new { message = "ID was null" });
                }
                var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                if (obj == null)
                {
                    return NotFound("No Device Found");
                }
                var objectID = obj.ObjectID.ToString();
                var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                if (asset == null) { return NotFound(new { message = "Asset Not Found" }); }
                TimeSpan difference = DateTime.Now - asset.CreatedAt;
                if (difference.TotalMinutes < 5)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetAssetStatus for user: " + userid);
                return BadRequest(new { message = ex.ToString() });
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EAGeoLocation([FromBody] LocationDTO location)
        {
            try
            {
                logger.LogInformation("EAGeoLocation Post request generated.");
                if (location is null || location.Longitude == "-" || location.Latitude == "-")
                {
                    //return BadRequest(new { message = "The received coordinates were null or invalid." });
                    return BadRequest(new { message = "" });
                }
                var asset = await db.EASpecifications.FirstOrDefaultAsync(a => a.AssetID == location.AssetId && a.OrgID == location.OrgId);
                if (asset == null)
                {
                    return NotFound(new { message = "There is no asset with the system name" });
                }
                int assetId = asset.ID;
                var existingModel = db.Locations.FirstOrDefault(e => e.SystemId == assetId);
                if (existingModel != null)
                {
                    existingModel.Longitude = location.Longitude;
                    existingModel.Latitude = location.Latitude;
                    existingModel.CreatedAt = DateTime.Now;
                    db.Locations.Update(existingModel);
                    await db.SaveChangesAsync();
                    return Ok(new { message = "Updated coordinates of the device." });
                }
                Location model = new Location()
                {
                    Longitude = location.Longitude,
                    Latitude = location.Latitude,
                    SystemId = assetId,
                    CreatedAt = DateTime.Now
                };
                db.Locations.Add(model);
                await db.SaveChangesAsync();
                return Ok(new { message = "Coordinates posted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogInformation("EAGeoLocation Post request error. " + ex.ToString());
                return BadRequest(new { message = "Error" + ex.ToString() });
            }
        }


        // USED BY Moniotring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{userid}/{orgid}")]
        public async Task<IActionResult> GetBLsoftwares(string userid, string orgid)
        {
            try
            {
                if (orgid == null)
                {
                    return BadRequest(new { message = "orgid was null" });
                }
                var apps = db.Apps.Where(app => app.EASpecification.OrgID == orgid).ToList();
                var blacklistedSoftwareNames = db.BlackListedSoftwares.Select(bl => bl.Name).ToList();

                // Create a new list of BLsoftware containing apps from the blacklist
                IEnumerable<BLsoftwareListDTO> blApps = apps
                    .Where(app => blacklistedSoftwareNames.Contains(app.AppName))
                    .Select(app => new BLsoftwareListDTO
                    {
                        Name = app.AppName,
                        AssetId = db.EASpecifications.FirstOrDefault(u => u.ID == app.SystemId).AssetID,
                    })
                    .ToList();
                List<BLsoftwareListDTO> bl = new List<BLsoftwareListDTO>();
                foreach (BLsoftwareListDTO blApp in blApps)
                {
                    bl.Add(blApp);
                }

                if (bl == null) { return NotFound(new { message = "No blacklisted softwares found." }); }
                return Ok(bl);
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetAssetStatus for user: " + userid);
                return BadRequest(new { message = ex.ToString() });
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{orgId}/{user}/{ID}")]
        public async Task<IActionResult> ActiveStatus(string orgId, string user, string ID)
        {
            try
            {
                if (orgId == null)
                {
                    logger.LogInformation("Code Error for GetAssetStatus, orgid was null for user: " + user);
                    return BadRequest(new { message = "orgid was null" });
                }
                DateTime lastActive = new DateTime();
                //var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgId);
                //if (obj == null)
                //{
                //    return NotFound("No Device Found");
                //}
                //string objectID = obj.ObjectID.ToString() ?? string.Empty;
                //var x = await db.EASpecifications.FirstOrDefaultAsync(u => u.ObjectID == objectID && u.OrgID == orgId);
                //if (x == null)
                //{
                //    logger.LogInformation("Code Error for GetAssetStatus, lastactive was null for user: " + user);
                //    return BadRequest(new { message = "lastactive was null" });

                //}
                var x = await mongoDAL.GetEASpecificationByAssetId(ID, orgId);
                lastActive = x.CreatedAt;
                TimeSpan timeDifference = DateTime.Now - lastActive;

                // Check if the time difference is more than 5 minutes
                bool isOffline = timeDifference.TotalMinutes > 5;

                // Return "Online" or "Offline" based on the result
                string status = isOffline ? "Offline" : "Online";
                ActiveStatusDTO activeStatus = new ActiveStatusDTO();
                activeStatus.Status = status;
                return Ok(activeStatus);
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetAssetStatus for user: " + user);

                return BadRequest(new { message = ex.ToString() });
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetServiceDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAServices\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var services = await db.Services.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAServicesDTO> result = new List<ITAMEAServicesDTO>();
                    foreach (Service service in services)
                    {
                        var model = new ITAMEAServicesDTO()
                        {
                            ServiceName = service.ServiceName,
                            ServiceType = service.ServiceType,
                            ServiceStatus = service.ServiceStatus,
                            StartType = service.StartType,
                            DisplayName = service.DisplayName,
                            PID = service.PID
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAServiceDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAServiceDetails([FromBody] List<ServiceDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for ServiceDetails" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var service in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == service.AssetId && e.OrgID == service.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.Ports.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM Services WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var service in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == service.AssetId && e.OrgID == service.OrgId);
                    Service model = new()
                    {
                        SystemId = id.ID,
                        ServiceName = service.ServiceName,
                        DisplayName = service.DisplayName,
                        ServiceStatus = service.ServiceStatus,
                        ServiceType = service.ServiceType,
                        StartType = service.StartType,
                        PID = service.PID,
                        CreatedAt = DateTime.Now
                    };
                    await db.Services.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for ServiceDetails");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetProcessDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAProcesses\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var processes = await db.Processes.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAProcesses> result = new List<ITAMEAProcesses>();
                    foreach (Process process in processes)
                    {
                        var model = new ITAMEAProcesses()
                        {
                            ProcessName = process.ProcessName,
                            ProcessId = process.ProcessId,
                            Description = process.Description,
                            Status = process.Status,
                            MemoryUsage = process.MemoryUsage,
                            Username = process.Username,
                            Path = process.Path,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAProcessDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAProcessDetails([FromBody] List<ProcessDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for ProcessDetails" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var process in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == process.AssetId && e.OrgID == process.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.Processes.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM Processes WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var process in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == process.AssetId && e.OrgID == process.OrgId);
                    Process model = new()
                    {
                        SystemId = id.ID,
                        ProcessId = process.ProcessId,
                        ProcessName = process.ProcessName,
                        Description = process.Description,
                        Status = process.Status,
                        Username = process.Username,
                        MemoryUsage = process.MemoryUsage,
                        Path = process.Path,
                        CreatedAt = DateTime.Now
                    };
                    await db.Processes.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for ProcessDetails");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetProcessorDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEAProcessors\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var processors = await db.Processors.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAProcessorDTO> result = new List<ITAMEAProcessorDTO>();
                    foreach (Processor processor in processors)
                    {
                        var model = new ITAMEAProcessorDTO()
                        {
                            DeviceID = processor.DeviceID,
                            Name = processor.Name,
                            Manufacturer = processor.Manufacturer,
                            MaxClockSpeed = processor.MaxClockSpeed,
                            Cores = processor.Cores,
                            LogicalProcessors = processor.LogicalProcessors,
                            ProcessorId = processor.ProcessorId,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAProcessorDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAProcessorDetails([FromBody] List<ProcessorDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for ProcessorDetails" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var processor in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == processor.AssetId && e.OrgID == processor.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.Processors.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM Processors WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var processor in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == processor.AssetId && e.OrgID == processor.OrgId);
                    Processor model = new()
                    {
                        SystemId = id.ID,
                        DeviceID = processor.DeviceID,
                        Name = processor.Name,
                        Manufacturer = processor.Manufacturer,
                        MaxClockSpeed = processor.MaxClockSpeed,
                        Cores = processor.Cores,
                        LogicalProcessors = processor.LogicalProcessors,
                        ProcessorId = processor.ProcessorId,
                        CreatedAt = DateTime.Now
                    };
                    await db.Processors.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for ProcessorDetails");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetRAMDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GETRAMDetails\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var rAMs = await db.RAMDetails.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEARAMDetailsDTO> result = new List<ITAMEARAMDetailsDTO>();
                    foreach (RAMDetail rAM in rAMs)
                    {
                        var model = new ITAMEARAMDetailsDTO()
                        {
                            BankLabel = rAM.BankLabel,
                            Capacity = rAM.Capacity,
                            Description = rAM.Description,
                            Manufacturer = rAM.Manufacturer,
                            MemoryType = rAM.MemoryType,
                            PartNumber = rAM.PartNumber,
                            SerialNumber = rAM.SerialNumber,
                            Speed = rAM.Speed,
                            SMBIOSMemoryType = rAM.SMBIOSMemoryType,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAProcessorDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EARAMDetails([FromBody] List<RAMDetailDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for RAMDetails" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var rAM in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == rAM.AssetId && e.OrgID == rAM.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.RAMDetails.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM RAMDetails WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var rAM in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == rAM.AssetId && e.OrgID == rAM.OrgId);
                    RAMDetail model = new()
                    {
                        SystemId = id.ID,
                        BankLabel = rAM.BankLabel,
                        Capacity = rAM.Capacity,
                        Description = rAM.Description,
                        Manufacturer = rAM.Manufacturer,
                        MemoryType = rAM.MemoryType,
                        PartNumber = rAM.PartNumber,
                        SerialNumber = rAM.SerialNumber,
                        Speed = rAM.Speed,
                        SMBIOSMemoryType = rAM.SMBIOSMemoryType,
                        CreatedAt = DateTime.Now
                    };
                    await db.RAMDetails.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for RAMDetails");
                return (IActionResult)ex;
            }
        }


        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetScheduledTasks(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GET Scheduledtasks\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var sTs = await db.ScheduledTasks.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAScheduledTasksDTO> result = new List<ITAMEAScheduledTasksDTO>();
                    foreach (ScheduledTask sT in sTs)
                    {
                        var model = new ITAMEAScheduledTasksDTO()
                        {
                            Status = sT.Status,
                            Author = sT.Author,
                            LastRunResult = sT.LastRunResult,
                            LastRunTime = sT.LastRunTime,
                            Name = sT.Name,
                            NextRunTime = sT.NextRunTime,
                            Path = sT.Path,
                            Trigger = sT.Trigger,
                            CreatedDate = sT.CreatedDate
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EA Scheduled Tasks for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAScheduledTasks([FromBody] List<ScheduledTaskDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for ScheduledTasks" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var sT in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == sT.AssetId && e.OrgID == sT.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.ScheduledTasks.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM ScheduledTasks WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var sT in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == sT.AssetId && e.OrgID == sT.OrgId);
                    ScheduledTask model = new()
                    {
                        SystemId = id.ID,
                        Name = sT.Name,
                        Status = sT.Status,
                        Author = sT.Author,
                        Path = sT.Path,
                        Trigger = sT.Trigger,
                        LastRunResult = sT.LastRunResult,
                        LastRunTime = sT.LastRunTime,
                        NextRunTime = sT.NextRunTime,
                        CreatedDate = sT.CreatedDate,
                        CreatedAt = DateTime.Now
                    };
                    await db.ScheduledTasks.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAScheduledTasks");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetStorageVolumes(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for Get StorageVolumes\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var sVs = await db.StorageVolumes.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAStorageVolumeDTO> result = new List<ITAMEAStorageVolumeDTO>();
                    foreach (StorageVolume sV in sVs)
                    {
                        var model = new ITAMEAStorageVolumeDTO()
                        {
                            BootVolume = sV.BootVolume,
                            Capacity = sV.Capacity,
                            SystemVolume = sV.SystemVolume,
                            FileSystem = sV.FileSystem,
                            FreeSpace = sV.FreeSpace,
                            DriveLetter = sV.DriveLetter,
                            Label = sV.Label,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EA Storage Volumes for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAStorageVolumes([FromBody] List<StorageVolumeDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EAStorageVolumes" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var sV in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == sV.AssetId && e.OrgID == sV.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var portsToDelete = db.StorageVolumes.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM StorageVolumes WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var sV in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == sV.AssetId && e.OrgID == sV.OrgId);
                    StorageVolume model = new()
                    {
                        SystemId = id.ID,
                        BootVolume = sV.BootVolume,
                        Capacity = sV.Capacity,
                        DriveLetter = sV.DriveLetter,
                        FileSystem = sV.FileSystem,
                        FreeSpace = sV.FreeSpace,
                        Label = sV.Label,
                        SystemVolume = sV.SystemVolume,
                        CreatedAt = DateTime.Now
                    };
                    await db.StorageVolumes.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAStorageVolumes");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetGraphicCards(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetGraphicCards\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var details = await db.GraphicCards.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAGraphicCardsDTO> result = new List<ITAMEAGraphicCardsDTO>();
                    foreach (GraphicCard detail in details)
                    {
                        var model = new ITAMEAGraphicCardsDTO()
                        {
                            AdapterCompatibility = detail.AdapterCompatibility,
                            AdapterRAM = detail.AdapterRAM,
                            Caption = detail.Caption,
                            DeviceID = detail.DeviceID,
                            VideoProcessor = detail.VideoProcessor,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetGraphicCards for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAGraphicCards([FromBody] List<GraphicCardDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EAGraphicCards" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var toDelete = db.GraphicCards.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM GraphicCards WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    GraphicCard model = new()
                    {
                        SystemId = id.ID,
                        AdapterCompatibility = x.AdapterCompatibility,
                        AdapterRAM = x.AdapterRAM,
                        Caption = x.Caption,
                        DeviceID = x.DeviceID,
                        VideoProcessor = x.VideoProcessor,
                        CreatedAt = DateTime.Now
                    };
                    await db.GraphicCards.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAGraphicCards");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetRAIDControllers(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetRAIDControllers\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var details = await db.RAIDControllers.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEARAIDControllersDTO> result = new List<ITAMEARAIDControllersDTO>();
                    foreach (RAIDController detail in details)
                    {
                        var model = new ITAMEARAIDControllersDTO()
                        {
                            Caption = detail.Caption,
                            Name = detail.Name,
                            ConfigManagerErrorCode = detail.ConfigManagerErrorCode,
                            ConfigManagerUserConfig = detail.ConfigManagerUserConfig,
                            Status = detail.Status,
                            SystemCreationClassName = detail.SystemCreationClassName,
                            SystemName = detail.SystemName,
                            Description = detail.Description,
                            Manufacturer = detail.Manufacturer,
                            PNPDeviceID = detail.PNPDeviceID,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetRAIDControllers for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EARAIDControllers([FromBody] List<RAIDControllerDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EARAIDControllers" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var toDelete = db.RAIDControllers.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM RAIDControllers WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    RAIDController model = new()
                    {
                        SystemId = id.ID,
                        Name = x.Name,
                        Status = x.Status,
                        PNPDeviceID = x.PNPDeviceID,
                        Caption = x.Caption,
                        SystemCreationClassName = x.SystemCreationClassName,
                        Description = x.Description,
                        Manufacturer = x.Manufacturer,
                        SystemName = x.SystemName,
                        ConfigManagerErrorCode = x.ConfigManagerErrorCode,
                        ConfigManagerUserConfig = x.ConfigManagerUserConfig,
                        CreatedAt = DateTime.Now
                    };
                    await db.RAIDControllers.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EARAIDControllers");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetNetworkAdapters(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetNetworkAdapters\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var details = await db.NetworkAdapters.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEANetworkAdaptersDTO> result = new List<ITAMEANetworkAdaptersDTO>();
                    foreach (NetworkAdapter detail in details)
                    {
                        var model = new ITAMEANetworkAdaptersDTO()
                        {
                            Name = detail.Name,
                            Status = detail.Status,
                            Speed = detail.Speed,
                            Description = detail.Description,
                            InterfaceIndex = detail.InterfaceIndex,
                            MACAddress = detail.MACAddress,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetNetworkAdapters for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EANetworkAdapters([FromBody] List<NetworkAdapterDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EANetworkAdapters" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var toDelete = db.NetworkAdapters.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM NetworkAdapters WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    NetworkAdapter model = new()
                    {
                        SystemId = id.ID,
                        Name = x.Name,
                        Status = x.Status,
                        Description = x.Description,
                        InterfaceIndex = x.InterfaceIndex,
                        MACAddress = x.MACAddress,
                        Speed = x.Speed,
                        CreatedAt = DateTime.Now
                    };
                    await db.NetworkAdapters.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EANetworkAdapters");
                return (IActionResult)ex;
            }
        }

        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetPhysicalDrives(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetPhysicalDrives\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var details = await db.PhysicalDrives.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAPhysicalDrivesDTO> result = new List<ITAMEAPhysicalDrivesDTO>();
                    foreach (PhysicalDrive detail in details)
                    {
                        var model = new ITAMEAPhysicalDrivesDTO()
                        {
                            DeviceID = detail.DeviceID,
                            FirmwareRevision = detail.FirmwareRevision,
                            Index = detail.Index,
                            InterfaceType = detail.InterfaceType,
                            MediaType = detail.MediaType,
                            Model = detail.Model,
                            Partitions = detail.Partitions,
                            SerialNumber = detail.SerialNumber,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetPhysicalDrives for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAPhysicalDrives([FromBody] List<PhysicalDriveDTO> value)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EAPhysicalDrives" +
                "\n======================================================================\n");
                if (value is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting
                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    if (id is null)
                    {
                        return BadRequest("id not found");
                    }
                    var toDelete = db.PhysicalDrives.Where(a => a.SystemId == id.ID);
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM PhysicalDrives WHERE SystemId = {id.ID}");
                    break;
                }


                foreach (var x in value)
                {
                    var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                    PhysicalDrive model = new()
                    {
                        SystemId = id.ID,
                        DeviceID = x.DeviceID,
                        FirmwareRevision = x.FirmwareRevision,
                        Index = x.Index,
                        InterfaceType = x.InterfaceType,
                        MediaType = x.MediaType,
                        Model = x.Model,
                        Partitions = x.Partitions,
                        SerialNumber = x.SerialNumber,
                        CreatedAt = DateTime.Now
                    };
                    await db.PhysicalDrives.AddAsync(model);
                }
                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAPhysicalDrives");
                return (IActionResult)ex;
            }
        }


        // USED BY Monitoring
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetOtherSpecifications(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetOtherSpecifications\n");

                if (ID != null)
                {
                    //var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);                    
                    //if (obj == null)
                    //{
                    //    return NotFound("No Device Found");
                    //}
                    //var objectID = obj.ObjectID.ToString();
                    //var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);

                    //if (asset == null)
                    //{
                    //    return NotFound();
                    //}
                    //var systemId = asset.ID;
                    //var details = await db.OtherSpecifications.Where(u => u.SystemId == systemId).ToListAsync();

                    var _device = mongoDAL.GetDevicesInformation(ID, orgid);
                    var detail = _device.Result.OtherSpecifications;

                    List<ITAMEAOtherSpecificationsDTO> result = new List<ITAMEAOtherSpecificationsDTO>();
                    if (detail != null)
                    {
                        var model = new ITAMEAOtherSpecificationsDTO()
                        {
                            Antivirus = detail.Antivirus,
                            MACAAddress = detail.MACAAddress,
                            BIOSVersion = detail.BIOSVersion,
                            CPUName = detail.CPUName,
                            InstalledRAM = detail.InstalledRAM,
                            OSVersion = detail.OSVersion,
                            SystemManufacturer = detail.SystemManufacturer,
                            SystemModel = detail.SystemModel,
                            SystemUptime = detail.SystemUptime,
                            SerialNumber = detail.SerialNumber,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetOtherSpecifications for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EAOtherSpecifications([FromBody] OtherSpecificationDTO x)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for EAOtherSpecifications" +
                "\n======================================================================\n");
                if (x is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest();

                }
                // To update Installed Apps instead of Inserting

                var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                if (id is null)
                {
                    return BadRequest("id not found");
                }
                var toDelete = db.OtherSpecifications.Where(a => a.SystemId == id.ID);
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM OtherSpecifications WHERE SystemId = {id.ID}");

                OtherSpecification model = new()
                {
                    SystemId = id.ID,
                    Antivirus = x.Antivirus,
                    MACAAddress = x.MACAAddress,
                    InstalledRAM = x.InstalledRAM,
                    BIOSVersion = x.BIOSVersion,
                    CPUName = x.CPUName,
                    SystemUptime = x.SystemUptime,
                    SystemModel = x.SystemModel,
                    SystemManufacturer = x.SystemManufacturer,
                    OSVersion = x.OSVersion,
                    SerialNumber = x.SerialNumber,
                    CreatedAt = DateTime.Now
                };
                await db.OtherSpecifications.AddAsync(model);

                //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                await db.SaveChangesAsync();


                logger.LogInformation("SUCCESS");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for EAOtherSpecifications");
                return (IActionResult)ex;
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEA_Accounts(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEA_Accounts\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var accounts = await db.Accounts.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAAccountsDTO> result = new List<ITAMEAAccountsDTO>();
                    foreach (Account account in accounts)
                    {
                        var model = new ITAMEAAccountsDTO()
                        {
                            Name = account.Name,
                            AccountType = account.AccountType,
                            Caption = account.Caption,
                            Domain = account.Domain,
                            SID = account.SID,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetEA_Accounts for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EA_Accounts([FromBody] List<AccountDTO> value17)
        {
            try
            {
                logger.LogInformation("\n======================================================================\n" +
                "POST request triggered for Accounts" +
                "\n======================================================================\n");

                if (value17 is null)
                {
                    logger.LogInformation("Bad Request");
                    return BadRequest($"\n Error Accounts details: Bad Request");


                }
                else
                {
                    // To update Installed Apps instead of Inserting
                    foreach (var x in value17)
                    {
                        var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                        if (id is null)
                        {
                            return NotFound($"\n Error Accounts details: id not found");

                        }
                        else
                        {
                            var toDelete = db.Accounts.Where(a => a.SystemId == id.ID);
                            await db.Database.ExecuteSqlRawAsync($"DELETE FROM Accounts WHERE SystemId = {id.ID}");
                            break;
                        }
                    }


                    foreach (var x in value17)
                    {
                        var id = await db.EASpecifications.FirstOrDefaultAsync(e => e.AssetID == x.AssetId && e.OrgID == x.OrgId);
                        if (id != null)
                        {
                            Account model = new()
                            {
                                SystemId = id.ID,
                                Name = x.Name,
                                AccountType = x.AccountType,
                                Caption = x.Caption,
                                Domain = x.Domain,
                                SID = x.SID,
                                CreatedAt = DateTime.Now
                            };
                            await db.Accounts.AddAsync(model);
                        }
                    }
                    //db.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('Ports', RESEED)");
                    await db.SaveChangesAsync();


                    logger.LogInformation("SUCCESS");
                    return Ok("SUCCESS");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for Accounts");
                return BadRequest($"\n Error Accounts details: {ex}");

            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEA_ActivePorts(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEA_ActivePorts\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var activePorts = await db.ActivePorts.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAActivePortsDTO> result = new List<ITAMEAActivePortsDTO>();
                    foreach (ActivePort activePort in activePorts)
                    {
                        var model = new ITAMEAActivePortsDTO()
                        {
                            PID = activePort.PID,
                            Proto = activePort.Proto,
                            State = activePort.State,
                            ForeignAddress = activePort.ForeignAddress,
                            LocalAddress = activePort.LocalAddress
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetEA_ActivePorts for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEA_ActiveNetworkDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEA_ActiveNetworkDetails\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var activeNetworks = await db.ActiveNetworkDetails.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAActiveNetworkDetailDTO> result = new List<ITAMEAActiveNetworkDetailDTO>();
                    foreach (ActiveNetworkDetail activeNetwork in activeNetworks)
                    {
                        var model = new ITAMEAActiveNetworkDetailDTO()
                        {
                            MacAddress = activeNetwork.MacAddress,
                            IpAddress = activeNetwork.IpAddress,
                            Description = activeNetwork.Description,
                            DefaultGateway = activeNetwork.DefaultGateway,
                            DhcpEnabled = activeNetwork.DhcpEnabled,
                            DnsServers = activeNetwork.DnsServers,
                            SubnetMask = activeNetwork.SubnetMask,
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetEA_ActiveNetworkDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetResourceUtil(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetResourceUtil\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var details = await db.ResourceUtils.Where(u => u.SystemId == systemId).ToListAsync();
                    List<ITAMEAResourceUtilDTO> result = new List<ITAMEAResourceUtilDTO>();
                    if (details != null)
                    {
                        foreach (var detail in details)
                        {
                            var model = new ITAMEAResourceUtilDTO()
                            {
                                BytesReceived = detail.BytesReceived,
                                BytesSent = detail.BytesSent,
                                CPUUsage = detail.CPUUsage,
                                PhysicalDiskUsage = detail.PhysicalDiskUsage,
                                MemoryUsage = detail.MemoryUsage,
                                GPUUsage = detail.GPUUsage
                            };
                            result.Add(model);
                        }
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetResourceUtil for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{ID}")]
        public async Task<IActionResult> GetEA_DiskDetails(string userid, string orgid, string ID = null)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetEA_DiskDetails\n");

                if (ID != null)
                {
                    var obj = db.Devices.FirstOrDefault(d => d.Id.ToString() == ID && d.OrgID.ToString() == orgid);
                    if (obj == null)
                    {
                        return NotFound("No Device Found");
                    }
                    var objectID = obj.ObjectID.ToString();
                    var asset = db.EASpecifications.FirstOrDefault(u => u.ObjectID == objectID && u.OrgID == orgid);
                    if (asset == null)
                    {
                        return NotFound();
                    }
                    var systemId = asset.ID;
                    var diskDetails = await db.DiskDetails.Where(u => u.SystemId == systemId).Include(u => u.PartitionDetails).ToListAsync();
                    List<ITAMEADiskDetailDTO> result = new List<ITAMEADiskDetailDTO>();
                    foreach (DiskDetail diskDetail in diskDetails)
                    {
                        var model = new ITAMEADiskDetailDTO()
                        {
                            Capacity = diskDetail.Capacity,
                            DeviceID = diskDetail.DeviceID,
                            Index = diskDetail.Index,
                            Model = diskDetail.Model,
                            Manufacturer = diskDetail.Manufacturer,
                            MediaType = diskDetail.MediaType,
                            SerialNumber = diskDetail.SerialNumber,
                            FirmwareRevision = diskDetail.FirmwareRevision,
                            Partitions = diskDetail.Partitions,
                            InterfaceType = diskDetail.InterfaceType,
                            Status = diskDetail.Status,
                            InstallDate = diskDetail.InstallDate,
                            PartitionDetails = diskDetail.PartitionDetails.Select(partition => new PartitionDetailDTO
                            {
                                DeviceID = partition.DeviceID,
                                Index = partition.Index,
                                DiskIndex = partition.DiskIndex,
                                Bootable = partition.Bootable,
                                BootPartition = partition.BootPartition,
                                PrimaryPartition = partition.PrimaryPartition,
                                Size = partition.Size,
                                State = partition.State,
                                DriveLetter = partition.DriveLetter,
                                FileSystem = partition.FileSystem,
                                FreeSpace = partition.FreeSpace,
                                UsedSpace = partition.UsedSpace,
                                Description = partition.Description,
                                VolumeName = partition.VolumeName,
                            }).ToList(),
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetEA_DiskDetails for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}")]
        public async Task<IActionResult> GetNotifications(string userid, string orgid)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetResourceUtil\n");

                if (orgid != null)
                {
                    var notifications = await mongoDAL.GetNotificationsByOrgId(orgid); //await db.Notifications.Where(u => u.OrgID == orgid).ToListAsync();
                    if (notifications == null || notifications.Count() == 0)
                    {
                        return NotFound();
                    }
                    List<ITAMEANotificationDTO> result = new List<ITAMEANotificationDTO>();
                    foreach (var notification in notifications)
                    {
                        if (notification.DevType == "CPU")
                        {
                            notification.DevType = "Desktop";
                        }
                        var model = new ITAMEANotificationDTO()
                        {
                            CreatedAt = notification.CreatedAt,
                            Body = notification.Body ?? "-",
                            Header = notification.Header ?? "-",
                            IsRead = notification.IsRead,
                            Message = notification.Message ?? "-",
                            User = notification.User ?? "-",
                            AssetID = notification.AssetID,
                            OrgID = notification.OrgID,
                            DevType = notification.DevType,
                            Id = notification.Id
                        };
                        result.Add(model);
                    }
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for GetResourceUtil for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{id}")]
        public async Task<IActionResult> IsReadNotification(string userid, string orgid, int Id)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetResourceUtil\n");

                if (orgid != null)
                {
                    var _result = await mongoDAL.NotificationStatusUpdateById(Id.ToString(), orgid);
                    if (_result.Status)
                        return Ok(_result.Message);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for IsReadNotification for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}")]
        public async Task<IActionResult> MarkAllAsRead(string userid, string orgid)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetResourceUtil\n");

                if (orgid != null)
                {
                    var _result = await mongoDAL.NotificationStatusUpdate_MarkAllRead_ByOrgId(orgid);
                    if (_result.Status)
                        return Ok(_result.Message);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for IsReadNotification for user: " + userid);
                return BadRequest(ex.ToString());
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}/{id}")]
        public async Task<IActionResult> DeleteNotification(string userid, string orgid, int Id)
        {
            try
            {
                logger.LogInformation("\nGET request triggered for GetResourceUtil\n");

                if (orgid != null)
                {
                    var _result = await mongoDAL.NotificationDeleteById(Id.ToString(), orgid);
                    if (_result.Status)
                        return Ok($"Deleted");
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for IsReadNotification for user: " + userid);
                return BadRequest(ex.ToString());
            }

        }

        [HttpPost]
        public async Task<IActionResult> EA_AllSpecifications([FromBody] POST_ALL_DTO allValues)
        {
            if (allValues is null)
                return BadRequest("Input Invalid");

            if (string.IsNullOrEmpty(allValues.OrgID))
                return BadRequest("Invalid Organization.");

            if (!await mongoDAL.CheckDevices(allValues.ObjectID, allValues.OrgID))
                return BadRequest("Device does not exist in Devices list.");

            if (!await mongoDAL.CheckDevicesApprovedOrNot(allValues.ObjectID, allValues.OrgID))
                return BadRequest("Device is not approved.");

            // MongoDB
            var _result = mongoDAL.AddDevicesInfo(allValues);

            return Ok(_result);

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginActivity([FromBody] EndAgent_API.Models.Dto__Data_Tranfer_Objects_.LoginLogoutActivity loginActivity)
        {
            try
            {
                var _result = await mongoDAL.AddLoginActivity(loginActivity);
                if (_result.Status)
                    return Ok(_result.Message);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return BadRequest();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}")]
        public async Task<IActionResult> GetDevices(string userid, string orgid)
        {
            try
            {
                if (!string.IsNullOrEmpty(orgid))
                {
                    var devices = await mongoDAL.GetAllDevicesByOrgId(orgid);
                    if (devices == null || devices.Count == 0) return NotFound();

                    return Ok(devices);
                }
                return BadRequest("Invalid Organization.");
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for IsReadNotification for user: " + userid);
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{ID}")]
        public async Task<IActionResult> ApproveDevice(string userid, string ID)
        {
            var _isApprovedFlag = await mongoDAL.ApproveDeviceByObjectId(ID);
            if (_isApprovedFlag.Status)
                return Ok();

            return NotFound();

        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{ID}")]
        public async Task<IActionResult> DeleteDevice(string userid, string ID)
        {

            var _result = await mongoDAL.DeleteDeviceById(ID);
            if (_result.Status)
                return Ok();

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] DeviceDTO value)
        {
            // If in ITAM CheckITAMds (laptop,cpu,mobiled,server,none)
            Validations validations = new Validations();
            string dtypeurl = $"{baseUrl}{deviceTypeEndPoint}/{value.OrgID}/{value.SystemName}";
            var d = await validations.GetDataInOneString(dtypeurl, itamApiKeyHeader, itamApiKey);
            if (d.Contains("*error*"))
            {
                logger.LogInformation($"\n Error EASpecifications: " + d.ToString());

            }
            bool isITAMFlag = false;
            if (d != "none")
            {
                isITAMFlag = true;
            }

            ////Dev: Srikanth Erukulla: 30-06-2025 - MongoDB Database Integration
            string _result = await mongoDAL.AddDevices(value, isITAMFlag);
            return Ok(_result);
        }


        // Dev: Viraj Date: 09-05-2024
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{userid}/{orgid}")]
        public async Task<IActionResult> GetDevicesForUpdate(string userid, string orgid)
        {
            try
            {
                if (!string.IsNullOrEmpty(orgid))
                {
                    var _devices = await mongoDAL.GetDevicesByApproved(true, orgid);
                    if (_devices != null)
                        return Ok(_devices);

                    return NotFound();
                }

                return BadRequest("Invalid Organization.");
            }
            catch (Exception ex)
            {
                logger.LogInformation("Code Error for IsReadNotification for user: " + userid);
                return BadRequest(ex.ToString());
            }
        }

        // Dev: Viraj; Date:24-05-2024; Get BIOS_SN of a device using SystemInformation 
        [HttpGet]
        [Route("{ID}")]
        public async Task<ActionResult> GetBIOS(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    logger.LogInformation("ID is empty/null");
                    return BadRequest("ID is empty/null");
                }

                var device = await mongoDAL.GetDeviceByAssetId(ID);  //db.Devices.FirstOrDefault(d => d.Id.ToString().Trim() == ID.Trim());
                if (device == null)
                {
                    return NotFound();
                }

                BIOSDetailsDTO data = new BIOSDetailsDTO();
                data.BIOS = device.BIOS;
                data.ID = device.ID;
                return Ok(data);

            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }

        // Patch Management
        // Dev: Viraj; Date:15-06-2024; Upload Patches and App (msi setup) files
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromBody] FileUploadDTO model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.UploadedFileBase64) || string.IsNullOrWhiteSpace(model.UpdateName))
                {
                    return BadRequest("Invalid data received.");
                }

                // Convert Base64 string to byte array
                byte[] fileBytes = Convert.FromBase64String(model.UploadedFileBase64);

                // Ensure the directory exists
                var uploadsFolderPath = Path.Combine("D:", "EPT", "OSPatches");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }

                var fileName = model.UpdateName; // Use UploadedFileName for file name
                var filePath = Path.Combine(uploadsFolderPath, fileName);

                // Write the byte array to a file
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                var normalizedFilePath = Path.GetFullPath(filePath);
                // MongoDB - 14-07-2025 : Srikanth Erukulla Developer 
                var _fileUploadEntry = await mongoDAL.AddFileUpload(model, normalizedFilePath, model.OrgID);
                if (!_fileUploadEntry.Status)
                    return BadRequest($"Failed to save file metadata: {_fileUploadEntry.Message}");

                // 5️⃣ Automatically create/update AppInstaller entry
                var exeName = Path.GetFileName(filePath);
                var existingInstaller = await mongoDAL.GetAppInstallerByExeAsync(exeName);

                if (existingInstaller == null)
                {
                    string extension = Path.GetExtension(exeName).ToLower();
                    string installerType = extension == ".msi" ? "msi" : "exe";

                    // 🔑 SilentArgs decision (ADMIN CONTROLLED)
                    string silentArgs;

                    if (!string.IsNullOrWhiteSpace(model.Parameters))
                    {
                        // Admin explicitly provided args
                        silentArgs = model.Parameters.Trim();
                    }
                    else if (installerType == "msi")
                    {
                        // Safe MSI default
                        silentArgs = "/qn /norestart";
                    }
                    else
                    {
                        // EXE → DO NOT GUESS
                        silentArgs = string.Empty;
                    }

                    var newInstaller = new AppInstallerDTO
                    {
                        AppName = Path.GetFileNameWithoutExtension(exeName),
                        ExecutableName = exeName,
                        InstallerType = installerType,
                        SilentArgs = silentArgs,
                        DetectionType = "file",
                        DetectionValue = normalizedFilePath,
                        RequiresUserSession = false,
                        RebootRequired = false,
                        Enabled = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await mongoDAL.InsertAppInstallerAsync(newInstaller);
                }

                return Ok(new
                {
                    message = "File uploaded successfully, AppInstaller entry created/updated.",
                    filePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to upload file: {ex.Message}");
            }
        }
      
        [HttpGet]
        [Route("{orgid}")]
        public async Task<IActionResult> LoadPatches(string orgid)
        {
            try
            {
                if (string.IsNullOrEmpty(orgid))
                {
                    return BadRequest();
                }
                // MongoDB - 14-07-2025 : Srikanth Erukulla Developer 
                var patches = await mongoDAL.GetFileUploades(orgid);
                List<FileUploadDTO> files = new List<FileUploadDTO>();
                if (patches == null || patches.Count() == 0)
                {
                    return Ok(files);
                }

                foreach (var patch in patches)
                {
                    FileUploadDTO model = new()
                    {
                        Id = patch.Id,
                        UpdateBitRate = patch.UpdateBitRate,
                        UpdateID = patch.UpdateID,
                        UpdateName = patch.UpdateName,
                        UpdateOS = patch.UpdateOS,
                        KBNumber = patch.KBNumber,
                        KBNumberDescription = patch.KBNumberDescription,
                        UploadedFileBase64 = "",
                        OrgID = patch.OrgID
                    };
                    files.Add(model);
                }
                return Ok(files);

            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.ToString());

                return BadRequest();
            }
        }

        // Dev: Srikanth - Date:27-01-2026; Get Total Available and Installed Patches Count
        [HttpGet]
        [Route("{orgid}/{assetId}")]
        public async Task<OSPatchesCount> GetAvailableAndInstalledPatchesCount(string orgid, string assetId, string kbtype)
        {
            try
            {
                int availableCount = 0;
                int installedCount = 0;

                if (kbtype.Equals("kb", StringComparison.OrdinalIgnoreCase))
                {
                    // OS Patches
                    var patches = await mongoDAL.GetOSCentralPatches(orgid);

                    availableCount = patches?
                        .Where(x => !string.IsNullOrWhiteSpace(x.KBNumber) && x.KBNumber != "app")
                        .Count() ?? 0;

                    var installed = await mongoDAL.GetUpdates(assetId, orgid);
                    installedCount = installed?.Count() ?? 0;
                }
                else
                {
                    // Software Patches
                    var softwares = await mongoDAL.GetSoftwareCentralPatches(orgid);

                    availableCount = softwares?
                        .Where(x => !string.IsNullOrWhiteSpace(x.PatchNumber) && x.IsDeleted != true)
                        .Count() ?? 0;

                    var installedApps = await mongoDAL.GetDevicesInstalledApps(assetId, orgid);
                    installedCount = installedApps?.Count() ?? 0;
                }

                return new OSPatchesCount
                {
                    AvailablePatchesCount = availableCount,
                    InstalledPatchesCount = installedCount
                };
            }
            catch
            {
                return new OSPatchesCount
                {
                    AvailablePatchesCount = 0,
                    InstalledPatchesCount = 0
                };
            }
        }
      
        [HttpGet]
        [Route("{orgid}")]
        public async Task<IActionResult> GetEligiblePatches(string orgid)
        {
            try
            {
                if (string.IsNullOrEmpty(orgid))
                {
                    return BadRequest("Organization ID is required.");
                }

                // MongoDB - 14-07-2025 : Srikanth Erukulla Developer 
                var devices = await mongoDAL.GetEASpecifications(orgid);

                var patches = await mongoDAL.GetFileUploades(orgid);

                var eligiblePatchesList = new List<EligiblePatches>();

                foreach (var device in devices)
                {
                    var KBNumbers = (await mongoDAL.GetUpdates(device.AssetID, orgid)).Select(U => U.Patch).ToList();

                    var eligiblePatches = new EligiblePatches
                    {
                        //SystemID = device.AssetID.ToString(),
                        SystemID = device.AssetID,
                        OrgID = orgid,
                        DeviceName = device.SystemName, // Assuming the property name is correct
                        DeviceBIOS = (await mongoDAL.GetDevice(device.AssetID, orgid)).BIOS, // Assuming the property name is correct
                        User = device.LoginUser, // Assuming the property name is correct
                        Patches = patches.Where(patch => !KBNumbers.Contains(patch.KBNumber) && patch.KBNumber != "app" && patch.UpdateBitRate == device.SubnetMask && (patch.UpdateOS.Contains(device.OperatingSystem) || device.OperatingSystem.Contains(patch.UpdateOS)))
                                         .Select(patch => new EPTPatchDataDTO
                                         {
                                             PatchID = patch.Id ?? "0",
                                             PatchName = string.IsNullOrEmpty(patch.UpdateName) ? patch.KBNumber : patch.UpdateName,
                                             KBNumber = patch.KBNumber
                                         })
                                         .ToList()
                    };

                    // Filter out patches already added to UpdatePatchQueue
                    //db.UpdatePatchQueue.Any(upq => upq.SystemID == device.ID.ToString() &&(upq.UpdateName == patch.PatchName || upq.UpdateKBNumber == patch.KBNumber))
                    eligiblePatches.Patches = eligiblePatches.Patches
                        .Where(patch => !mongoDAL.CheckUpdatePatchQueueAny(device.ID, patch.PatchName, patch.KBNumber))
                        .ToList();

                    eligiblePatchesList.Add(eligiblePatches);
                }

                return Ok(eligiblePatchesList);
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("{orgid}/{assetId}/{kbtype}")]
        public async Task<IActionResult> GetAvailablePatches(string orgid, string assetId, string kbtype)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orgid))
                    return BadRequest("Organization ID is required.");

                if (string.IsNullOrWhiteSpace(assetId))
                    return BadRequest("Asset ID is required.");

                // 1️⃣ Installed Updates - Patches
                var installedUpdates = await mongoDAL.GetDevicesUpdates(assetId, orgid);

                var installedKbSet = installedUpdates?
                                                    .Where(x => !string.IsNullOrWhiteSpace(x.Patch))
                                                    .Select(x => x.Patch.Trim().ToUpper())
                                                    .ToHashSet() ?? new HashSet<string>();

                // 2️⃣ Device Info
                var device = await mongoDAL.GetEASpecificationByAssetId(assetId, orgid);
                if (device == null || string.IsNullOrWhiteSpace(device.ID))
                    return NotFound("Device not found.");

                var deviceOS = device.OperatingSystem?.Trim().ToUpper();
                //var deviceArchitecture = device.OperatingSystem != null && device.OperatingSystem.Contains("64") ? "X64" : "X86";
                var deviceArchitecture = device.SubnetMask;
                //// 3️⃣ Already Queued
                //var queuedKbRaw = await mongoDAL.GetQueuedKbNumbers(assetId);

                //var queuedKbSet = queuedKbRaw?
                //                            .Where(x => !string.IsNullOrWhiteSpace(x))
                //                            .Select(x => x.Trim().ToUpper())
                //                            .ToHashSet() ?? new HashSet<string>();
                var successfulPatchIds = await mongoDAL.GetSuccessfulPatchIds(assetId);

                var successfulPatchIdSet = successfulPatchIds?
                                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                                            .ToHashSet() ?? new HashSet<string>();
                //var queuedPatchIds = await mongoDAL.GetQueuedPatchIds(assetId);
                var queuedPatchIds = await mongoDAL.GetPendingQueuedPatchIds(assetId);

                var queuedPatchIdSet = queuedPatchIds?
                                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                                    .ToHashSet() ?? new HashSet<string>();
                var eligiblePatches = new OSPatches
                {
                    SystemID = assetId,
                    OrgID = orgid,
                    DeviceName = device.SystemName,
                    DeviceBIOS = (await mongoDAL.GetDevice(assetId, orgid))?.BIOS,
                    User = device.LoginUser
                };

                if (kbtype.Equals("app", StringComparison.OrdinalIgnoreCase))
                {
                    var softwarePatches = await mongoDAL.GetSoftwareCentralPatches(orgid);
                    var installedApps = await mongoDAL.GetDevicesInstalledApps(assetId, orgid);
                    eligiblePatches.AvailablePatches = softwarePatches
                                                                    .Where(patch =>
                                                                    {
                                                                        if (patch.IsDeleted)
                                                                            return false;

                                                                        //// already successfully installed through queue
                                                                        //if (!string.IsNullOrWhiteSpace(patch.Id) && successfulPatchIdSet.Contains(patch.Id))
                                                                        //    return false;
                                            
                                                                        var installed = installedApps
                                                                            .FirstOrDefault(x => x.AppName.StartsWith(
                                                                                patch.SoftwareName.Length >= 4 ? patch.SoftwareName.Substring(0, 4) : patch.SoftwareName,
                                                                                StringComparison.OrdinalIgnoreCase));

                                                                        // software not installed
                                                                        if (installed == null)
                                                                            return true;

                                                                        // version comparison
                                                                        if (Version.TryParse(installed.Version, out var installedVersion) &&
                                                                            Version.TryParse(patch.Version, out var patchVersion))
                                                                        {
                                                                            //return patchVersion > installedVersion;
                                                                            return patchVersion > installedVersion;
                                                                        }

                                                                        return false;
                                                                    })
                                                                    .Select(patch => new EPTPatchDataDTO
                                                                    {
                                                                        PatchID = patch.Id ?? "0",
                                                                        PatchName = patch.SoftwareName,
                                                                        KBNumber = patch.PatchNumber,
                                                                        PatchOS = patch.Version,
                                                                        PatchBitRate = patch.BitRate,

                                                                        // ✔ correct queue check
                                                                        PatchStatus = (!string.IsNullOrWhiteSpace(patch.Id) &&
                                                                                       queuedPatchIdSet.Contains(patch.Id))
                                                                                       ? "Installing"
                                                                                       : "Available"
                                                                    })
                                                                    .ToList();
                                                        
                }                                                        
                else                                                        
                {
                    var osPatches = await mongoDAL.GetOSCentralPatches(orgid);
                    var normalizedDeviceOS = deviceOS?.Trim().ToUpper();
                    eligiblePatches.AvailablePatches = osPatches
                                        .Where(patch =>
                                            // OSVersion 
                                            (device.OSVersion == patch.OSVersion) &&

                                            // patch must have KB
                                            !string.IsNullOrWhiteSpace(patch.KBNumber) &&

                                            // patch must not already be installed in system updates
                                            !installedKbSet.Contains(patch.KBNumber.Trim().ToUpper()) &&

                                            // patch must not already be installed through patch queue
                                            !string.IsNullOrWhiteSpace(patch.Id) &&
                                            !successfulPatchIdSet.Contains(patch.Id) &&

                                            // patch must match system OS
                                            (
                                                string.IsNullOrWhiteSpace(patch.UpdateOS) ||
                                                normalizedDeviceOS.Contains(patch.UpdateOS.Trim().ToUpper())
                                            ) &&

                                            // architecture compatibility
                                            (
                                                string.IsNullOrWhiteSpace(patch.BitRate) ||
                                                patch.BitRate.Equals(deviceArchitecture, StringComparison.OrdinalIgnoreCase)
                                            ) &&


                                            // patch must be active
                                            patch.IsDeleted != true
                                        )
                                        .Select(patch => new EPTPatchDataDTO
                                        {
                                            PatchID = patch.Id ?? "0",
                                            PatchName = string.IsNullOrWhiteSpace(patch.Title) ? patch.KBNumber : patch.Title,
                                            KBNumber = patch.KBNumber,
                                            PatchOS = patch.UpdateOS,
                                            PatchBitRate = patch.BitRate,
                                            // ✔ correct queue check
                                            PatchStatus = (!string.IsNullOrWhiteSpace(patch.Id) &&
                                                           queuedPatchIdSet.Contains(patch.Id))
                                                           ? "Installing"
                                                           : "Available"
                                        })
                                        .ToList();
                }
                    eligiblePatches.InstalledPatches =
                    installedUpdates?.ToList() ?? new List<UpdateDto>();

                eligiblePatches.OSPatchesCount = new OSPatchesCount
                {
                    AvailablePatchesCount = eligiblePatches.AvailablePatches.Count,
                    InstalledPatchesCount = eligiblePatches.InstalledPatches.Count
                };

                return Ok(eligiblePatches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //public async Task<IActionResult> GetAvailablePatches(string orgid, string assetId, string kbtype)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(orgid))
        //        {
        //            return BadRequest("Organization ID is required.");
        //        }

        //        if (string.IsNullOrEmpty(assetId))
        //        {
        //            return BadRequest("assetId Id is required.");
        //        }
        //        //added for comparision of tnstalled Vs available patches
        //        var installedUpdates = await mongoDAL.GetUpdates(assetId, orgid);

        //        var installedKbSet = installedUpdates
        //                                .Where(x => !string.IsNullOrEmpty(x.Patch))
        //                                .Select(x => x.Patch.Trim())
        //                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        //        var osCentralPatches = await mongoDAL.GetOSCentralPatches(orgid);


        //        var eligiblePatches = new OSPatches();
        //        eligiblePatches.OSPatchesCount = await GetAvailableAndInstalledPatchesCount(orgid, assetId, kbtype);
        //        var device = await mongoDAL.GetEASpecificationByAssetId(assetId, orgid);

        //        if (device != null && device.ID!=null)
        //        {
        //            var patches = await mongoDAL.GetFileUploades(orgid);
        //            var KBNumbers = (await mongoDAL.GetUpdates(assetId, orgid)).Select(U => U.Patch).ToList();


        //            //SystemID = device.AssetID.ToString(),
        //            eligiblePatches.SystemID = assetId;
        //            eligiblePatches.OrgID = orgid;
        //            eligiblePatches.DeviceName = device.SystemName;
        //            eligiblePatches.DeviceBIOS = (await mongoDAL.GetDevice(assetId, orgid)).BIOS;
        //            eligiblePatches.User = device.LoginUser;
        //            if (kbtype != "app")
        //            {
        //                eligiblePatches.AvailablePatches = patches.Where(patch => !KBNumbers.Contains(patch.KBNumber) && patch.KBNumber != "app" && patch.UpdateBitRate == device.SubnetMask && (patch.UpdateOS.Contains(device.OperatingSystem) || device.OperatingSystem.Contains(patch.UpdateOS)))
        //                                 .Select(patch => new EPTPatchDataDTO
        //                                 {
        //                                     PatchID = patch.Id ?? "0",
        //                                     PatchName = string.IsNullOrEmpty(patch.UpdateName) ? patch.KBNumber : patch.UpdateName,
        //                                     KBNumber = patch.KBNumber,
        //                                     KBNumberDescription = patch.KBNumberDescription,
        //                                     PatchOS = patch.UpdateOS,
        //                                     PatchBitRate = patch.UpdateBitRate,
        //                                     PatchStatus = (mongoDAL.GetUpdatePatchQueueStatusOnly(assetId, patch.UpdateName, patch.KBNumber)).Result

        //                                 })
        //                                 .ToList();
        //            }
        //            else
        //            {
        //                eligiblePatches.AvailablePatches = patches.Where(patch => !KBNumbers.Contains(patch.KBNumber) && patch.KBNumber == "app" && patch.UpdateBitRate == device.SubnetMask && (patch.UpdateOS.Contains(device.OperatingSystem) || device.OperatingSystem.Contains(patch.UpdateOS)))
        //                                 .Select(patch => new EPTPatchDataDTO
        //                                 {
        //                                     PatchID = patch.Id ?? "0",
        //                                     PatchName = string.IsNullOrEmpty(patch.UpdateName) ? patch.KBNumber : patch.UpdateName,
        //                                     KBNumber = patch.KBNumber,
        //                                     KBNumberDescription = patch.UpdateID, // Software Available (Software Name assigning into KBNumberDescription)
        //                                     PatchOS = patch.UpdateOS,
        //                                     PatchBitRate = patch.UpdateBitRate,
        //                                     PatchStatus = (mongoDAL.GetUpdatePatchQueueStatusOnly(assetId, patch.UpdateName, patch.KBNumber)).Result

        //                                 })
        //                                 .ToList();
        //            }


        //            // Filter out patches already added to UpdatePatchQueue
        //            //db.UpdatePatchQueue.Any(upq => upq.SystemID == device.ID.ToString() &&(upq.UpdateName == patch.PatchName || upq.UpdateKBNumber == patch.KBNumber))
        //            eligiblePatches.AvailablePatches = eligiblePatches.AvailablePatches
        //                .Where(patch => !mongoDAL.CheckUpdatePatchQueueAny(device.ID, patch.PatchName, patch.KBNumber))
        //                .ToList();

        //        }

        //        return Ok(eligiblePatches);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exceptions (logging, etc.)
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        [HttpGet]
        [Route("{orgid}/{assetId}/{kbtype}")]
        public async Task<IActionResult> GetInstalledPatches(string orgid, string assetId, string kbtype)
        {
            try
            {
                var eligiblePatches = new OSPatches();
                eligiblePatches.OSPatchesCount = await GetAvailableAndInstalledPatchesCount(orgid, assetId, kbtype);
                var device = await mongoDAL.GetEASpecificationByAssetId(assetId, orgid);
                if (device != null && device.ID != null)
                {
                    eligiblePatches.SystemID = assetId;
                    eligiblePatches.OrgID = orgid;
                    eligiblePatches.DeviceName = device.SystemName;
                    eligiblePatches.DeviceBIOS = (await mongoDAL.GetDevice(assetId, orgid)).BIOS;
                    eligiblePatches.User = device.LoginUser;
                    var _patches = await mongoDAL.GetUpdates(assetId, orgid);
                    eligiblePatches.InstalledPatches = _patches;
                }

                return Ok(eligiblePatches);
            }
            catch (Exception ex)
            {
                return NotFound();
            }

            return NotFound();
        }
        [HttpGet]
        [Route("{orgid}/{assetId}/{kbtype}")]
        public async Task<IActionResult> GetInstalledSoftwares(string orgid, string assetId, string kbtype)
        {
            try
            {
                var eligiblePatches = new OSPatches();
                eligiblePatches.OSPatchesCount = await GetAvailableAndInstalledPatchesCount(orgid, assetId, kbtype);
                var device = await mongoDAL.GetEASpecificationByAssetId(assetId, orgid);
                if (device != null && device.ID != null)
                {
                    eligiblePatches.SystemID = assetId;
                    eligiblePatches.OrgID = orgid;
                    eligiblePatches.DeviceName = device.SystemName;
                    eligiblePatches.DeviceBIOS = (await mongoDAL.GetDevice(assetId, orgid)).BIOS;
                    eligiblePatches.User = device.LoginUser;
                    var _patches = await mongoDAL.GetDevicesInstalledApps(assetId, orgid);
                    eligiblePatches.InstalledApps = _patches;
                }

                return Ok(eligiblePatches);
            }
            catch (Exception ex)
            {
                return NotFound();
            }

            return NotFound();
        }


        // Dev: Viraj; Date:20-06-2024; Get Eligible Apps available for a devices in an Org

        [HttpGet]
        [Route("{orgid}/{assetId}")]
        public async Task<IActionResult> GetPatches(string orgid, string assetId)
        {
            try
            {
                var _patches = await mongoDAL.GetDevice(assetId, orgid);
                if (_patches != null)
                    return Ok(_patches);
            }
            catch (Exception ex)
            {
                return NotFound();
            }

            return NotFound();
        }

        [HttpGet]
        [Route("{orgid}")]
        public async Task<IActionResult> GetEligibleApps(string orgid)
        {
            try
            {
                if (string.IsNullOrEmpty(orgid))
                {
                    return BadRequest("Organization ID is required.");
                }

                // MongoDB - 29-10-2025 : Srikanth Erukulla Developer 
                var devices = await mongoDAL.GetEASpecifications(orgid);

                var patches = await mongoDAL.GetFileUploades(orgid);

                var eligiblePatchesList = new List<EligiblePatches>();

                foreach (var device in devices)
                {
                    var AppNames = (await mongoDAL.GetInstalledApps(device.AssetID, orgid)).Select(a => a.AppName).ToList();

                    var eligiblePatches = new EligiblePatches
                    {
                        //SystemID = device.ID.ToString(),
                        SystemID = device.AssetID,
                        OrgID = orgid,
                        DeviceName = device.SystemName, // Assuming the property name is correct
                        DeviceBIOS = (await mongoDAL.GetDevice(device.AssetID, orgid)).BIOS,
                        User = device.LoginUser, // Assuming the property name is correct
                        Patches = patches
                            .Where(patch => !AppNames.Contains(patch.UpdateID) && patch.KBNumber == "app" && patch.UpdateBitRate == device.SubnetMask && (patch.UpdateOS.Contains(device.OperatingSystem) || device.OperatingSystem.Contains(patch.UpdateOS)))
                            .Select(patch => new EPTPatchDataDTO
                            {
                                PatchID = patch.Id ?? "0",
                                PatchName = patch.UpdateName,
                                KBNumber = patch.KBNumber
                            })
                            .ToList()
                    };

                    // Filter out patches already added to UpdatePatchQueue
                    eligiblePatches.Patches = eligiblePatches.Patches
                        .Where(patch => !mongoDAL.CheckUpdatePatchQueueAny(device.ID, patch.PatchName))
                        .ToList();

                    eligiblePatchesList.Add(eligiblePatches);
                }

                return Ok(eligiblePatchesList);
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Dev: Viraj; Date:25-06-2024; Add Apps and Updates in Queue when submitted so service will know to download and update
        [HttpPost]
        public async Task<IActionResult> AddUpdatePatchesinQueue(List<UpdatePatchQueueTempDTO> updates)
        {
            try
            {
                Dictionary<string, string> _message = new Dictionary<string, string>();

                if (updates == null || updates.Count == 0)
                    return BadRequest("No updates provided.");

                var validPatchIDs = await mongoDAL.GetFileUploadIDs();
                var validSystemIDs = await mongoDAL.GetEASpecificationIDs();

                string _patchfiletargetedlocation = configuration.GetValue<string>("PatchSync:PatchFolder");

                foreach (var update in updates)
                {
                    // Validate IDs
                    if (!validPatchIDs.Contains(update.PatchID) || !validSystemIDs.Contains(update.SystemID))
                        continue;

                    var patch = await mongoDAL.GetFileUploadeById(update.PatchID);

                    if (patch == null || string.IsNullOrWhiteSpace(patch.KBNumber) || string.IsNullOrWhiteSpace(patch.PatchFilePath))
                        continue;

                    string filePathRaw = patch.PatchFilePath ?? "";

                    // ✅ Extract file names first
                    var fileNames = filePathRaw
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Select(path => Path.GetFileName(path))
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    if (fileNames.Count == 0)
                        continue;

                    // ✅ Build full paths
                    var finalPaths = fileNames
                        .Select(name => Path.Combine(_patchfiletargetedlocation, name));

                    // ✅ Convert to comma-separated strings
                    string finalPathString = string.Join(",", finalPaths);
                    string fileNameString = string.Join(",", fileNames);

                    var model = new UpdatePatchQueue
                    {
                        PatchId = update.PatchID,
                        UpdateName = patch.PatchTitle,
                        PatchFilePath = finalPathString,   // full paths
                        PatchFileName = fileNameString,    // ✅ filenames only
                        UpdateKBNumber = patch.KBNumber,
                        Status = "0",
                        SystemID = update.SystemID,
                        Reason = "",
                        CreatedAt = DateTime.UtcNow,
                        ScheduledTime = update.ScheduleTime,
                        OrgId = update.OrgId
                    };

                    var result = await mongoDAL.UpdatePatchQueue(model);

                    if (result.Status)
                    {
                        await mongoDAL.AddPatchEntry(model);
                        _message[model.UpdateName] = result.Message;
                    }
                }

                return Ok(_message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdatePatchesinQueue_Software(List<UpdatePatchQueueTempDTO> updates)
        {
            try
            {
                Dictionary<string, string> _message = new Dictionary<string, string>();

                if (updates == null || updates.Count == 0)
                    return BadRequest("No updates provided.");

                var validPatchIDs = await mongoDAL.GetSoftwareFileUploadIDs();
                var validSystemIDs = await mongoDAL.GetEASpecificationIDs();

                foreach (var update in updates)
                {
                    if (!validPatchIDs.Contains(update.PatchID) || !validSystemIDs.Contains(update.SystemID))
                        continue;
                    string _patchfiletargetedlocation = configuration.GetValue<string>("PatchSync:PatchFolder");
                    var patch = await mongoDAL.GetSoftwareFileUploadeById(update.PatchID);
                    if (patch == null || string.IsNullOrWhiteSpace(patch.KBNumber) || string.IsNullOrWhiteSpace(patch.PatchFilePath))
                        continue;

                    var model = new UpdatePatchQueue
                    {
                        PatchId = update.PatchID,
                        UpdateName = patch.PatchTitle,        // Display title                       
                        PatchFilePath = $"{_patchfiletargetedlocation}\\{Path.GetFileName(patch.PatchFilePath ?? "")}",  // Full path
                        PatchFileName = Path.GetFileName(patch.PatchFilePath ?? ""),
                        UpdateKBNumber = patch.KBNumber,
                        Status = "0",
                        SystemID = update.SystemID,
                        Reason = "",
                        CreatedAt = DateTime.UtcNow,
                        ScheduledTime = update.ScheduleTime,
                        OrgId=update.OrgId
                    };

                    var result = await mongoDAL.UpdatePatchQueue(model);
                    if (result.Status)
                    {
                        await mongoDAL.AddPatchEntry(model);
                        _message[model.UpdateName] = result.Message;
                    }
                }

                return Ok(_message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Dev: Srikanth Erukulla; 24-03-2026; Download Patches and App setups uploaded and pushed to certain device for Windows Service        
        [HttpGet]
        [Route("{objid}")]
        public async Task<IActionResult> GetPatchesAndAppQueueFile(string objid)
        {
            try
            {
                if (string.IsNullOrEmpty(objid))
                    return BadRequest("Object ID is required.");

                var update = (await mongoDAL.GetUpdatePatchQueues(objid))
                                .Where(u => u.Status == "0")
                                .OrderBy(u => u.CreatedAt)
                                .FirstOrDefault();

                if (update == null)
                    return NotFound("No updates in the queue with status 0.");

                if (DateTime.Now <= update.ScheduledTime)
                    return Conflict("Scheduled time not reached yet");

                if (string.IsNullOrWhiteSpace(update.PatchFilePath))
                    return NotFound("No patch file path specified.");

                // Parse comma-separated paths
                var filePaths = update.PatchFilePath
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(p => p.Trim())
                                      .Where(p => System.IO.File.Exists(p))
                                      .ToList();

                if (!filePaths.Any())
                    return NotFound($"No valid patch files found for '{update.PatchFileName}'.");

                // Single file — stream directly, no ZIP overhead
                if (filePaths.Count == 1)
                {
                    var fileStream = new FileStream(filePaths[0], FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                    string singleFileName = Path.GetFileName(filePaths[0]) + "|" + update.UpdateKBNumber;
                    return File(fileStream, "application/octet-stream", singleFileName);
                }

                // Multiple files — stream ZIP directly into response (no RAM buffering)
                string zipFileName = $"{update.PatchFileName}|{update.UpdateKBNumber}.zip";

                Response.ContentType = "application/zip";
                Response.Headers.ContentDisposition = $"attachment; filename=\"{zipFileName}\"";

                // Stream ZIP directly into the response body
                using var archive = new ZipArchive(Response.BodyWriter.AsStream(), ZipArchiveMode.Create, leaveOpen: true);

                foreach (var filePath in filePaths)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Fastest);
                    await using var entryStream = entry.Open();
                    await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                    await fileStream.CopyToAsync(entryStream); // copies in chunks, not all at once
                }

                return new EmptyResult(); // response already written above
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Dev: Viraj; Date:25-06-2024; Download App setups uploaded and pushed to certain device for Windows Service
        [HttpGet]
        [Route("{objid}")]
        public async Task<IActionResult> GetAppQueueFile(string objid)
        {
            try
            {
                if (string.IsNullOrEmpty(objid))
                    return BadRequest("Object ID is required.");

                var update = (await mongoDAL.GetUpdatePatchQueues(objid))
                                .Where(u => u.Status == "0" && u.UpdateKBNumber == "app")
                                .OrderBy(u => u.CreatedAt)
                                .FirstOrDefault();

                if (update == null)
                    return NotFound("No updates in the queue with status 0.");
                //update.PatchFilePath =  "D:\\EPT\\OSPatches\\AnyDesk.exe"
                if (string.IsNullOrWhiteSpace(update.PatchFilePath) || !System.IO.File.Exists(update.PatchFilePath))
                    return NotFound($"Patch file '{update.PatchFileName}' not found.");

                //Scheduled Time for App Installation
                if (DateTime.Now <= update.ScheduledTime)
                    return Conflict("Scheduled time not reached yet");

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(update.PatchFilePath);
                return File(fileBytes, "application/octet-stream", update.PatchFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Dev: Viraj; Date:25-06-2024; For Windows Service to download the update in queue (Ordered by pushed time ascending)
        [HttpGet]
        [Route("{objid}")]
        public async Task<IActionResult> GetUpdateQueueFile(string objid)
        {
            try
            {
                if (string.IsNullOrEmpty(objid))
                {
                    return BadRequest("Object ID is required.");
                }

                // Get queued updates for this system
                var updateList = await mongoDAL.GetUpdatePatchQueues(objid, "0");

                if (updateList == null || !updateList.Any())
                {
                    return NotFound("No updates in the queue with status 0.");
                }

                // Pick the first patch that is not an app installer
                var update = updateList
                    .Where(u => u.UpdateKBNumber != "app")
                    .OrderBy(u => u.CreatedAt)
                    .FirstOrDefault();

                if (update == null)
                    return NoContent();

                // Use PatchFilePath and PatchFileName from the DTO
                string filePath = update.PatchFilePath;
                string fileName = string.IsNullOrEmpty(update.PatchFileName)
                                  ? Path.GetFileName(filePath)
                                  : update.PatchFileName;

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File '{fileName}' not found at '{filePath}'.");
                }

                //Scheduled Time for Patch Installation
                if (DateTime.Now <= update.ScheduledTime)
                    return Conflict("Scheduled time not reached yet");

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Dev: Viraj; Date:25-06-2024; Update status of Patch/App queues in device status by service after installation
        [HttpPut]
        public async Task<IActionResult> EditUpdatePatchesinQueue([FromBody] EditupdatePatchQueueDTO model)
       {
            string _patchId = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(model.ObjID) || string.IsNullOrEmpty(model.UpdateName))
                {
                    return BadRequest();
                }
                //var systemID = await mongoDAL.GetEASpecificationByAssetId(model.ObjID);   //db.EASpecifications.FirstOrDefault(e => e.ObjectID.ToLower().Trim() == model.ObjID.ToLower().Trim()).ID.ToString();
                var _result = await mongoDAL.UpdateUpdatePatchQueue(model);
                if (_result.Status)
                {
                    var _patobject = await mongoDAL.GetPatchIdFromFileUpload(model.UpdateName, model.KBNumber);
                    // Add Patch Status into Patch History Table
                    TEST_WebApiOsDetails.Models.UpdatePatchQueue patchHistory = new UpdatePatchQueue()
                    {
                        SystemID = model.ObjID,
                        PatchId = !string.IsNullOrEmpty(_patobject) ? _patobject : "",
                        UpdateName = model.UpdateName,
                        UpdateKBNumber = model.KBNumber,
                        Status = model.Status,
                        Reason = model.Reason
                    };
                    await mongoDAL.AddPatchEntry(patchHistory);
                    return Ok(new
                    {
                        status = _result.Status,
                        // message = _result.Message,
                        reason = model.Reason
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });

            }
        }


        //// Dev: Viraj; Date:25-06-2024; Update status of Patch/App queues in device status by service after installation
        //[HttpPut("EditUpdateQueue")]
        //public async Task<IActionResult> EditUpdatePatchesinQueue([FromBody] EditupdatePatchQueueDTO model)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(model.PatchId))
        //            return BadRequest("PatchId is required");

        //        var _result = await mongoDAL.UpdateUpdatePatchQueue(model);

        //        if (!_result.Status)
        //            return NotFound("Patch not found");

        //        // Add Patch Status into Patch History Table
        //        var patchHistory = new UpdatePatchQueue()
        //        {
        //            SystemID = model.ObjID,
        //            PatchId = model.PatchId,
        //            UpdateName = model.UpdateName,
        //            UpdateKBNumber = model.KBNumber,
        //            Status = model.Status,
        //            Reason = model.Reason,
        //            CreatedAt = model.CreatedAt
        //        };

        //        await mongoDAL.AddPatchEntry(patchHistory);

        //        return Ok(new
        //        {
        //            status = true,
        //            reason = model.Reason
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            status = false,
        //            message = ex.Message
        //        });
        //    }
        //}

        // Dev: Viraj; Date:25-06-2024; Show logs of successful or failed installations
        [HttpGet]
        [Route("{orgid}")]
        public async Task<IActionResult> GetPatchLogs(string orgid)
        {
            try
            {
                if (string.IsNullOrEmpty(orgid))
                {
                    return BadRequest("Organization ID is required.");
                }

                var updatePatchesinQueue = await mongoDAL.GetUpdatePatchQueuesbyStatus("0");
                if (updatePatchesinQueue == null)
                {
                    return NotFound();
                }
                var model = new List<UpdatePatchQueue>();
                foreach (var x in updatePatchesinQueue)
                {
                    var org = await mongoDAL.GetEASpecificationByUniqueId(x.SystemID, orgid);
                    if (org != null)
                    {
                        x.SystemID = org.SystemName;
                        model.Add(x);
                    }
                }

                return Ok(model);

            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //added by vijaya     
        [HttpGet]
        [Route("{objid}")]
        public async Task<IActionResult> GetPatchStatus(string objid)
        {
            var patches = await mongoDAL.GetUpdatePatchQueuesStatus(objid);

            if (patches == null || !patches.Any())
                return NotFound("No patches found for this object.");

            var result = patches.Select(p => new
            {
                p.SystemID,
                p.PatchId,
                p.Status,
                p.Reason,
                p.CreatedAt
            }).ToList();

            return Ok(result);
        }
        [HttpGet]
        [Route("{exeName}")]
        public async Task<IActionResult> AppInstaller(string exeName)
        {
            if (string.IsNullOrWhiteSpace(exeName))
                return BadRequest("ExecutableName is required");

            var installer = await mongoDAL.GetAppInstallerByExeAsync(exeName);

            if (installer == null)
                return NotFound($"Installer not found for {exeName}");

            return Ok(new AppInstallerDTO
            {
                InstallerType = "exe",
                SilentArgs = installer
            });
        }
        // ---------------- CREATE / POST AppInstaller ----------------
        [HttpPost("AppInstaller")]
        public async Task<IActionResult> CreateAppInstaller(AppInstallerDTO model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.ExecutableName) ||
                string.IsNullOrWhiteSpace(model.SilentArgs))
            {
                return BadRequest("ExecutableName and SilentArgs are required.");
            }

            var existing = await mongoDAL.GetAppInstallerByExeAsync(model.ExecutableName);
            if (existing != null)
                return Conflict($"Installer policy already exists for {model.ExecutableName}");

            await mongoDAL.InsertAppInstallerAsync(model);

            return Ok(new
            {
                message = "Installer policy created successfully",
                executable = model.ExecutableName
            });
        }
        [HttpGet]
        [Route("{systemId}")]
        public async Task<IActionResult> HasPendingWork(string systemId)
        {
            var exists = await mongoDAL.HasPendingPatch(systemId);
            return Ok(exists);
        }
        [HttpPost("agent/log")]
        public async Task<IActionResult> Log([FromBody] AgentLogDto log)
        {

            await _logs.InsertOneAsync(log);
            return Ok();
        }

        // Developer : Srikanth Erukulla - 23-02-2026
        [HttpPost("cis/selected")]
        public async Task<IActionResult> AddCISPatches([FromBody] CISDTO model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { Status = false, Message = "object reference is null", StatusCode = 400 });

                var _result = await mongoDAL.AddCISOptions(model);
                if (_result.Status)
                    return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });

                return BadRequest(new { Status = _result.Status, Message = "something wrong, please try after sometime", StatusCode = 400 });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message, StatusCode = 400 });
            }
        }

        [HttpGet("cis/getselected/{orgId}")]
        public async Task<IActionResult> GetCISPatches(string orgId)
        {
            try
            {
                if (string.IsNullOrEmpty(orgId))
                    return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

                var _result = await mongoDAL.GetCISOptions(orgId);
                if (_result != null)
                    return Ok(_result);

                return BadRequest(new { Status = false, Message = "something wrong, fetching the cis options", StatusCode = 400 });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message, StatusCode = 400 });
            }
        }

        [HttpGet("cis/getall")]

        public async Task<IActionResult> GetCISPatches()

        {

            string _orgid = string.Empty;

            try

            {

                var _result = await mongoDAL.GetCISOptions();

                if (_result != null)

                {

                    foreach (var _cis in _result)

                    {

                        _orgid = _cis.OrgId;

                        var familyLits = _cis.OsPatchesSelected.Split(',').ToList();

                        var patches = await _patchSyncService.SyncTenantAsync(_cis.OrgId, familyLits);

                    }

                    return Ok();

                }

                return BadRequest(new { Status = false, Message = "something wrong, fetching the cis options", StatusCode = 400 });

            }

            catch (Exception ex)

            {

                logger.LogInformation($"error occured in GetCISPatches : {ex}");

                return BadRequest(new { Status = false, Message = ex.Message, StatusCode = 400 });

            }

        }


        [HttpGet("cisosapps/{orgId}")]
        public async Task<IActionResult> GetCISOSPatchesAndSoftware(string orgId)
        {
            try
            {
                if (string.IsNullOrEmpty(orgId))
                    return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

                var _result = await mongoDAL.GetOSPatchesAndSoftwares(orgId);
                if (_result != null)
                    return Ok(_result);

                return BadRequest(new { Status = false, Message = "something wrong, fetching the cis options", StatusCode = 400 });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message, StatusCode = 400 });
            }
        }

        //CDP stand for Create Desktop Policy
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> WhiteListPolicy([FromBody] DesktopPoliciesDTO modal)
        {
            if(modal==null)
                return BadRequest(new { Status = false, Message = "object should not be null", StatusCode = 400 });

            if (string.IsNullOrEmpty(modal.OrgId))
                return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

            var _result = await mongoDAL.AddDesktopPolicies(modal);
            if (!_result.Status) 
            {
                if (_result.Message.Contains("Already Exist"))
                {
                    return Conflict(new { Status = false, Message = _result.Message, StatusCode = 409 });
                }
                else
                {
                    return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });
                }
            }


            return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
        }

        [HttpGet("blockandunblock/{orgId}")]
        public async Task<IActionResult> DeviationDevices(string orgId)
        {
            if (string.IsNullOrEmpty(orgId))
                return BadRequest("OrgId is required.");

            // ✅ STEP 1: Fetch whitelist ONCE — single DB call outside all loops
            List<DesktopPoliciesDTO> blockedApps = await mongoDAL.GetDesktopPolicies(orgId);

            if (blockedApps == null || !blockedApps.Any())
                return Ok(new List<WhitelistAppsDTO>());

            // ✅ STEP 2: Build HashSet for O(1) case-insensitive lookup
            var blockedAppNames = new HashSet<string>(
                blockedApps
                    .Where(x => !string.IsNullOrEmpty(x.SoftwareName))
                    .Select(x => x.SoftwareName.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            // ✅ STEP 3: Fetch all devices once
            List<DevicesDTO> devices = await mongoDAL.GetDeviceByOrgId(orgId);

            if (devices == null || !devices.Any())
                return Ok(new List<WhitelistAppsDTO>());

            // ✅ STEP 4: Fetch installed apps for ALL devices in PARALLEL
            //           Task.WhenAll fires all DB calls simultaneously
            var tasks = devices.Select(async device =>
            {
                List<InstalledAppDto> appList = await mongoDAL.GetDevicesInstalledApps(
                    device.ID,
                    device.OrgID
                );

                // ✅ STEP 5: Check if ANY installed app matches the blocked list
                //           HashSet.Contains is O(1) — no nested loop DB calls
                bool isRestricted = appList.Any(app =>
                   !string.IsNullOrEmpty(app.AppName) &&
                   blockedAppNames.Contains(app.AppName.Trim())
                );

                //bool isRestricted = appList.Any(app =>
                //    !string.IsNullOrEmpty(app.AppName) &&
                //    blockedAppNames.Any(blocked =>
                //        app.AppName.Trim().Contains(blocked, StringComparison.OrdinalIgnoreCase))
                //);



                return new WhitelistAppsDTO
                {
                    DeviceName = device.SystemName,
                    LoginUser = device.LoginUser,
                    PublicIP = device.PublicIP,
                    Status = isRestricted ? "Restricted" : "Clean",
                    Restricted = isRestricted
                };
            });

            var whitelistApps = await Task.WhenAll(tasks);

            return Ok(whitelistApps);
        }
        //EndPoint Group : Creating new Endpoint Group
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> EndPointGroup([FromBody] EndpointGroup modal)
        {
            if (modal == null)
                return BadRequest(new { Status = false, Message = "object should not be null", StatusCode = 400 });

            if (string.IsNullOrEmpty(modal.OrgId))
                return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

            var _result = await mongoDAL.CreateEndpointGroup(modal);
            if (!_result.Status)
                return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });

            return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
        }
        //EndPoint Group : Creating new Endpoint Group
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateEndPointGroup([FromBody] UpdateEndpointGroup modal)
        {
            if (modal == null)
                return BadRequest(new { Status = false, Message = "object should not be null", StatusCode = 400 });

            if (string.IsNullOrEmpty(modal.OrgId))
                return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

            var _result = await mongoDAL.UpdateEndpointGroup(modal);
            if (!_result.Status)
                return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });

            return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
        }
        //EndPoint Group : Creating new Endpoint Group
        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateEndPointGroupScheduledTime([FromBody] GroupPatchScheduleTimeDTO modal)
        {
            try
            {
                if (modal == null)
                    return BadRequest(new { Status = false, Message = "object should not be null", StatusCode = 400 });

                if (string.IsNullOrEmpty(modal.OrgId))
                    return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

                var _result = await mongoDAL.UpdateEndPointGroupScheduledTime(modal);
                if (!_result.Status)
                    return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });

                await mongoDAL.UpdatePatchQueueforEndpointGrouping(modal.OrgId, modal.GroupId, modal.GroupPatchScheduledTimeId);

                return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
            }
            catch(Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message, StatusCode = 400 });
            }
        }
        [HttpPost("deletedevicebygroupid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteDeviceFromEndpointGroup([FromBody] DeleteDeviceEndPointGroupDTO model)
        {
            if (string.IsNullOrEmpty(model.GroupId))
                return BadRequest(new { Status = false, Message = "groupid should not be null", StatusCode = 400 });

            if (string.IsNullOrEmpty(model.OrgId))
                return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

            var _result = await mongoDAL.DeleteDeviceFromEndpointGroup(model);
            if (!_result.Status)
                return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });

            return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
        }
        [HttpPost("delete/{orgId}/{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEndPointGroup(string orgId, string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return BadRequest(new { Status = false, Message = "groupid should not be null", StatusCode = 400 });

            if (string.IsNullOrEmpty(orgId))
                return BadRequest(new { Status = false, Message = "orgid should not be null", StatusCode = 400 });

            var _result = await mongoDAL.DeleteEndpointGroup(groupId, orgId);
            if (!_result.Status)
                return BadRequest(new { Status = false, Message = _result.Message, StatusCode = 400 });

            return Ok(new { Status = _result.Status, Message = _result.Message, StatusCode = 200 });
        }
        //Get All Devices which not mapped with any endpoint groups
        [HttpGet("get/{orgId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEndPointGroup(string orgId)
        {
            try
            {
                var _result = await mongoDAL.GetDevicesNotInAnyGroup(orgId);
                if (_result != null && _result.Count > 0)
                    return Ok(_result);

                return NotFound();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Get All Devices which are mapped with particular endpoint group
        [HttpGet("getbygroupId/{orgId}/{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDevicesByEndPointGroupId(string orgId, string groupId)
        {
            try
            {
                var _result = await mongoDAL.GetDevicesByGroupId(groupId, orgId);
                if (_result != null && _result.Count > 0)
                    return Ok(_result);

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Get All group name
        [HttpGet("get/{orgId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEndPointGroupName(string orgId)
        {
            try
            {
                var _result = await mongoDAL.GetGroupsByOrgId(orgId);
                if (_result != null && _result.Count > 0)
                    return Ok(_result);

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get/{orgId}/{gid}/{gsid}")]
        public async Task<IActionResult> EPGUPQS(string orgid, string gid, string gsid)
        {
            var result =await  mongoDAL.UpdatePatchQueueforEndpointGrouping(orgid, gid, gsid);
            return Ok(result);
        }

        /// <summary>
        /// GET api/patchqueue/summary/{orgId}
        /// Returns full patch queue summary for graphs
        /// </summary>
        [HttpGet("summary/{orgId}")]
        public async Task<IActionResult> GetPatchQueueKPI(string orgId)
        {
            if (string.IsNullOrWhiteSpace(orgId))
                return BadRequest(new { message = "orgId is required." });

            var result = await mongoDAL.GetPatchQueueSummaryAsync(orgId);

            if (result == null)
                return NotFound(new { message = $"No patch queue data found for orgId: {orgId}" });
            else
                result.MissingPatches = await mongoDAL.GetMissingPatchesBreakdownAsync(orgId);

            return Ok(result);
        }
    }
}
