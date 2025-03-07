using CRDEConverterJsonExcel.objectClass;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.core
{
    class GeneralMethod
    {
        public static string getProjectDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static string getDateTimeNow()
        {
            return DateTime.Now.ToString("dd_MM_yyyy-HH_mm");
        }
        
        public static string getTimestampNow()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static dynamic convertTryParse(dynamic value, string typeData)
        {
            double tempDouble;
            Int64 tempInt;
            DateTime tempDateTime;
            dynamic result;

            switch (typeData)
            {
                case "Integer":
                    Int64.TryParse(value, out tempInt);
                    result = tempInt;
                    break;
                case "Float":
                    double.TryParse(value, out tempDouble);
                    result = tempDouble;
                    break;
                case "Date":
                    DateTime.TryParse(value, out tempDateTime);
                    result = tempDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    break;
                default:
                    result = value;
                    break;
            }

            return result;
        }

        public static ObservableCollection<Item> browseFile(string extension, bool allowedMultipleFiles)
        {
            ObservableCollection<Item> listItem = new ObservableCollection<Item>();

            // Create OpenFileDialog 
            string filter = getFilterExtension(extension);
            string json = "";

            OpenFileDialog dlg = new OpenFileDialog { Filter = filter, Multiselect = allowedMultipleFiles };

            Nullable<bool> result = dlg.ShowDialog();

            // If Allowed MultipleFiles
            string fileName = "";
            if (result == true)
            {
                if (allowedMultipleFiles)
                {
                    // Open document 
                    foreach (string filePath in dlg.FileNames)
                    {
                        fileName = filePath.Split("\\").Last().Split(".").First();

                        listItem.Add(new Item { FileName = fileName, FilePath = filePath, JSON = File.ReadAllText(filePath), IsSelected = false });
                    }
                }
                else
                {
                    fileName = dlg.FileName.Split("\\").Last().Split(".").First();
                    json = extension == "json" ? File.ReadAllText(dlg.FileName) : ""; 

                    listItem.Add(new Item { FileName = fileName, FilePath = dlg.FileName, JSON = json, IsSelected = false });
                }
            }
            
            return listItem;
        }

        public static ObservableCollection<Item> browseFolder(string extension)
        {
            ObservableCollection<Item> listItem = new ObservableCollection<Item>();

            OpenFolderDialog folderDialog = new OpenFolderDialog();
            Nullable<bool> result = folderDialog.ShowDialog();

            if (result == true)
            {
                string[] filePaths = Directory.GetFiles(folderDialog.FolderName);

                foreach (string filePath in filePaths)
                {
                    string fileName = filePath.Split("\\").Last().Split(".").First();
                    string fileExt = filePath.Split("\\").Last().Split(".").Last();
                    string dateCreated = "";
                    FileInfo oFileInfo = new FileInfo(filePath);

                    // Assign Date Created Properties
                    if (oFileInfo.Exists)
                    {
                        DateTime dtCreationTime = oFileInfo.CreationTime;
                        dateCreated = dtCreationTime.ToString();
                    }

                    if (extension == "json" && fileExt == extension)
                        listItem.Add(new Item { FileName = fileName, FilePath = filePath, JSON = File.ReadAllText(filePath), CreatedDate = dateCreated, IsSelected = false });

                    else if (extension == "completed" && fileExt.ToUpper() == extension.ToUpper())
                        listItem.Add(new Item { FileName = fileName, FilePath = filePath, CreatedDate = dateCreated, IsSelected = false });
                }
            }

            return listItem;
        }

        public static string saveFileDialog(string extension, string defaultName = "")
        {
            string filter = getFilterExtension(extension);
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = filter, FileName = defaultName };
            Nullable<bool> result = saveFileDialog.ShowDialog();
            string filePath = "";

            if (result == true)
                filePath = saveFileDialog.FileName;

            return filePath;
        }

        public static string saveFolderDialog()
        {
            OpenFolderDialog saveFileDialog = new OpenFolderDialog();
            Nullable<bool> result = saveFileDialog.ShowDialog();
            string filePath = "";

            if (result == true)
                filePath = saveFileDialog.FolderName;

            return filePath;
        }

        private static string getFilterExtension(string extension)
        {
            string filter = "";
            switch (extension)
            {
                case "json":
                    filter = "Json files (*.json)|*.json";
                    break;
                case "excel":
                    filter = "Excel Files|*.xls;*.xlsx";
                    break;
                case "txt":
                    filter = "Text Files|*.txt";
                    break;
                case "completed":
                    filter = "Completed Files|*.COMPLETED";
                    break;
                default:
                    filter = "All files (*.*)|*.*";
                    break;
            }

            return filter;
        }

        public static ObservableCollection<Item> selectAllList(ObservableCollection<Item> items, CheckBox checkBox)
        {
            foreach (Item item in items)
            {
                item.IsSelected = (bool)checkBox.IsChecked;
            }

            return items;
        }
    }
}
