using Azure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TEST_WebApiOsDetails.ITAM
{
    public class Validations
    {
        

        // output: one string
        public async Task<string> GetDataInOneString(string url, string header, string key)
        {

            try
            {
                
                if (header == null || key == null)
                {
                    return ("*error* in validation configuration.");
                }
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(header, key);
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        string assetExists = JsonConvert.DeserializeObject<string>(responseContent);

                        if (assetExists != null)
                        {
                            return assetExists.ToString();
                        }
                    }
                    return $"*error* : {response.StatusCode} : {response}";
                }

            }
            catch (Exception ex)
            {
                return "*error* in Validation" + ex.ToString();
            }
        }

        
    }
}
