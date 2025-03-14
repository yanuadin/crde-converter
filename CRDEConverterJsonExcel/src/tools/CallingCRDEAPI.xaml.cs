﻿using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for CallingCRDEAPI.xaml
    /// </summary>
    public partial class CallingCRDEAPI : UserControl
    {
        private CRDE config = new CRDE();
        private ObservableCollection<Item> lb_JSONRequestItems = new ObservableCollection<Item>();
        private ObservableCollection<Item> lb_JSONResponseItems = new ObservableCollection<Item>();

        public CallingCRDEAPI()
        {
            InitializeComponent();

            t5_cb_environment.ItemsSource = config.getEnvironmentNameList();
        }

        private void t5_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json", "txt" };
                lb_JSONRequestItems = GeneralMethod.browseFile(extension, true);
                t5_lb_RequestList.ItemsSource = lb_JSONRequestItems;
                t5_tb_folder.Text = string.Join(@"\", lb_JSONRequestItems.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open file");
            }
        }

        private void t5_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json", "txt" };
                lb_JSONRequestItems = GeneralMethod.browseFolder(extension);
                t5_lb_RequestList.ItemsSource = lb_JSONRequestItems;
                t5_tb_folder.Text = string.Join(@"\", lb_JSONRequestItems.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open folder");
            }
        }

        private void t5_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONRequestItems = new ObservableCollection<Item>();
            lb_JSONResponseItems = new ObservableCollection<Item>();
            t5_lb_RequestList.ItemsSource = lb_JSONRequestItems;
            t5_lb_ResponseList.ItemsSource = lb_JSONResponseItems;
            t5_tb_folder.Text = "";
            t5_tb_output_file.Text = "";
            t5_cb_selectAllRequest.IsChecked = false;
            t5_cb_selectAllResponse.IsChecked = false;
        }

        private void t5_cb_SelectAllRequest_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_JSONRequestItems, t5_cb_selectAllRequest);
        }

        private async void t5_btn_Run_Click(object sender, RoutedEventArgs e)
        {
            List<Item> filteredSelected = lb_JSONRequestItems.Where(item => item.IsSelected).ToList();

            if (filteredSelected.Count > 0)
            {
                if (t5_cb_environment.SelectedValue != null)
                {
                    string savePath = GeneralMethod.saveFolderDialog();

                    if (savePath != "")
                    {
                        // Flush response list item
                        lb_JSONResponseItems = new ObservableCollection<Item>();
                        t5_lb_ResponseList.ItemsSource = lb_JSONResponseItems;

                        // Send Request to API
                        string endpoint = config.getEnvironment(t5_cb_environment.Text)["ENDPOINT_REQUEST"].ToString();
                        
                        if(endpoint != "" && endpoint != null)
                        {
                            foreach (Item request in filteredSelected)
                            {
                                string fileExt = request.FilePath.Split("\\").Last().Split(".").Last();
                                if (fileExt == "txt")
                                {
                                    using (StringReader reader = new StringReader(request.FileContent))
                                    {
                                        string line;
                                        int lineNumber = 1;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            sendRequestToAPI(endpoint, line, savePath);
                                        }
                                    }
                                } else
                                {
                                    sendRequestToAPI(endpoint, request.FileContent, savePath);
                                }
                            }
                            t5_lb_ResponseList.ItemsSource = lb_JSONResponseItems;
                        } else
                        {
                            MessageBox.Show("[FAILED]: API address not found");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("[WARNING]: Please select environment");
                }
            } 
            else
            {
                MessageBox.Show("[WARNING]: No one item were selected");
            }
        }

        private async void sendRequestToAPI(string endpoint, string requestContent, string savePath)
        {
            Converter converter = new Converter();

            JObject jsonContent = JObject.Parse(requestContent);
            string jsonName = jsonContent.First.First.First.First["InquiryCode"].ToString();
            string responseJSONText = await Api.PostApiDataAsync(endpoint, jsonContent, jsonName);
            if (responseJSONText != "")
            {
                JObject responseJSON = JObject.Parse(responseJSONText);
                string responseJSONTextIndent = JsonConvert.SerializeObject(responseJSON, Formatting.Indented);

                // Save Response to JSON File
                string fileOutputPath = converter.saveTextFile(savePath + @"\" + jsonName + ".json", responseJSONTextIndent, "res");
                lb_JSONResponseItems.Add(new Item { FileName = jsonName, FilePath = fileOutputPath, FileContent = responseJSONText, IsSelected = false });
            }
        }

        private void t5_cb_SelectAllResponse_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_JSONResponseItems, t5_cb_selectAllResponse);
        }

        private void t5_btn_ConvertToExcel_Click(object sender, RoutedEventArgs e)
        {
            List<Item> filteredSelected = lb_JSONResponseItems.Where(item => item.IsSelected).ToList();
            Converter converter = new Converter();

            if (filteredSelected.Count > 0)
            {
                using (var package = new ExcelPackage())
                {
                    // Loop through the multiple files
                    int iterator = 0;
                    foreach (Item file in filteredSelected)
                        converter.convertJSONToExcel(package, file.FileContent, iterator++);

                    string fname = filteredSelected.Count > 1 ? "MultipleFiles" : filteredSelected.First<Item>().FileName;

                    // Save Excel file
                    string[] extension = { "excel" };
                    string savePath = GeneralMethod.saveFileDialog(extension, fname + "-res.xlsx");

                    if (savePath != "")
                    {
                        package.SaveAs(new FileInfo(savePath));
                        t5_tb_output_file.Text = savePath;
                        MessageBox.Show(@"[SUCCESS]: Conversion successful");
                    }
                }   
            }
            else
            {
                MessageBox.Show("[WARNING]: No one item were selected");
            }
        }
    }
}
