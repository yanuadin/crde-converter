using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CRDEConverterJsonExcel.core
{
    class Api
    {
        public static async Task<string> GetApiDataAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the API
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> PostApiDataAsync(string url, object data)
        {
            using (HttpClient client = new HttpClient())
            {
                // Serialize the data to JSON
                string jsonData = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Send a POST request to the API
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async void postRequestCRDE(string selectedEndpoint, string json, string saveFileNameResponse, int iterator)
        {
            saveFileNameResponse = saveFileNameResponse + "_response";
            Converter converter = new Converter();

            // Parse JSON
            JObject jsonObject = JObject.Parse(json);

            // Data to send in the POST request
            try
            {
                using (var package = new ExcelPackage())
                {
                    // Call the API and get the response
                    string responseJsonText = await Api.PostApiDataAsync(selectedEndpoint, jsonObject);
                    JObject parseResponseJson = JObject.Parse(responseJsonText);
                    string responseJsonIndent = JsonConvert.SerializeObject(parseResponseJson, Formatting.Indented);

                    // Save Response to JSON File
                    converter.saveTextFile(@"E:\Yanu\temp_sample\" + saveFileNameResponse + ".json", responseJsonIndent, "res");

                    // Convert Response to Excel
                    converter.convertJSONToExcel(package, responseJsonText, iterator);

                    // Save Excel file
                    string excelFilePath = @"E:\Yanu\temp_sample\" + saveFileNameResponse + "-res.xlsx";
                    package.SaveAs(new FileInfo(excelFilePath));

                    // Add to List Box Response
                    //X: lb_responseList.Items.Add(new Item { fileName = saveFileNameResponse, json = json, isSelected = false });

                    MessageBox.Show("[SUCCESS]: [" + saveFileNameResponse + @"] Save Response was successful! File saved to \output\json\response and \output\excel\response");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"[API_FAILED]: {ex.StatusCode} : {ex.Message}", "Error");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"[API_FAILED]: An error occurred: {ex.Message}", "Error");

            }
        }
    }
}
