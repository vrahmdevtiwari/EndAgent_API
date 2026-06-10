using EndAgent_API.Models.Dto__Data_Tranfer_Objects_;
using Microsoft.AspNetCore.Mvc;
using TEST_WebApiOsDetails.data;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;

namespace TEST_WebApiOsDetails.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralRepoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly PatchSyncService _patchSyncService;

        public CentralRepoController(IConfiguration configuration, PatchSyncService patchSyncService)
        {
            _configuration = configuration;
            var handler = new HttpClientHandler() {
                ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler);
            _patchSyncService = patchSyncService;
        }
        [HttpGet("productfamilies")]
        public async Task<IActionResult> GetProductFamilies()
        {            
            try
            {
                string centralBaseUrl = _configuration["CentralApi:BaseUrl"];
                string endpoint = _configuration["CentralApi:GetOSProductList"];

                var response = await _httpClient.GetAsync(centralBaseUrl + endpoint);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                var json = await response.Content.ReadAsStringAsync();

                return Content(json, "application/json");
            }
            catch (Exception ex) 
            {
                string message = ex.Message;
                return null;
            }
        }
        [HttpGet("patchbyproductfamilies")]
        public async Task<IActionResult> Getpatchbyfamily(string familyIds)
        {
            string centralBaseUrl = _configuration["CentralApi:BaseUrl"];
            string endpoint = _configuration["CentralApi:GetPatchbyfamily"];
            string url = $"{centralBaseUrl}{endpoint}?familyIds={familyIds}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();

            return Content(json, "application/json");
        }
        [HttpPost]
        [Route("syncbyorgid/{orgid}")] 
        public async Task<IActionResult> SyncByOrgId(string orgid, [FromBody] List<string> familyIds)
        {
            var tokenOrgId = User.FindFirst("orgId")?.Value;

            if (tokenOrgId != orgid)
                return Unauthorized("Invalid tenant");

            if (familyIds == null || !familyIds.Any())
                return BadRequest(new { success = false, message = "No families selected for sync" });

            var patches = await _patchSyncService.SyncTenantAsync(orgid, familyIds);

            return Ok(new
            {
                success = true,
                message = "Sync completed successfully",
                osPatches = patches.OSPatches,
                softwarePatches = patches.SoftwarePatches
            });
        }


        [HttpPost("alert/{updateId}")]
        public async Task<IActionResult> UpdateClientPatches(string updateId)
        {
            if(string.IsNullOrEmpty(updateId))
                return BadRequest();

            var status = await _patchSyncService.SendAlerttoAllPatchesByUpdateId(updateId,"os");
            if(status.Status)
                return Ok(status.Status);

            return BadRequest(status.Status);
        }
        [HttpPost("appalert/{appId}")]
        public async Task<IActionResult> UpdateClientSoftware(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                return BadRequest();

            var status = await _patchSyncService.SendAlerttoAllPatchesByUpdateId(appId, "app");
            if (status.Status)
                return Ok(status.Status);

            return BadRequest(status.Status);
        }

        [HttpPost("deletepatch/{updateId}")]
        public async Task<IActionResult> DeleteClientPatches(string updateId)
        {
            if (string.IsNullOrEmpty(updateId))
                return BadRequest();

            var status = await _patchSyncService.DeletePatchesAndAppsByUpdateId(updateId, "os");
            if (status.Status)
                return Ok(status.Status);

            return BadRequest(status.Status);
        }
        [HttpPost("deleteapp/{updateId}")]
        public async Task<IActionResult> DeleteClientSoftware(string updateId)
        {
            if (string.IsNullOrEmpty(updateId))
                return BadRequest();

            var status = await _patchSyncService.DeletePatchesAndAppsByUpdateId(updateId, "app");
            if (status.Status)
                return Ok(status.Status);

            return BadRequest(status.Status);
        }

        //[HttpGet("patchalert/{orgid}")]
        //public async Task<IActionResult> OSPatchAlert(string orgid)
        //{
        //    var result = await _patchSyncService.GetOSPatchesByOrgId(orgid);

        //    bool isValid = await _patchSyncService.PatchesAutoSync(orgid);
        //    if (isValid)
        //        return Ok(isValid);

        //    return BadRequest();
        //}

    }
}
