using MongoDB.Driver;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;
using EndAgent_API.Models.Dto__Data_Tranfer_Objects_;
using EndAgent_API.MongoDB;
using EndAgent_API.Models;


public class PatchSyncService
{
    private readonly IMongoCollection<CentralOSPatches> _osCollection;
    private readonly IMongoCollection<CentralSoftwarePatches> _softwareCollection;
    private readonly IMongoCollection<CISDTO> _orgFamilyCollection;
    private readonly IMongoCollection<CISHistory> _cisHistoryCollections;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _patchFolder;
    public PatchSyncService(
        IMongoDatabase database,
        IConfiguration configuration)
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _httpClient = new HttpClient(handler);

        _configuration = configuration;

        _osCollection = database.GetCollection<CentralOSPatches>("OSCentralCollection");
        _softwareCollection = database.GetCollection<CentralSoftwarePatches>("SoftwareCentralCollection");
        _orgFamilyCollection = database.GetCollection<CISDTO>("OrgSelectedFamilies");
        _patchFolder = configuration["PatchSync:PatchFolder"];
        if (!Directory.Exists(_patchFolder))
            Directory.CreateDirectory(_patchFolder);
        _cisHistoryCollections = database.GetCollection<CISHistory>("CISHistory");

    }

    public async Task<PatchResponseDto> SyncTenantAsync(string orgid, List<string>? familyIds = null)
    {
        // Use passed familyIds if not null/empty, otherwise fetch from Mongo
        var families = (familyIds != null && familyIds.Any())
            ? familyIds
            : await GetSelectedFamiliesFromMongo(orgid);

        if (!families.Any())
            return new PatchResponseDto(); // return empty instead of null

        string familyString = string.Join(",", families);

        // Step 1: Get existing patches state for this org
        var existingPatches = await GetOSPatchesByOrgId(orgid,"os");
        var existingSoftwares = await GetOSPatchesByOrgId(orgid, "app");
        // Step 2: Build request body
        var requestBody = new PatchRequestDto
        {
            FamilyIds = familyString,
            ExistingPatches = existingPatches,
            ExistingSoftwares = existingSoftwares
        };

        string baseUrl = _configuration["CentralApi:BaseUrl"];
        string endpoint = _configuration["CentralApi:GetPatchbyfamily"];
        //string url = $"{baseUrl}{endpoint}?familyIds={familyString}";
        string url = $"{baseUrl}{endpoint}";
        // Step 3: POST instead of GET
        // Step 3: Explicitly serialize and send
        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //Console.WriteLine($"URL: {url}");
        //Console.WriteLine($"Body: {json}");

        var httpResponse = await _httpClient.PostAsync(url, content);
        var response = await httpResponse.Content.ReadFromJsonAsync<PatchResponseDto>()
                       ?? new PatchResponseDto();

        // OS Patches
        if (response?.OSPatches != null)
        {
            foreach (var patch in response.OSPatches)
            {
                try
                {
                    patch.OrgId = orgid;
                    await UpsertPatchAsync(_osCollection, patch);

                    await DownloadPatchFileAndUpdatePath(_osCollection, patch);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OS Patch Error ({patch?.UpdateId}): {ex.Message}");
                }
            }
        }

        // Software Patches
        if (response?.SoftwarePatches != null)
        {
            foreach (var patch in response.SoftwarePatches)
            {
                try
                {
                    patch.OrgId = orgid;
                    await UpsertPatchAsync(_softwareCollection, patch);

                    await DownloadPatchFileAndUpdatePath(_softwareCollection, patch);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Software Patch Error ({patch?.UpdateId}): {ex.Message}");
                }
            }
        }

        return response;
    }
    private async Task<List<string>> GetSelectedFamiliesFromMongo(string orgid)
    {
        var filter = Builders<CISDTO>.Filter.Eq(x => x.OrgId, orgid);
        var orgData = await _orgFamilyCollection.Find(filter).FirstOrDefaultAsync();

        if (orgData == null)
            return new List<string>();

        var families = new List<string>();

        if (!string.IsNullOrEmpty(orgData.OsPatchesSelected))
            families.AddRange(orgData.OsPatchesSelected.Split(',', StringSplitOptions.RemoveEmptyEntries));

        if (!string.IsNullOrEmpty(orgData.SoftwarePatchesSelected))
            families.AddRange(orgData.SoftwarePatchesSelected.Split(',', StringSplitOptions.RemoveEmptyEntries));

        return families;
    }
    private async Task UpsertPatchAsync<T>(IMongoCollection<T> collection, T patch)
    {
        try
        {
            dynamic patchObj = patch;

            //// Remove _id so MongoDB doesn't try to insert duplicate
            //if (patchObj.Id != null)
            //    patchObj.Id = null;

            var filter = Builders<T>.Filter.Eq("update_id", patchObj.UpdateId);

            var options = new ReplaceOptions { IsUpsert = true };

            await collection.ReplaceOneAsync(filter, patch,options);
        }
        catch (Exception ex)
        {
            string _error = ex.Message;
        }
    }

    private async Task DownloadPatchFileAndUpdatePath<T>(IMongoCollection<T> collection, dynamic patch)
    {
        if (patch == null || string.IsNullOrWhiteSpace(patch.PatchPath))
            return;

        var localPaths = new List<string>();

        // Split by comma and trim whitespace from each path
        var patchPathArray = patch.PatchPath.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var patchPath in patchPathArray)
        {
            var fileName = Path.GetFileName(patchPath);
            var localPath = Path.Combine(_patchFolder, fileName);

            // Download or copy the file if it doesn't exist locally
            if (!File.Exists(localPath))
            {
                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);

                // ✅ FIX: Encode the individual patchPath, not the entire patch.PatchPath
                var encodedPath = Uri.EscapeDataString(patchPath.Trim());

                string baseUrl = _configuration["CentralApi:BaseUrl"];
                string download = _configuration["CentralApi:Download"];
                string url = $"{baseUrl}{download}?fullPath={encodedPath}";

                var bytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(localPath, bytes);
            }

            localPaths.Add(localPath);
        }

        // ✅ FIX: Join with comma - works for both single and multiple values
        patch.PatchPath = string.Join(",", localPaths);

        // Upsert the patch back to MongoDB
        var filter = Builders<T>.Filter.Eq("update_id", patch.UpdateId);
        var options = new ReplaceOptions { IsUpsert = true };
        patch.Id = null;

        await collection.ReplaceOneAsync(filter, patch, options);
    }

    //Alert for New Patches Updates
    //type indicate which one ospatch or software
    public async Task<MongoError> SendAlerttoAllPatchesByUpdateId(string _updateId, string type)
    {
        if (type == "os")
        {
            var _filter = Builders<CentralOSPatches>.Filter.Eq("update_id", _updateId);
            var _notifyUpdate = Builders<CentralOSPatches>.Update.Set("is_stale", true);

            var result = await _osCollection.UpdateManyAsync(_filter, _notifyUpdate);

            return new MongoError()
            {
                Status = result.ModifiedCount > 0,
                Message = result.ModifiedCount > 0 ?
                    $"Updated {result.ModifiedCount} OS Patches" :
                    "No OS Patches found with this update id."
            };
        }
        if(type=="app")
        {
            var _filter = Builders<CentralSoftwarePatches>.Filter.Eq("update_id", _updateId);
            var _notifyUpdate = Builders<CentralSoftwarePatches>.Update.Set("is_stale", true);

            var result = await _softwareCollection.UpdateManyAsync(_filter, _notifyUpdate);

            return new MongoError()
            {
                Status = result.ModifiedCount > 0,
                Message = result.ModifiedCount > 0 ?
                    $"Updated {result.ModifiedCount} Software" :
                    "No app found with this app id."
            };
        }

        return new MongoError() { Status = false, Message="failed" };
    }

    public async Task<MongoError> DeletePatchesAndAppsByUpdateId(string _updateId, string type)
    {
        try
        {
            if (type == "os")
            {
                var _filter = Builders<CentralOSPatches>.Filter.Eq("update_id", _updateId);
                //var osPatchProjection = Builders<CentralOSPatches>.Projection
                //    .Include("update_id")
                //    .Include("file_name")
                //    .Include("patch_path")
                //    .Include("_id");

                //var osPatchList = await _osCollection.Find(_filter)
                //                                     .Project<OSPatchesDeleteDTO>(osPatchProjection)
                //                                     .ToListAsync();
                var osPatchList = await _osCollection.Find(_filter).ToListAsync();

                if (osPatchList.Count == 0)
                    return new MongoError() { Status = false, Message = "No OS patches found for the given update_id." };

                var deletePatchResult = await _osCollection.DeleteManyAsync(_filter);

                if (osPatchList.Count == deletePatchResult.DeletedCount)
                {
                    foreach (var ospatch in osPatchList)
                    {
                        CISHistory _history = new CISHistory()
                        {
                            CISType = "ospatches",
                            UpdateId = ospatch.UpdateId,
                            Name = ospatch.Title,
                            KBNumber = ospatch.KBNumber,
                            BitRate = ospatch.BitRate,
                            Platform = ospatch.Platform,
                            Version = ospatch.Version,
                            Vendor = ospatch.Vendor,
                            Category = ospatch.Category,
                            FileName = ospatch.FileName,
                            FilePath = ospatch.PatchPath
                        };
                        await _cisHistoryCollections.InsertOneAsync(_history);
                    }

                    var uniqueOsPatches = osPatchList
                        .Where(p => !string.IsNullOrEmpty(p.PatchPath))
                        .GroupBy(p => p.PatchPath)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var patch in uniqueOsPatches)
                    {
                        if (File.Exists(patch.PatchPath))
                        {
                            File.Delete(patch.PatchPath);
                        }
                    }
                    return new MongoError() { Status = true, Message = "OS patches deleted successfully." };
                }

                return new MongoError() { Status = false, Message = "Some OS patches could not be deleted from DB." };
            }

            if (type == "app")
            {
                var _filter = Builders<CentralSoftwarePatches>.Filter.Eq("update_id", _updateId);
                //var appPatchProjection = Builders<CentralSoftwarePatches>.Projection
                //    .Include("update_id")
                //    .Include("file_name")
                //    .Include("patch_path")
                //    .Include("_id");

                //var appPatchList = await _softwareCollection.Find(_filter)
                //                                       .Project<OSPatchesDeleteDTO>(appPatchProjection)
                //                                       .ToListAsync();
                var appPatchList = await _softwareCollection.Find(_filter)
                                                       .ToListAsync();

                if (appPatchList.Count == 0)
                    return new MongoError() { Status = false, Message = "No app patches found for the given update_id." };

                var deleteAppResult = await _softwareCollection.DeleteManyAsync(_filter);

                if (appPatchList.Count == deleteAppResult.DeletedCount)
                {
                    foreach (var ospatch in appPatchList)
                    {
                        CISHistory _history = new CISHistory()
                        {
                            CISType = "app",
                            UpdateId = ospatch.UpdateId,
                            Name = ospatch.SoftwareName,
                            KBNumber = ospatch.PatchNumber,
                            BitRate = ospatch.BitRate,
                            Platform = ospatch.Platform,
                            Version = ospatch.Version,
                            Vendor = ospatch.Vendor,
                            Category = ospatch.Category,
                            FileName = ospatch.FileName,
                            FilePath = ospatch.PatchPath
                        };
                        await _cisHistoryCollections.InsertOneAsync(_history);
                    }

                    var uniqueAppPatches = appPatchList
                        .Where(p => !string.IsNullOrEmpty(p.PatchPath))
                        .GroupBy(p => p.PatchPath)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var patch in uniqueAppPatches)
                    {
                        if (File.Exists(patch.PatchPath))
                        {
                            File.Delete(patch.PatchPath);
                        }
                    }
                    return new MongoError() { Status = true, Message = "App patches deleted successfully." };
                }

                return new MongoError() { Status = false, Message = "Some app patches could not be deleted from DB." };
            }

            return new MongoError() { Status = false, Message = $"Unknown type '{type}'. Expected 'os' or 'app'." };
        }
        catch (MongoException ex)
        {
            return new MongoError() { Status = false, Message = $"MongoDB error: {ex.Message}" };
        }
        catch (IOException ex)
        {
            return new MongoError() { Status = false, Message = $"File deletion error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new MongoError() { Status = false, Message = $"Unexpected error: {ex.Message}" };
        }
    }

    //for notification
    public async Task<bool> PatchesAutoSync(string orgId)
    {
        var _filter = Builders<CentralOSPatches>.Filter.Eq("OrgId", orgId)
                    & Builders<CentralOSPatches>.Filter.Eq("is_stale", true)
                    & Builders<CentralOSPatches>.Filter.Eq("is_deleted", false);

        return await _osCollection.Find(_filter).AnyAsync();
    }

    //which fetch list of updated_id, is_stale
    public async Task<List<OSPatchesUpdateIsStaleDTO>> GetOSPatchesByOrgId(string orgId, string type)
    {
        if (type == "os")
        {
            var _filter = Builders<CentralOSPatches>.Filter.Eq("OrgId", orgId);
            var _projection = Builders<CentralOSPatches>.Projection
                .Include("update_id")
                .Include("is_stale")
                .Exclude("_id"); // exclude _id unless you need it

            var result = await _osCollection
                .Find(_filter)
                .Project<OSPatchesUpdateIsStaleDTO>(_projection)
                .ToListAsync();

            return result;
        }
        if(type=="app")
        {
            var _filter = Builders<CentralSoftwarePatches>.Filter.Eq("OrgId", orgId);
            var _projection = Builders<CentralSoftwarePatches>.Projection
                .Include("update_id")
                .Include("is_stale")
                .Exclude("_id"); // exclude _id unless you need it

            var result = await _softwareCollection
                .Find(_filter)
                .Project<OSPatchesUpdateIsStaleDTO>(_projection)
                .ToListAsync();

            return result;
        }

        return null;
    }



}
