using CRDEConverterJsonExcel.core;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CRDEConverterJsonExcel.objectClass;
using System.Diagnostics;
using System.Xml.Linq;
using Amazon.S3.Model;
using System.IO.Packaging;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CRDE_Helper.controller;
using Amazon.Runtime.Internal.Transform;

namespace CRDE_Helper.src.tools
{
    /// <summary>
    /// Interaction logic for ExcelConverterBackTest.xaml
    /// </summary>
    public partial class ExcelConverterBackTest : UserControl
    {
        List<string> excelPath = new List<string>();
        ObservableCollection<Item> lb_JSONItems = new ObservableCollection<Item>();
        ObservableCollection<Item> masterData = new ObservableCollection<Item>();
        bool isInterrupted = false;

        public ExcelConverterBackTest()
        {
            InitializeComponent();
        }

        private void t9_btn_SelectMasterExcelFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] extension = { "excel" };
                masterData = GeneralMethod.browseFile(extension, true);
                if (masterData.Count > 0)
                    t9_tb_master_excel_file.Text = string.Join(@"\", masterData.First<Item>().FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t9_btn_SelectBackTestExcelFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clean Data
                lb_JSONItems.Clear();
                excelPath.Clear();
                t9_tb_back_test_excel_file.Text = "";

                string[] extension = { "excel" };
                ObservableCollection<Item> excelFile = GeneralMethod.browseFile(extension, true);

                if (excelFile.Count > 0)
                {
                    foreach (Item file in excelFile)
                    {
                        string fileName = file.FileName;
                        string filePath = file.FilePath;
                        excelPath.Add(filePath);

                        using (var package = new ExcelPackage(new FileInfo(filePath)))
                        {
                            ExcelWorksheet ws = package.Workbook.Worksheets["#HEADER#"];
                            for (int row = 3; row <= ws.Dimension.Rows; row++)
                            {
                                lb_JSONItems.Add(new Item { FileName = ws.Cells[row, 5].Text, FilePath = filePath, FileContent = "", IsSelected = false });
                            }
                        }
                    }
                    t9_tb_back_test_excel_file.Text = String.Join(Environment.NewLine, excelPath.ToArray());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private async void t9_btn_ConvertTextFile_Click(object sender, RoutedEventArgs e)
        {
            await ConvertFilesAsync("txt");
        }

        private async void t9_btn_ConvertJSONFile_Click(object sender, RoutedEventArgs e)
        {
            await ConvertFilesAsync("json");
        }

        private async Task ConvertFilesAsync(string fileType)
        {
            // Disable the cursor and set it to "Wait" (spinning circle)
            t9_sp_main.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var start = DateTime.Now;
                Trace.WriteLine("START : " + start);

                Converter converter = new Converter();
                var progress = new Progress<int>();
                List<string> chunkPathResult = new List<string>();
                string savePath = GeneralMethod.saveFolderDialog();

                // Visible Progress Bar
                t9_tb_progressName.Visibility = Visibility.Visible;
                t9_progressBar.Visibility = Visibility.Visible;
                t9_progressText.Visibility = Visibility.Visible;
                t9_btn_StopProgressBar.Visibility = Visibility.Visible;

                if (t9_cb_isSplittedFile.IsChecked == false)
                {
                    chunkPathResult = await ProcessExcelChunksAsync(progress, savePath);
                }
                else
                {
                    chunkPathResult = excelPath;
                }

                // Merge Excel
                int totalOfSuccess = await ProcessConversionAsync(converter, progress, chunkPathResult, savePath, fileType);

                MessageBox.Show($"[SUCCESS]: {totalOfSuccess} files converted successfully");

                var finish = DateTime.Now;
                TimeSpan timeDifference = finish - start;
                Trace.WriteLine("FiNISH : " + finish);
                Trace.WriteLine("DURATION (s) : " + timeDifference.TotalSeconds);
                Trace.WriteLine("DURATION (m) : " + timeDifference.TotalMinutes);
            }
            finally
            {
                // Re-enable the cursor and reset it to the default
                ResetUIState();
            }
        }

        private async Task<List<string>> ProcessExcelChunksAsync(IProgress<int> progress, string savePath)
        {
            List<string> chunkPathResult = new List<string>();

            foreach (string path in excelPath)
            {
                string chunkSavePath = string.Join(@"\", path.Split(@"\")[0..^1]) + @"\";
                int chunkSize = 100;
                ExcelPackage package = new ExcelPackage(new FileInfo(path));
                ExcelWorkbook workbook = package.Workbook;
                Dictionary<string, int> dictionarySheetStartRow = new Dictionary<string, int>();

                double totalHeaderRow = (double)(workbook.Worksheets["#HEADER#"].Dimension.Rows - 2) / chunkSize;
                double countOfChunk = Math.Ceiling(totalHeaderRow);

                InitializeProgressBar(countOfChunk, "Splitting Excel File");

                progress = new Progress<int>(value =>
                {
                    t9_progressBar.Value = (int)((double)value / countOfChunk * 100);
                    t9_progressText.Text = $"{value}/{countOfChunk}";
                });

                await Task.Delay(100);

                for (int chunk = 1; chunk <= countOfChunk; chunk++)
                {
                    if (isInterrupted)
                        break;

                    chunkPathResult.Add(chunkProcess(workbook, dictionarySheetStartRow, chunkSize, chunk, savePath));
                    ((IProgress<int>)progress).Report(chunk);
                    await Task.Delay(50);
                }
            }

            return chunkPathResult;
        }

        private async Task<int> ProcessConversionAsync(Converter converter, IProgress<int> progress, List<string> chunkPathResult, string savePath, string fileType)
        {
            int totalOfSuccess = 0;

            InitializeProgressBar(lb_JSONItems.Count, "Converting File");

            progress = new Progress<int>(value =>
            {
                t9_progressBar.Value = (int)((double)value / lb_JSONItems.Count * 100);
                t9_progressText.Text = $"{value}/{lb_JSONItems.Count}";
            });

            await Task.Delay(100);

            foreach (string chunkPath in chunkPathResult)
            {
                if (isInterrupted)
                    break;

                ExcelPackage mergeExcel = converter.mergeTwoExcel(
                    new ExcelPackage(new FileInfo(t9_tb_master_excel_file.Text)),
                    new ExcelPackage(new FileInfo(chunkPath)));

                string fileNameSaved = savePath + @"\" + Path.GetFileNameWithoutExtension(chunkPath) +
                    (fileType == "txt" ? ".txt" : ".json");

                var (resultPath, successCount) = await converter.convertExcelTo(
                    t9_tb_master_excel_file.Text,
                    lb_JSONItems.ToList(),
                    fileType,
                    progress,
                    mergeExcel,
                    fileType == "txt" ? fileNameSaved : savePath);

                totalOfSuccess += successCount;

                if (savePath != "")
                {
                    if (fileType == "txt")
                        t9_tb_text_output.Text = savePath;
                    else
                        t9_tb_json_output.Text = savePath;
                }

                File.Delete(chunkPath);
                ((IProgress<int>)progress).Report(totalOfSuccess);
                await Task.Delay(50);
            }

            return totalOfSuccess;
        }

        private void InitializeProgressBar(double totalItems, string progressName)
        {
            t9_progressBar.Value = 0;
            t9_progressText.Text = $"0/{totalItems}";
            t9_tb_progressName.Text = progressName;
        }

        private void ResetUIState()
        {
            t9_sp_main.IsEnabled = true;
            Mouse.OverrideCursor = null;
            t9_tb_progressName.Visibility = Visibility.Hidden;
            t9_progressBar.Visibility = Visibility.Hidden;
            t9_progressText.Visibility = Visibility.Hidden;
            t9_btn_StopProgressBar.Visibility = Visibility.Hidden;
            isInterrupted = false;
        }

        private string chunkProcess(ExcelWorkbook workbook, Dictionary<string, int> dictionarySheetStartRow, int chunkSize, int chunk, string savePath)
        {
            ExcelPackage newPackage = new ExcelPackage();
            string lastHeaderId = workbook.Worksheets[0].Cells[((int)chunkSize * chunk) + 3, 1].Text;

            // Loop through the worksheets in the Excel file to JSON
            for (int sheet = 0; sheet < workbook.Worksheets.Count; sheet++)
            {
                // Get the worksheet by name
                ExcelWorksheet ws = workbook.Worksheets[sheet];

                // DictionaryHeader
                if (!dictionarySheetStartRow.ContainsKey(ws.Name))
                    dictionarySheetStartRow.Add(ws.Name, 3);

                if (ws != null && ws.Dimension != null)
                {
                    // Get the number of rows and columns
                    int rowCount = ws.Dimension.Rows;
                    int colCount = ws.Dimension.Columns;
                    int selectedCol = 1;

                    if (ws.Name != "#HEADER#" && ws.Cells[2, 1].Text != "Main_Id")
                        selectedCol = 3;

                    var columnCells = ws.Cells[dictionarySheetStartRow[ws.Name], selectedCol, ws.Dimension.End.Row, selectedCol];

                    // Find using LINQ (still loops internally but more concise)
                    var matchingCell = columnCells.FirstOrDefault(c => c.Value?.ToString() == lastHeaderId);

                    newPackage.Workbook.Worksheets.Add(ws.Name);
                    ExcelWorksheet newWs = newPackage.Workbook.Worksheets[ws.Name];

                    if (matchingCell != null)
                    {
                        ws.Cells[1, 1, 2, colCount].Copy(newWs.Cells[1, 1, 2, colCount]);
                        ws.Cells[dictionarySheetStartRow[ws.Name], 1, matchingCell.Start.Row - 1, colCount].Copy(newWs.Cells[3, 1, (int)chunkSize + 2, colCount]);
                        dictionarySheetStartRow[ws.Name] = matchingCell.Start.Row;

                    }
                    else if (lastHeaderId == "")
                    {
                        ws.Cells[1, 1, 2, colCount].Copy(newWs.Cells[1, 1, 2, colCount]);
                        ws.Cells[dictionarySheetStartRow[ws.Name], 1, rowCount, colCount].Copy(newWs.Cells[3, 1, (int)chunkSize + 2, colCount]);
                    }
                }
            }

            string chunkPath = savePath + @$"\backtrace-batch-{chunk}.xlsx";
            newPackage.SaveAs(new FileInfo(chunkPath));

            return chunkPath;
        }

        private void t9_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            isInterrupted = true;
        }

        private async void t9_btn_StopProgressBar_MouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private async void t9_btn_StopProgressBar_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }
    }
}
