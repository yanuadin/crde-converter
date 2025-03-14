using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for JSONMasking.xaml
    /// </summary>
    public partial class JSONMasking : UserControl
    {
        private ObservableCollection<Item> JSONItemList = new ObservableCollection<Item>();
        private CRDE config = new CRDE();

        public JSONMasking()
        {
            InitializeComponent();
            
            t3_cb_maskingTemplate.ItemsSource = config.getMaskingTemplateList();
        }

        private void t3_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFile(extension, true);
                t3_dg_JSONList.ItemsSource = JSONItemList;
                t3_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open file");
            }
        }

        private void t3_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFolder(extension);
                t3_dg_JSONList.ItemsSource = JSONItemList;
                t3_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open folder");
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
            List<Item> filteredSelected = JSONItemList.Where(item => item.IsSelected).ToList();
            Converter converter = new Converter();

            try
            {
                if (filteredSelected.Count > 0)
                {
                    if (t3_cb_maskingTemplate.SelectedValue != null)
                    {
                        MaskingTemplate maskingTemplate = config.getMaskingTemplate(t3_cb_maskingTemplate.Text).ToObject<MaskingTemplate>();
                        string savePath = GeneralMethod.saveFolderDialog();

                        if (savePath != "")
                        {
                            foreach (Item item in filteredSelected)
                            {
                                JObject jsonItem = JObject.Parse(item.FileContent);
                                foreach (Masking masking in maskingTemplate.Mask)
                                {
                                    jsonItem = maskingVariableJSON(jsonItem, masking);
                                }

                                // Save Response to JSON File
                                string JSONTextIndent = JsonConvert.SerializeObject(jsonItem, Formatting.Indented);
                                string fileOutputPath = converter.saveTextFile(savePath + @"\" + item.FileName + "-mask" + ".json", JSONTextIndent, "req");
                            }
                            t3_tb_output.Text = savePath;
                            MessageBox.Show("[SUCCESS]: JSON Masking has been saved");
                        }
                    } else
                    {
                        MessageBox.Show("[WARNING]: Please select masking template");
                    }
                } else
                {
                    MessageBox.Show("[WARNING]: No one item were selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
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
    }
}
