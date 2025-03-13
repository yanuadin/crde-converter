using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for ExcelConverter.xaml
    /// </summary>
    public partial class ExcelConverter : UserControl
    {
        ObservableCollection<Item> lb_JSONItems = new ObservableCollection<Item>();

        public ExcelConverter()
        {
            InitializeComponent();
        }

        private void t2_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONItems = new ObservableCollection<Item>();
                ObservableCollection<Item> excelFile = GeneralMethod.browseFile("excel", false);
                string fileName = excelFile.First<Item>().FileName;
                string filePath = excelFile.First<Item>().FilePath;
                t2_tb_folder.Text = filePath;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets["#HEADER#"];
                    for (int row = 3; row <= ws.Dimension.Rows; row++)
                    {
                        lb_JSONItems.Add(new Item { FileName = ws.Cells[row, 5].Text, FilePath = filePath, JSON = "", IsSelected = false });
                    }
                    t2_lb_JSONList.ItemsSource = lb_JSONItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open file");
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

        private void t2_btn_ConvertExcelToTxt_Click(object sender, RoutedEventArgs e)
        {
            Converter converter = new Converter();
            List<Item> filteredSelected = lb_JSONItems.Where(item => item.IsSelected).ToList();

            if (filteredSelected.Count > 0)
            {
                string filePath = filteredSelected.First<Item>().FilePath;
                string savePath = converter.convertExcelTo(filePath, filteredSelected, "txt");
                if (savePath != "")
                {
                    t2_tb_txt_output.Text = savePath;
                } else 
                {
                    MessageBox.Show("[FAILED]: Save path not found");
                }
            } else
            {
                MessageBox.Show("[WARNING]: No one item were selected");
            }
        }

        private void t2_btn_ConvertExcelToJSON_Click(object sender, RoutedEventArgs e)
        {
            Converter converter = new Converter();
            List<Item> filteredSelected = lb_JSONItems.Where(item => item.IsSelected).ToList();

            if (filteredSelected.Count > 0)
            {
                string filePath = filteredSelected.First<Item>().FilePath;
                string savePath = converter.convertExcelTo(filePath, filteredSelected, "json");
                if (savePath != "")
                {
                    t2_tb_json_output.Text = savePath;
                }
                else
                {
                    MessageBox.Show("[FAILED]: Save path not found");
                }
            } else
            {
                MessageBox.Show("[WARNING]: No one item were selected");
            }
        }
    }
}
