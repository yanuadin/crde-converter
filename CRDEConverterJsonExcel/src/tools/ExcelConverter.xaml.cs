using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for ExcelConverter.xaml
    /// </summary>
    public partial class ExcelConverter : UserControl
    {
        List<Item> lb_JSONItems = new List<Item>();

        public ExcelConverter()
        {
            InitializeComponent();
        }

        private void t2_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONItems = new List<Item>();
                List<Item> excelFile = GeneralMethod.browseFile("excel", false);
                string fileName = excelFile.First<Item>().fileName;
                string filePath = excelFile.First<Item>().filePath;
                t2_tb_folder.Text = filePath;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets["#HEADER#"];
                    for (int row = 3; row <= ws.Dimension.Rows; row++)
                    {
                        lb_JSONItems.Add(new Item { fileName = ws.Cells[row, 5].Text, filePath = filePath, json = "", isSelected = false });
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
            t2_lb_JSONList.Items.Refresh();
        }
        
        private void t2_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            lb_JSONItems = new List<Item>();
            t2_lb_JSONList.ItemsSource = lb_JSONItems;
            t2_tb_folder.Text = "";
        }

        private void t2_btn_ConvertExcelToTxt_Click(object sender, RoutedEventArgs e)
        {
            Converter converter = new Converter();
            List<Item> filteredSelected = lb_JSONItems.Where(item => item.isSelected).ToList();

            if (filteredSelected.Count > 0)
            {
                string filePath = filteredSelected.First<Item>().filePath;
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
            List<Item> filteredSelected = lb_JSONItems.Where(item => item.isSelected).ToList();

            if (filteredSelected.Count > 0)
            {
                string filePath = filteredSelected.First<Item>().filePath;
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
