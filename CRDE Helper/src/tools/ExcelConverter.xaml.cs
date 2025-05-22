using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for ExcelConverter.xaml
    /// </summary>
    public partial class ExcelConverter : UserControl
    {
        ObservableCollection<Item> lb_JSONItems = new ObservableCollection<Item>();
        bool isInterrupted = false;

        public ExcelConverter()
        {
            InitializeComponent();
        }

        private void t2_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONItems = new ObservableCollection<Item>();
                string[] extension = { "excel" };
                ObservableCollection<Item> excelFile = GeneralMethod.browseFile(extension, false);
                
                if (excelFile.Count > 0)
                {
                    string fileName = excelFile.First<Item>().FileName;
                    string filePath = excelFile.First<Item>().FilePath;
                    t2_tb_folder.Text = filePath;

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets["#HEADER#"];
                        for (int row = 3; row <= ws.Dimension.Rows; row++)
                        {
                            lb_JSONItems.Add(new Item { FileName = ws.Cells[row, 5].Text, FilePath = filePath, FileContent = "", IsSelected = false });
                        }
                        t2_lb_JSONList.ItemsSource = lb_JSONItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }
        
        private void t2_cb_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralMethod.selectAllList(lb_JSONItems, t2_cb_selectAll);
        }
        
        private void t2_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONItems = new ObservableCollection<Item>();
            t2_lb_JSONList.ItemsSource = lb_JSONItems;
            t2_tb_folder.Text = "";
            t2_tb_json_output.Text = "";
            t2_tb_txt_output.Text = "";
            t2_cb_selectAll.IsChecked = false;
        }

        private async void t2_btn_ConvertExcelToTxt_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t2_sp_main.IsEnabled = false;
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
                        t2_progressBar.Value = (int)((double)value / filteredCount * 100);
                        t2_progressText.Text = $"{value}/{filteredCount}";
                    });

                    // Initialize Progress Bar
                    t2_progressBar.Value = 0;
                    t2_progressText.Text = "0/0";
                    t2_progressBar.Visibility = Visibility.Visible;
                    t2_progressText.Visibility = Visibility.Visible;
                    t2_btn_StopProgressBar.Visibility = Visibility.Visible;

                    string filePath = filteredSelected.First<Item>().FilePath;
                    var (savePath, successCount) = await converter.convertExcelTo(filePath, filteredSelected, "txt", progress);

                    if (savePath != "")
                    {
                        t2_tb_txt_output.Text = savePath;
                        MessageBox.Show($"[SUCCESS]: {successCount} files converted successfully");
                    }
                    else
                        MessageBox.Show("[FAILED]: Save path not found");
                }
                else
                    MessageBox.Show("[WARNING]: No one item were selected");
            } finally
            {
                // Re-enable the cursor and reset it to the default
                t2_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t2_progressBar.Visibility = Visibility.Hidden;
                t2_progressText.Visibility = Visibility.Hidden;
                t2_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private async void t2_btn_ConvertExcelToJSON_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t2_sp_main.IsEnabled = false;
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
                        t2_progressBar.Value = (int)((double)value / filteredCount * 100);
                        t2_progressText.Text = $"{value}/{filteredCount}";
                    });

                    // Initialize Progress Bar
                    t2_progressBar.Value = 0;
                    t2_progressText.Text = "0/0";
                    t2_progressBar.Visibility = Visibility.Visible;
                    t2_progressText.Visibility = Visibility.Visible;
                    t2_btn_StopProgressBar.Visibility = Visibility.Visible;

                    string filePath = filteredSelected.First<Item>().FilePath;
                    var (savePath, successCount) = await converter.convertExcelTo(filePath, filteredSelected, "json", progress);

                    if (savePath != "")
                    {
                        t2_tb_json_output.Text = savePath;
                        MessageBox.Show($"[SUCCESS]: {successCount} files converted successfully");
                    }
                    else
                        MessageBox.Show("[FAILED]: Save path not found");
                }
                else
                    MessageBox.Show("[WARNING]: No one item were selected");
            } finally
            {
                // Re-enable the cursor and reset it to the default
                t2_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t2_progressBar.Visibility = Visibility.Hidden;
                t2_progressText.Visibility = Visibility.Hidden;
                t2_btn_StopProgressBar.Visibility = Visibility.Hidden;
                isInterrupted = false;
            }
        }

        private void t2_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            isInterrupted = true;
        }

        private async void t2_btn_StopProgressBar_MouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private async void t2_btn_StopProgressBar_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        private void t2_tb_SearchJSONList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = lb_JSONItems.Where(file => file.FileName.Contains(t2_tb_SearchJSONList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t2_lb_JSONList.ItemsSource = search;
        }

        private void t2_lb_JSONList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t2_lb_JSONList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }
    }
}
