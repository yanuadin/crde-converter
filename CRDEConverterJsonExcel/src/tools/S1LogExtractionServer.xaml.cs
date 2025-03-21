using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CRDEConverterJsonExcel.objectClass;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO.Compression;
using CRDEConverterJsonExcel.controller;
using System.Diagnostics;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for S1LogExtractionServer.xaml
    /// </summary>
    public partial class S1LogExtractionServer : UserControl
    {
        private S1LogController s1LogController = new S1LogController();
        private ProcessCodeController processCodeController= new ProcessCodeController();
        private ObservableCollection<Item> lb_ServerLogFiles = new ObservableCollection<Item>();
        private ObservableCollection<Item> lb_DownloadLogFiles = new ObservableCollection<Item>();
        private ObservableCollection<Item> lb_JSONFiles = new ObservableCollection<Item>();
        private bool isInterrupted = false;
        AmazonS3Client s3Client;

        public S1LogExtractionServer()
        {
            InitializeComponent();

            t7_cb_process_code.ItemsSource = processCodeController.getProcessCodeList();
            t7_cb_environment.ItemsSource = s1LogController.getS1LogList();
        }

        private async void t7_btn_LoadFromServer_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t7_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (t7_cb_environment.SelectedValue != null)
                {
					// AWS S3 Configuration
					S1Log s1Log = s1LogController.getS1Log("Name", t7_cb_environment.Text)?.ToObject<S1Log>();
                    
                    if (s1Log != null)
                    {
                        // Flush Server List
                        lb_ServerLogFiles.Clear();

                        // Arrange bucketName and folderPath
                        // ex : /bucket-name/path/
                        string[] splitDirectoryS1 = s1Log.DirectoryS1.Split(@"/");
                        if (splitDirectoryS1[0] == "")
                            splitDirectoryS1 = splitDirectoryS1.Skip(1).ToArray();

                        string bucketName = splitDirectoryS1[0] == "" ? splitDirectoryS1[1] : splitDirectoryS1[0];
                        string folderPath = string.Join(@"/", splitDirectoryS1[1..^0]);

                        // Config and connect to Amazon S3 Client
						var amazonS3Config = new AmazonS3Config { ServiceURL = s1Log.HostName };
                        s3Client = new AmazonS3Client(s1Log.AccessKeyID, s1Log.SecretAccessKey, amazonS3Config);

						// Set up request parameter object
						ListObjectsV2Request request = new ListObjectsV2Request
                        {
                            BucketName = bucketName,
                            Prefix = folderPath, // Specify the folder path
                        };
                        
                        // Get response object
                        ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

                        // Initialize progress reporting
                        var progress = new Progress<int>(value =>
                        {
                            t7_progressBar.Value = 90;
                            t7_progressText.Text = $"{value}";
                        });

                        // Initialize Progress Bar
                        t7_progressBar.Value = 0;
                        t7_progressText.Text = "0/0";
                        t7_progressBar.Visibility = Visibility.Visible;
                        t7_progressText.Visibility = Visibility.Visible;
                        t7_btn_StopProgressBar.Visibility = Visibility.Visible;
                        
                        int completedItems = 0;
                        foreach (S3Object entry in response.S3Objects)
                        {
                            if (isInterrupted)
                                break;

                            if (entry.Key != folderPath)
                            {
                                string filePathWithoutName = string.Join(@"/", entry.Key.Split(@"/")[0..^1]);
                                string fileName = string.Join(@".", entry.Key.Split(@"/").Last().Split(".")[0..^1]);
                                string fileExtension = entry.Key.Split(@"/").Last().Split(".").Last();
                                
                                if (filePathWithoutName.ToLower().Equals(folderPath.ToLower()) && fileName != "" && fileExtension != "" && (fileExtension.ToLower() == "completed" || fileExtension.ToLower() == "zip"))
                                {
                                    string content = await ReadObjectContentAsync(bucketName, entry.Key);
                                    if (fileExtension == "zip")
                                    {
                                        ObservableCollection<Item> ZIPFiles = await ReadZipFileContentAsync(bucketName, entry.Key);
                                        foreach (Item file in ZIPFiles)
                                        {
                                            file.FilePath = entry.Key;
                                            file.AdditionalField = fileName + "." + fileExtension;
                                            file.CreatedDate = entry.LastModified.ToString();
                                            lb_ServerLogFiles.Add(file);

                                            // Update progress
                                            completedItems++;
                                            ((IProgress<int>)progress).Report(completedItems);
                                        }
                                    } else
                                    {
                                        // Update progress
                                        completedItems++;
                                        ((IProgress<int>)progress).Report(completedItems);
                                        lb_ServerLogFiles.Add(new Item { FileName = fileName, FileExt = fileExtension, FilePath = entry.Key, FileContent = content, CreatedDate = entry.LastModified.ToString(), IsSelected = false });
                                    }
                                }
                            }
                        }
                        t7_dg_ServerLogList.ItemsSource = lb_ServerLogFiles;
                        MessageBox.Show("[SUCCES]: Log has been loaded successfully");

					} else
						MessageBox.Show("[WARNING]: Environment not found");
				}
				else
                    MessageBox.Show("[WARNING]: Please select environment");
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show("Error encountered on server." + Environment.NewLine + $"Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unknown error encountered on server." + Environment.NewLine + $"Message: {ex.Message}");
            }
            finally
            {
                // Re-enable the cursor and reset it to the default
                t7_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t7_progressBar.Visibility = Visibility.Hidden;
                t7_progressText.Visibility = Visibility.Hidden;
                t7_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private void t7_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            isInterrupted = true;
        }

        private async void t7_btn_StopProgressBar_MouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private async void t7_btn_StopProgressBar_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        private void t7_btn_ClearServerLogListListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_ServerLogFiles = new ObservableCollection<Item>();
            t7_dg_ServerLogList.ItemsSource = lb_ServerLogFiles;
            t7_cb_selectServerAllLogList.IsChecked = false;
        }

        private void t7_cb_selectServerAllLogList_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_ServerLogFiles, t7_cb_selectServerAllLogList);
        }

        private async void t7_btn_DownloadFromServer_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t7_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                List<Item> filteredSelected = lb_ServerLogFiles.Where(item => item.IsSelected).ToList();
                int filteredCount = filteredSelected.Count;
                Converter converter = new Converter();

                if (filteredCount > 0)
                {
                    string savePath = GeneralMethod.saveFolderDialog();

                    if (savePath != "")
                    {
                        // Flush response list item
                        lb_DownloadLogFiles.Clear();
                        t7_tb_DownloadOutput.Clear();

                        // Initialize progress reporting
                        var progress = new Progress<int>(value =>
                        {
                            t7_progressBar.Value = (int)((double)value / filteredCount * 100);
                            t7_progressText.Text = $"{value}/{filteredCount}";
                        });

                        // Initialize Progress Bar
                        t7_progressBar.Value = 0;
                        t7_progressText.Text = "0/0";
                        t7_progressBar.Visibility = Visibility.Visible;
                        t7_progressText.Visibility = Visibility.Visible;
                        t7_btn_StopProgressBar.Visibility = Visibility.Visible;

                        int completedItems = 0;
                        foreach (Item item in filteredSelected)
                        {
                            if (isInterrupted)
                                break;

                            string saveFilePath = savePath + @"\" + item.FileName + "." + item.FileExt;
                            converter.saveTextFile(saveFilePath, item.FileContent);
                            lb_DownloadLogFiles.Add(new Item { FileName = item.FileName, FileExt = item.FileExt, FilePath = saveFilePath, CreatedDate = item.CreatedDate, AdditionalField = item.AdditionalField, IsSelected = false });

                            // Update progress
                            completedItems++;
                            ((IProgress<int>)progress).Report(completedItems);
                        }
                        t7_dg_DownloadLogList.ItemsSource = lb_DownloadLogFiles;
                        t7_tb_DownloadOutput.Text = savePath;
                        MessageBox.Show("[SUCCES]: Log has been downloaded successfully");
                    }
                }
                else
                    MessageBox.Show("[WARNING]: No one item were selected");
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
            finally
            {
                // Re-enable the cursor and reset it to the default
                t7_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t7_progressBar.Visibility = Visibility.Hidden;
                t7_progressText.Visibility = Visibility.Hidden;
                t7_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private void t7_btn_ClearDownloadLogListListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_DownloadLogFiles = new ObservableCollection<Item>();
            t7_dg_DownloadLogList.ItemsSource = lb_DownloadLogFiles;
        }

        private void t7_btn_ExtractLog_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t7_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (t7_cb_process_code.SelectedValue == null)
                    MessageBox.Show("[WARNING]: Please select the process code");
                else
                {
                    Converter converter = new Converter();
                    string processCode = t7_cb_process_code.SelectedValue.ToString();
                    List<Item> filteredSelected = lb_ServerLogFiles.Where(item => item.IsSelected).ToList();
                    int filteredCount = filteredSelected.Count;

                    if (filteredCount > 0)
                    {
                        // Initialize progress reporting
                        var progress = new Progress<int>(value =>
                        {
                            t7_progressBar.Value = (int)((double)value / filteredCount * 100);
                            t7_progressText.Text = $"{value}/{filteredCount}";
                        });

                        // Initialize Progress Bar
                        t7_progressBar.Value = 0;
                        t7_progressText.Text = "0/0";
                        t7_progressBar.Visibility = Visibility.Visible;
                        t7_progressText.Visibility = Visibility.Visible;
                        t7_btn_StopProgressBar.Visibility = Visibility.Visible;

                        string savePath = GeneralMethod.saveFolderDialog();

                        if (savePath != "")
                        {
                            // Clear List
                            lb_JSONFiles = new ObservableCollection<Item>();
                            t7_lb_JSONList.ItemsSource = lb_JSONFiles;
                            int successCount = 0;
                            int completedItems = 0;

                            foreach (Item file in filteredSelected)
                            {
                                if (isInterrupted)
                                    break;

                                string filePath = file.FilePath;
                                string fileName = file.FileName;
                                JArray contentFile = new JArray();

                                using (StringReader reader = new StringReader(file.FileContent))
                                {
                                    string line;
                                    int lineNumber = 1;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        // RUNID
                                        if (lineNumber % 3 == 1)
                                        {
                                            string[] splitLine = line.Split("#");
                                            JObject arrangeRunId = new JObject();
                                            foreach (string runId in splitLine)
                                            {
                                                string[] runIdSplit = runId.Split(":");
                                                if (runIdSplit.Count() > 1)
                                                {
                                                    arrangeRunId[runIdSplit[0]] = runIdSplit[1];
                                                }
                                            }
                                            contentFile.Add(arrangeRunId);
                                        }

                                        // REQ

                                        // IO
                                        if (lineNumber % 3 == 0)
                                        {
                                            string[] splitLine = line.Split('\t');

                                            if (splitLine.Count() > 1)
                                            {
                                                // Get JSON String
                                                JArray jsonColletion = new JArray();
                                                for (int i = 2; i < splitLine.Count(); i++)
                                                {
                                                    if (splitLine[i] != "")
                                                        jsonColletion.Add(splitLine[i]);
                                                }
                                                contentFile[lineNumber / 3 - 1]["IO_JSON"] = jsonColletion;
                                            }
                                        }

                                        lineNumber++;
                                    }

                                    // Convert IO_JSON to JSON File
                                    foreach (JObject content in contentFile)
                                    {
                                        if (content["PROCESSCODE"] != null && content["PROCESSCODE"].ToString() == processCode)
                                        {
                                            for (int i = 0; i < content["IO_JSON"].Count(); i++)
                                            {
                                                // Save Response to JSON File
                                                string typeJSON = i == 0 ? "req" : "res";
                                                string additionalField = typeJSON == "req" ? "Request" : "Response";
                                                JObject json = JObject.Parse(content["IO_JSON"][i].ToString());
                                                string formattingIndentJSON = JsonConvert.SerializeObject(json, Formatting.Indented);
                                                string JSONFileName = content["REQUESTID"].ToString();
                                                string saveFilePath = converter.saveTextFile(savePath + @"\" + JSONFileName + ".json", formattingIndentJSON, typeJSON);
                                                lb_JSONFiles.Add(new Item { FilePath = saveFilePath, FileName = content["REQUESTID"].ToString(), AdditionalField = additionalField });
                                                successCount++;
                                            }
                                        }
                                    }
                                }

                                // Update progress
                                completedItems++;
                                ((IProgress<int>)progress).Report(completedItems);
                            }
                            t7_lb_JSONList.ItemsSource = lb_JSONFiles;

                            MessageBox.Show($"[SUCCESS]: {successCount} file was saved successfully");
                        }
                    }
                    else
                        MessageBox.Show("[FAILED]: Please select at least one item");
                }
            }
            finally
            {
                // Re-enable the cursor and reset it to the default
                t7_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t7_progressBar.Visibility = Visibility.Hidden;
                t7_progressText.Visibility = Visibility.Hidden;
                t7_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private void t7_btn_ClearJSONListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONFiles = new ObservableCollection<Item>();
            t7_lb_JSONList.ItemsSource = lb_JSONFiles;
        }

        private async Task<string> ReadObjectContentAsync(string bucketName, string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (var response = await s3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var reader = new StreamReader(responseStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<ObservableCollection<Item>> ReadZipFileContentAsync(string bucketName, string key)
        {
            ObservableCollection<Item> ZIPFiles = new ObservableCollection<Item>();

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (var response = await s3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var memoryStream = new MemoryStream())
            {
                // Copy the S3 object stream to a memory stream
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset the stream position

                // Verify the file is a valid ZIP file
                if (IsZipFile(memoryStream))
                {
                    // Read the ZIP file contents
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            string content = "";
                            using (var entryStream = entry.Open())
                            using (var reader = new StreamReader(entryStream))
                            {
                                content = await reader.ReadToEndAsync();
                            }

                            string fileName = entry.FullName.Split(@"/").Last().Split(".").First();
                            string fileExtension = entry.FullName.Split(@"/").Last().Split(".").Last();
                            if (fileExtension.ToUpper() == "COMPLETED")
                                ZIPFiles.Add(new Item { FileName = fileName, FileExt = fileExtension, FileContent = content, IsSelected = false });
                        }
                    }
                }
            }

            return ZIPFiles;
        }

        private bool IsZipFile(Stream stream)
        {
            try
            {
                // Check if the stream is a valid ZIP file
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    return true;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        private void t7_tb_SearchServerLogList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = lb_ServerLogFiles.Where(file => file.FileName.Contains(t7_tb_SearchServerLogList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t7_dg_ServerLogList.ItemsSource = search;
        }

        private void t7_tb_SearchDownloadLogList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = lb_DownloadLogFiles.Where(file => file.FileName.Contains(t7_tb_SearchDownloadLogList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t7_dg_DownloadLogList.ItemsSource = search;
        }

        private void t7_tb_SearchJSONList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = lb_JSONFiles.Where(file => file.FileName.Contains(t7_tb_SearchJSONList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t7_lb_JSONList.ItemsSource = search;
        }

        private void t7_dg_ServerLogList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t7_dg_ServerLogList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }

        private void t7_dg_DownloadLogList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t7_dg_DownloadLogList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }
    }
}
