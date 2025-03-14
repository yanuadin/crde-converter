using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for JSONDateTimeToDate.xaml
    /// </summary>
    public partial class JSONDateTimeToDate : UserControl
    {
        private ObservableCollection<Item> JSONItemList = new ObservableCollection<Item>();

        public JSONDateTimeToDate()
        {
            InitializeComponent();
        }

        private void t6_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFile(extension, true);
                t6_dg_JSONList.ItemsSource = JSONItemList;
                t6_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open file");
            }
        }

        private void t6_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                JSONItemList = GeneralMethod.browseFolder(extension);
                t6_dg_JSONList.ItemsSource = JSONItemList;
                t6_tb_folder.Text = string.Join(@"\", JSONItemList.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open folder");
            }
        }

        private void t6_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            JSONItemList = new ObservableCollection<Item>();
            t6_dg_JSONList.ItemsSource = JSONItemList;
            t6_tb_folder.Text = "";
            t6_tb_output.Text = "";
            t6_cb_selectAll.IsChecked = false;
        }

        private void t6_cb_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(JSONItemList, t6_cb_selectAll);
        }

        private void t6_btn_ConvertDate_Click(object sender, RoutedEventArgs e)
        {
            List<Item> filteredSelected = JSONItemList.Where(item => item.IsSelected).ToList();
            Converter converter = new Converter();

            try
            {
                if (filteredSelected.Count > 0)
                {
                    string savePath = GeneralMethod.saveFolderDialog();

                    if (savePath != "")
                    {
                        int successCount = 0;
                        int failedCount = 0;
                        foreach (Item item in filteredSelected)
                        {
                            // Save Response to JSON File
                            string formattedDateJSON = replaceDateTimeFormat(item.FileContent, "yyyy-MM-dd");
                            if (formattedDateJSON != "")
                            {
                                JObject resultJSON = JObject.Parse(formattedDateJSON);
                                string formattingIndentJSON = JsonConvert.SerializeObject(resultJSON, Formatting.Indented);
                                string fileOutputPath = converter.saveTextFile(savePath + @"\" + item.FileName + "-formatted-datetime" + ".json", formattingIndentJSON, "req");
                                successCount++;
                            } else
                                failedCount++;
                        }
                        t6_tb_output.Text = savePath;
                        MessageBox.Show($"[SUCCESS]: {successCount} / {successCount + failedCount} JSON Date Format has been saved");
                    }
                }
                else
                {
                    MessageBox.Show("[WARNING]: No one item were selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private string replaceDateTimeFormat(string input, string newFormat)
        {
            // Define the regex pattern
            string pattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}";

            // Use Regex to find matches
            MatchCollection matches = Regex.Matches(input, pattern);

            // Iterate through matches and replace with the new format
            foreach (Match match in matches)
            {
                if (DateTime.TryParseExact(match.Value, "yyyy-MM-ddTHH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                {
                    // Replace the matched datetime string with the new format
                    string formattedDateTime = dateTime.ToString(newFormat);
                    input = input.Replace(match.Value, formattedDateTime);
                }
            }

            if (matches.Count == 0)
                input = "";

            return input;
        }
    }
}
