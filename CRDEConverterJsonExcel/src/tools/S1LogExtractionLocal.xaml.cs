using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Net.Mime;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for S1LogExtractionLocal.xaml
    /// </summary>
    public partial class S1LogExtractionLocal : UserControl
    {
        private CRDE config = new CRDE();
        private ObservableCollection<Item> lb_LogFiles = new ObservableCollection<Item>();
        private ObservableCollection<Item> lb_JSONFiles = new ObservableCollection<Item>();

        public S1LogExtractionLocal()
        {
            InitializeComponent();

            t4_cb_process_code.ItemsSource = config.getProcessCode().ToList();
        }

        private void t4_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_LogFiles = GeneralMethod.browseFolder("completed");
                t4_lb_LogList.ItemsSource = lb_LogFiles;
                t4_tb_folder.Text = string.Join(@"\", lb_LogFiles.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open folder");
            }
        }

        private void t4_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_LogFiles = new ObservableCollection<Item>();
            t4_lb_LogList.ItemsSource = lb_LogFiles;
            t4_tb_folder.Text = "";
            t4_cb_selectAll.IsChecked = false;
        }

        private void t4_cb_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_LogFiles, t4_cb_selectAll);
        }

        private void t4_btn_ExtractLog_Click(object sender, RoutedEventArgs e)
        {
            if (t4_cb_process_code.SelectedValue == null)
            {
                MessageBox.Show("[WARNING]: Please select the process code");
            }
            else
            {
                Converter converter = new Converter();
                string processCode = t4_cb_process_code.SelectedValue.ToString();
                List<Item> filteredSelected = lb_LogFiles.Where(item => item.IsSelected).ToList();

                if (filteredSelected.Count > 0)
                {
                    string savePath = GeneralMethod.saveFolderDialog();

                    if (savePath != "")
                    {
                        // Clear List
                        lb_JSONFiles = new ObservableCollection<Item>();
                        t4_lb_JSONList.ItemsSource = lb_LogFiles;
                        int successCount = 0;

                        foreach (Item file in filteredSelected)
                        {
                            try
                            {
                                string filePath = file.FilePath;
                                string fileName = file.FileName;
                                JArray contentFile = new JArray();

                                using (TextReader reader = new StreamReader(filePath))
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
                                        if (content["PROCESSCODE"].ToString() == processCode)
                                        {
                                            for (int i = 0; i < content["IO_JSON"].Count(); i++)
                                            {
                                                // Save Response to JSON File
                                                string typeJSON = i == 0 ? "req" : "res";
                                                string additionalField = typeJSON == "req" ? "Request" : "Response";
                                                string formattingIndentJSON = JsonConvert.SerializeObject(content["IO_JSON"][i], Formatting.Indented);
                                                string JSONFileName = content["REQUESTID"].ToString();
                                                string saveFilePath = converter.saveTextFile(savePath + @"\" + JSONFileName + ".json", formattingIndentJSON, typeJSON);
                                                lb_JSONFiles.Add(new Item { FilePath = saveFilePath, FileName = content["REQUESTID"].ToString(), AdditionalField = additionalField });
                                                successCount++;
                                            }
                                        }
                                    }
                                    t4_lb_JSONList.ItemsSource = lb_JSONFiles;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("[FAILED]: " + ex.Message);
                                break;
                            }
                        }

                        MessageBox.Show($"[SUCCESS]: {successCount} file was saved successfully");
                    }
                } else
                {
                    MessageBox.Show("[FAILED]: Please select at least one item");
                }
            }
        }

        private void t4_btn_ClearJSONListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONFiles = new ObservableCollection<Item>();
            t4_lb_JSONList.ItemsSource = lb_JSONFiles;
        }
    }
}
