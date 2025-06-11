using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace CRDEConverterJsonExcel.core
{
    class Api
    {
        public static async Task<APIResponse> GetApiDataAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the API
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
                string responseData = await response.Content.ReadAsStringAsync();

                return new APIResponse { success = true, message = "SUCCESS", data = responseData };
            }
        }

        public static async Task<APIResponse> PostApiDataAsync(string url, object data)
        {
            using (HttpClient client = new HttpClient())
            {
                // Serialize the data to JSON
                string jsonData = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                try
                {
                    // Send a POST request to the API
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();

                    // Handle different status codes
                    if (response.IsSuccessStatusCode)
                    {
                        return new APIResponse { success = true, message = "SUCCESS", data = responseData };
                    }
                    else
                    {
                        return new APIResponse
                        {
                            success = false,
                            message = $"[ERROR] : {responseData}",
                            data = ""
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new APIResponse { success = false, message = $"[ERROR] : {ex.Message}", data = "", isInterrupted = true };
                }
            }
        }
    }
}
