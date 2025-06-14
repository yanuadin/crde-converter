﻿using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private bool isInterrupted = false;
        private string saveOutputPath = "";
        private CRDE config = new CRDE();
        private CancellationTokenSource _cts;

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

        private async void t1_btn_ConvertJSONToExcel_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t1_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            saveOutputPath = "";
            t1_btn_OpenExcelFile.Visibility = Visibility.Hidden;
            _cts = new CancellationTokenSource();

            try
            {
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
                    t1_btn_StopProgressBar.Visibility = Visibility.Visible;

                    // Create Excel package
                    using (var package = new ExcelPackage())
                    {
                        // Arrange File Name
                        string fname = "";
                        if (filteredCount == 1)
                        {
                            JObject parseJSON = JObject.Parse(filteredSelected.First<Item>().FileContent);
                            string type = parseJSON.First.ToObject<JProperty>()?.Name.ToString() == "StrategyOneRequest" ? "-req" : "-res";
                            fname = parseJSON.First.First.First.First["InquiryCode"].ToString() + type;
                        }
                        else
                            fname = "MultipleFiles";

                        fname += ".xlsx";

                        // Save Excel file
                        string[] extension = { "excel" };
                        string savePath = GeneralMethod.saveFileDialog(extension, fname);

                        // Loop through the multiple files
                        int iterator = 0;
                        int completedItems = 0;

                        Converter converter = new Converter();

                        foreach (Item file in filteredSelected)
                        {
                            if (isInterrupted)
                                break;

                            string filePath = file.FilePath;
                            string fileName = file.FileName;
                            string jsonContent = File.ReadAllText(filePath);

                            await Task.Run(() => converter.convertJSONToExcel(package, jsonContent, iterator++), _cts.Token);

                            // Update progress
                            completedItems++;
                            ((IProgress<int>)progress).Report(completedItems);

                            await Task.Delay(50);

                            if (_cts.Token.IsCancellationRequested)
                                break;
                        }

                        if (savePath != "")
                        {
                            saveOutputPath = savePath;
                            package.SaveAs(new FileInfo(savePath));
                            t1_tb_output.Text = savePath;
                            t1_btn_OpenExcelFile.Visibility = Visibility.Visible;
                            MessageBox.Show(@"[SUCCESS]: Conversion successful");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[ERROR]: {ex.Message}");
            }
            finally
            {
                // Re-enable the cursor and reset it to the default
                t1_sp_main.IsEnabled = true;
                Mouse.OverrideCursor = null;
                t1_progressBar.Visibility = Visibility.Hidden;
                t1_progressText.Visibility = Visibility.Hidden;
                isInterrupted = false;
                t1_btn_StopProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void t1_btn_OpenExcelFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GeneralMethod.openFile(saveOutputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[ERROR]: {ex.Message}");
            }
        }

        private void t1_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            isInterrupted = true;
            _cts?.Cancel();
        }

        private async void t1_btn_StopProgressBar_MouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private async void t1_btn_StopProgressBar_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        private void t1_tb_SearchJSONList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = lb_JSONItems.Where(file => file.FileName.Contains(t1_tb_SearchJSONList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Item>();

            if (search != null)
                t1_lb_JSONList.ItemsSource = search;
        }

        private void t1_lb_JSONList_CopyCell(object sender, DataGridRowClipboardEventArgs e)
        {
            var currentCell = e.ClipboardRowContent[t1_lb_JSONList.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }
    }
}
