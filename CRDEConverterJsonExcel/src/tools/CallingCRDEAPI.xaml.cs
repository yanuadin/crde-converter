using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

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

            t5_cb_environment.ItemsSource = config.getEnvironmentList();
        }

        public void refreshConfig()
        {
            config = new CRDE();
            t5_cb_environment.ItemsSource = config.getEnvironmentList();
        }

        private void t5_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONRequestItems = GeneralMethod.browseFile("json", true);
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
                lb_JSONRequestItems = GeneralMethod.browseFolder("json");
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
            Converter converter = new Converter();

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
                        foreach (Item request in filteredSelected)
                        {
                            string responseJSONText = await Api.PostApiDataAsync(endpoint, JObject.Parse(request.JSON), request.FileName);
                            if(responseJSONText != "")
                            {
                                JObject responseJSON = JObject.Parse(responseJSONText);
                                string responseJSONTextIndent = JsonConvert.SerializeObject(responseJSON, Formatting.Indented);
                                string responseName = responseJSON.First.First.First.First["InquiryCode"].ToString();

                                // Save Response to JSON File
                                string fileOutputPath = converter.saveTextFile(savePath + @"\" + responseName + ".json", responseJSONTextIndent, "res");
                                lb_JSONResponseItems.Add(new Item { FileName = responseName, FilePath = fileOutputPath, JSON = responseJSONText, IsSelected = false });
                            }
                        }
                        t5_lb_ResponseList.ItemsSource = lb_JSONResponseItems;
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
                        converter.convertJSONToExcel(package, file.JSON, iterator++);

                    string fname = filteredSelected.Count > 1 ? "MultipleFiles" : filteredSelected.First<Item>().FileName;

                    // Save Excel file
                    string savePath = GeneralMethod.saveFileDialog("excel", fname + "-res.xlsx");

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
