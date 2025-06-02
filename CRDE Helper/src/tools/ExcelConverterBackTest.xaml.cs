using CRDEConverterJsonExcel.core;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CRDEConverterJsonExcel.objectClass;
using System.Diagnostics;
using System.Windows.Input;
using CRDE_Helper.objectClass;

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
        private CancellationTokenSource _cts;

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
            _cts = new CancellationTokenSource();

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
                    chunkPathResult = await ProcessExcelChunksAsync(progress, savePath);
                else
                    chunkPathResult = excelPath;

                // Merge Excel
                int totalOfSuccess = await Task.Run(() => ProcessConversionAsync(converter, progress, chunkPathResult, savePath, fileType), _cts.Token);

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
                ObservableCollection<BackTraceDictionary> backTraceDictionaries = new ObservableCollection<BackTraceDictionary>();

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
                    chunkPathResult.Add(chunkProcess(workbook, backTraceDictionaries, chunkSize, chunk, savePath));
                    ((IProgress<int>)progress).Report(chunk);
                    await Task.Delay(50);
                }
            }

            return chunkPathResult;
        }

        private async Task<int> ProcessConversionAsync(Converter converter, IProgress<int> progress, List<string> chunkPathResult, string savePath, string fileType)
        {
            int totalOfSuccess = 0;

            try
            {
                InitializeProgressBar(lb_JSONItems.Count, "Converting File");

                progress = new Progress<int>(value =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        t9_progressBar.Value = (int)((double)value / lb_JSONItems.Count * 100);
                        t9_progressText.Text = $"{value}/{lb_JSONItems.Count}";
                    });
                });

                await Task.Delay(100);

                foreach (string chunkPath in chunkPathResult)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    string masterExcelPath = "";
                    Dispatcher.Invoke(() =>
                    {
                        masterExcelPath = t9_tb_master_excel_file.Text;
                    });

                    ExcelPackage mergeExcel = converter.mergeTwoExcel(
                        new ExcelPackage(new FileInfo(masterExcelPath)),
                        new ExcelPackage(new FileInfo(chunkPath)));

                    string fileNameSaved = savePath + @"\" + Path.GetFileNameWithoutExtension(chunkPath) +
                        (fileType == "txt" ? ".txt" : ".json");

                    var (resultPath, successCount) = await converter.convertExcelTo(
                        masterExcelPath,
                        lb_JSONItems.ToList(),
                        fileType,
                        progress,
                        mergeExcel,
                        fileType == "txt" ? fileNameSaved : savePath, _cts.Token);

                    totalOfSuccess += successCount;

                    if (savePath != "")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (fileType == "txt")
                                t9_tb_text_output.Text = savePath;
                            else
                                t9_tb_json_output.Text = savePath;
                        });
                    }

                    _cts.Token.ThrowIfCancellationRequested();

                    File.Delete(chunkPath);
                    ((IProgress<int>)progress).Report(totalOfSuccess);
                    await Task.Delay(50);
                }

                return totalOfSuccess;
            } catch (OperationCanceledException)
            {
                return totalOfSuccess;
            }
        }

        private void InitializeProgressBar(double totalItems, string progressName)
        {
            Dispatcher.Invoke(() =>
            {
                t9_progressBar.Value = 0;
                t9_progressText.Text = $"0/{totalItems}";
                t9_tb_progressName.Text = progressName;
            });
        }

        private void ResetUIState()
        {
            t9_sp_main.IsEnabled = true;
            Mouse.OverrideCursor = null;
            t9_tb_progressName.Visibility = Visibility.Hidden;
            t9_progressBar.Visibility = Visibility.Hidden;
            t9_progressText.Visibility = Visibility.Hidden;
            t9_btn_StopProgressBar.Visibility = Visibility.Hidden;
        }

        private string chunkProcess(ExcelWorkbook workbook, ObservableCollection<BackTraceDictionary> backTraceDictionaries, int chunkSize, int chunk, string savePath)
        {
            ExcelPackage newPackage = new ExcelPackage();
            string lastHeaderId = workbook.Worksheets[0].Cells[((int)chunkSize * chunk) + 3, 1].Text;

            // Loop through the worksheets in the Excel file to JSON
            for (int sheet = 0; sheet < workbook.Worksheets.Count; sheet++)
            {
                // Get the worksheet by name
                ExcelWorksheet ws = workbook.Worksheets[sheet];

                // DictionaryHeader
                if (backTraceDictionaries.FirstOrDefault(d => d.SheetName == ws.Name) == null)
                    backTraceDictionaries.Add(new BackTraceDictionary() { SheetName = ws.Name, ChunkStartRow = 3 });

                var backTraceDictionary = backTraceDictionaries.FirstOrDefault(d => d.SheetName == ws.Name);

                if (ws != null && ws.Dimension != null && backTraceDictionary != null)
                {
                    // Get the number of rows and columns
                    int rowCount = ws.Dimension.Rows;
                    int colCount = ws.Dimension.Columns;

                    // Header and Data Have Main_Id
                    if (ws.Name == "#HEADER#" || ws.Cells[2, 1].Text == "Main_Id")
                    {
                        backTraceDictionary.ChunkSelectedCol = 1;
                        backTraceDictionary.ChunkParentCol = 3;
                    }

                    string parentName = ws.Cells[3, 2].Text;
                    if (ws.Name != "#HEADER#" && lastHeaderId != "")
                    {
                        var parent = backTraceDictionaries.FirstOrDefault(d => d.SheetName == parentName);
                        if (parent != null)
                            lastHeaderId = parent.ChunkLastId.ToString();
                    }

                    var columnCells = ws.Cells[backTraceDictionary.ChunkStartRow, backTraceDictionary.ChunkSelectedCol, ws.Dimension.End.Row, backTraceDictionary.ChunkSelectedCol];

                    newPackage.Workbook.Worksheets.Add(ws.Name);
                    ExcelWorksheet newWs = newPackage.Workbook.Worksheets[ws.Name];

                    // Find using LINQ (still loops internally but more concise)
                    var matchingCell = columnCells.FirstOrDefault(c => c.Value?.ToString() == lastHeaderId);
                    if (lastHeaderId != "")
                    {
                        if (matchingCell == null)
                        {
                            string tempLastHeader = lastHeaderId;
                            while (matchingCell == null)
                            {
                                tempLastHeader = (int.Parse(tempLastHeader) + 1).ToString();
                                matchingCell = columnCells.FirstOrDefault(c => c.Value?.ToString() == tempLastHeader);
                            }
                        }
                    }

                    if (matchingCell != null)
                    {
                        ws.Cells[1, 1, 2, colCount].Copy(newWs.Cells[1, 1, 2, colCount]);
                        ws.Cells[backTraceDictionary.ChunkStartRow, 1, matchingCell.Start.Row - 1, colCount].Copy(newWs.Cells[3, 1, (int)chunkSize + 2, colCount]);
                        backTraceDictionary.ChunkStartRow = matchingCell.Start.Row;
                        backTraceDictionary.ChunkLastId = ws.Name == "#HEADER#" ? int.Parse(lastHeaderId) : GeneralMethod.convertTryParse(ws.Cells[matchingCell.Start.Row, backTraceDictionary.ChunkParentCol].Text, "Integer");
                    }
                    else if (lastHeaderId == "")
                    {
                        ws.Cells[1, 1, 2, colCount].Copy(newWs.Cells[1, 1, 2, colCount]);
                        ws.Cells[backTraceDictionary.ChunkStartRow, 1, rowCount, colCount].Copy(newWs.Cells[3, 1, (int)chunkSize + 2, colCount]);
                    }
                }
            }

            string chunkPath = savePath + @$"\backtrace-batch-{chunk}.xlsx";
            newPackage.SaveAs(new FileInfo(chunkPath));

            return chunkPath;
        }

        private void t9_btn_StopProgressBar_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
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
