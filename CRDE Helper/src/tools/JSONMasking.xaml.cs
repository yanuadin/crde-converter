using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.controller;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for JSONMasking.xaml
    /// </summary>
    public partial class JSONMasking : UserControl
    {
        private ObservableCollection<Item> JSONItemList = new ObservableCollection<Item>();
        private MaskingTemplateController maskingTemplateController = new MaskingTemplateController();
        private bool isInterrupted = false;

        public JSONMasking()
        {
            InitializeComponent();
            
            t3_cb_maskingTemplate.ItemsSource = maskingTemplateController.getMaskingTemplateList();
        }

        private void t3_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFile(extension, true);
                if (JSONItemList.Count > 0)
                {
                    t3_dg_JSONList.ItemsSource = JSONItemList;
                    t3_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t3_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFolder(extension);
                if (JSONItemList.Count > 0)
                {
                    t3_dg_JSONList.ItemsSource = JSONItemList;
                    t3_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t3_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            JSONItemList = new ObservableCollection<Item>();
            t3_dg_JSONList.ItemsSource = JSONItemList;
            t3_tb_folder.Text = "";
            t3_tb_output.Text = "";
            t3_cb_selectAll.IsChecked = false;
        }

        private void t3_cb_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(JSONItemList, t3_cb_selectAll);
        }

        private void t3_btn_MaskJSON_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t3_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                Converter converter = new Converter();
                List<Item> filteredSelected = JSONItemList.Where(item => item.IsSelected).ToList();
                int filteredCount = filteredSelected.Count;

                if (filteredCount > 0)
                {
                    if (t3_cb_maskingTemplate.SelectedValue != null)
                    {
                        // Initialize progress reporting
                        var progress = new Progress<int>(value =>
                        {
                            t3_progressBar.Value = (int)((double)value / filteredCount * 100);
                            t3_progressText.Text = $"{value}/{filteredCount}";
                        });

                        // Initialize Progress Bar
                        t3_progressBar.Value = 0;
                        t3_progressText.Text = "0/0";
                        t3_progressBar.Visibility = Visibility.Visible;
                        t3_progressText.Visibility = Visibility.Visible;
                        t3_btn_StopProgressBar.Visibility = Visibility.Visible;

                        MaskingTemplate maskingTemplate = maskingTemplateController.getMaskingTemplate("Name", t3_cb_maskingTemplate.Text).ToObject<MaskingTemplate>();
                        string savePath = GeneralMethod.saveFolderDialog();

                        if (savePath != "")
                        {
                            int completedItems = 0;
                            foreach (Item item in filteredSelected)
                            {
                                if (isInterrupted)
                                    break;

                                JObject jsonItem = JObject.Parse(item.FileContent);
                                foreach (Masking masking in maskingTemplate.Mask)
                                {
                                    jsonItem = maskingVariableJSON(jsonItem, masking);
                                }

                                // Save Response to JSON File
                                string JSONTextIndent = JsonConvert.SerializeObject(jsonItem, Formatting.Indented);
                                string fileOutputPath = converter.saveTextFile(savePath + @"\" + item.FileName + "-mask" + ".json", JSONTextIndent, "req");

                                // Update progress
                                completedItems++;
                                ((IProgress<int>)progress).Report(completedItems);
                            }
                            t3_tb_output.Text = savePath;
                            MessageBox.Show("[SUCCESS]: JSON Masking has been saved");
                        }
                    } else
                        MessageBox.Show("[WARNING]: Please select masking template");
                } else
                    MessageBox.Show("[WARNING]: No one item were selected");
            } catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            } finally
            {
                // Re-enable the cursor and reset it to the default
                t3_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t3_progressBar.Visibility = Visibility.Hidden;
                t3_progressText.Visibility = Visibility.Hidden;
                t3_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private JObject maskingVariableJSON(JObject json, Masking masking, string key = "")
        {
            foreach (var property in json)
            {
                if (property.Value.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                {
                    if (property.Key == "Variables" || property.Key == "Header")
                    {
                        JObject variable = (JObject)property.Value;
                        string maskingKey = masking.Variable.Split('.').First();
                        string maskingVariable = masking.Variable.Split('.').Last();

                        if (maskingKey == key && variable[maskingVariable] != null)
                        {
                            variable[maskingVariable] = masking.Value;
                            json["Variables"] = variable;
                        }
                    }
                    else
                        maskingVariableJSON((JObject)property.Value, masking, property.Key);
                }
                else if (property.Value.GetType().ToString() == "Newtonsoft.Json.Linq.JArray" && property.Key == "Categories")
                    foreach (var category in property.Value)
                        maskingVariableJSON((JObject)category, masking, key);
            }

            return json;
        }

        private void t3_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            isInterrupted = true;
        }

        private async void t3_btn_StopProgressBar_MouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private async void t3_btn_StopProgressBar_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        private void t3_tb_SearchJSONList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = JSONItemList.Where(file => file.FileName.Contains(t3_tb_SearchJSONList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t3_dg_JSONList.ItemsSource = search;
        }

        private void t3_dg_JSONList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t3_dg_JSONList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }
    }
}
