using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for JSONConverter.xaml
    /// </summary>
    public partial class JSONConverter : UserControl
    {
        private ObservableCollection<Item> lb_JSONItems = new ObservableCollection<Item>();

        public JSONConverter()
        {
            InitializeComponent();
        }

        private void t1_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                lb_JSONItems = GeneralMethod.browseFile(extension, true);
                if (lb_JSONItems.Count > 0)
                {
                    t1_lb_JSONList.ItemsSource = lb_JSONItems;
                    t1_tb_folder.Text = string.Join(@"\", lb_JSONItems.First<Item>().FilePath.Split(@"\")[0..^1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t1_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "json" };
                lb_JSONItems = GeneralMethod.browseFolder(extension);

                if (lb_JSONItems.Count > 0)
                {
                    t1_lb_JSONList.ItemsSource = lb_JSONItems;
                    t1_tb_folder.Text = string.Join(@"\", lb_JSONItems.First<Item>().FilePath.Split(@"\")[0..^1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t1_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONItems = new ObservableCollection<Item>();
            t1_lb_JSONList.ItemsSource = lb_JSONItems;
            t1_tb_folder.Text = "";
            t1_tb_output.Text = "";
            t1_cb_selectAll.IsChecked = false;
        }

        private void t1_cb_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_JSONItems, t1_cb_selectAll);
        }

        private void t1_btn_ConvertJSONToExcel_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t1_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                Converter converter = new Converter();
                List<Item> filteredSelected = lb_JSONItems.Where(item => item.IsSelected).ToList();
                int filteredCount = filteredSelected.Count;

                if (filteredCount > 0)
                {
                    // Initialize progress reporting
                    var progress = new Progress<int>(value =>
                    {
                        t1_progressBar.Value = (int)((double)value / filteredCount * 100);
                        t1_progressText.Text = $"{value}/{filteredCount}";
                    });

                    // Initialize Progress Bar
                    t1_progressBar.Value = 0;
                    t1_progressText.Text = "0/0";
                    t1_progressBar.Visibility = Visibility.Visible;
                    t1_progressText.Visibility = Visibility.Visible;

                    // Create Excel package
                    using (var package = new ExcelPackage())
                    {
                        // Arrange File Name
                        string fname = "";
                        if (filteredCount == 1)
                        {
                            JObject parseJSON = JObject.Parse(filteredSelected.First<Item>().FileContent);
                            fname = parseJSON.First.First.First.First["InquiryCode"].ToString();
                        }
                        else
                            fname = "MultipleFiles";

                        fname += "-req.xlsx";

                        // Loop through the multiple files
                        int iterator = 0;
                        int completedItems = 0;
                        foreach (Item file in filteredSelected)
                        {
                            string filePath = file.FilePath;
                            string fileName = file.FileName;
                            string jsonContent = File.ReadAllText(filePath);

                            converter.convertJSONToExcel(package, jsonContent, iterator++);

                            // Update progress
                            completedItems++;
                            ((IProgress<int>)progress).Report(completedItems);
                        }

                        // Save Excel file
                        string[] extension = { "excel" };
                        string savePath = GeneralMethod.saveFileDialog(extension, fname);

                        if (savePath != "")
                        {
                            package.SaveAs(new FileInfo(savePath));
                            t1_tb_output.Text = savePath;
                            MessageBox.Show(@"[SUCCESS]: Conversion successful");
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: Error: " + ex.Message);
            } finally
            {
                // Re-enable the cursor and reset it to the default
                t1_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t1_progressBar.Visibility = Visibility.Hidden;
                t1_progressText.Visibility = Visibility.Hidden;
            }
        }

        private void t1_lb_JSONList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t1_lb_JSONList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }
    }
}
