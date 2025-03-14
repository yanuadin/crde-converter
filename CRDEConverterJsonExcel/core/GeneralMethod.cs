using CRDEConverterJsonExcel.objectClass;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
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

        public static ObservableCollection<Item> browseFile(string[] extension, bool allowedMultipleFiles)
        {
            ObservableCollection<Item> listItem = new ObservableCollection<Item>();

            // Create OpenFileDialog 
            string filter = getFilterExtension(extension);
            string json = "";

            OpenFileDialog dlg = new OpenFileDialog { Filter = filter, Multiselect = allowedMultipleFiles };

            Nullable<bool> result = dlg.ShowDialog();

            // If Allowed MultipleFiles
            string fileName = "";
            string fileExt = "";
            if (result == true)
            {
                if (allowedMultipleFiles)
                {
                    // Open document 
                    foreach (string filePath in dlg.FileNames)
                    {
                        fileName = filePath.Split("\\").Last().Split(".").First();
                        fileExt = filePath.Split("\\").Last().Split(".").Last();

                        if (fileExt == "zip")
                        {
                            Item zipItem = readZipFile(filePath, extension);
                            if (zipItem != null)
                                listItem.Add(zipItem);
                        } else 
                            listItem.Add(new Item { FileName = fileName, FilePath = filePath, CreatedDate = getCreatedDateOfFile(filePath), FileContent = File.ReadAllText(filePath), IsSelected = false });
                    }
                }
                else
                {
                    fileName = dlg.FileName.Split("\\").Last().Split(".").First();
                    fileExt = dlg.FileName.Split("\\").Last().Split(".").Last();
                    json = extension.Contains("json") ? File.ReadAllText(dlg.FileName) : "";

                    if (fileExt == "zip")
                    {
                        Item zipItem = readZipFile(dlg.FileName, extension);
                        if (zipItem != null)
                            listItem.Add(zipItem);
                    } else 
                        listItem.Add(new Item { FileName = fileName, FilePath = dlg.FileName, CreatedDate = getCreatedDateOfFile(dlg.FileName), FileContent = json, IsSelected = false });
                }
            }
            
            return listItem;
        }

        public static ObservableCollection<Item> browseFolder(string[] extension)
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

                    if (fileExt == "zip")
                    {
                        Item zipItem = readZipFile(filePath, extension);
                        if (zipItem != null)
                            listItem.Add(zipItem);
                    }
                    else if (extension.Contains(fileExt.ToLower()))
                        listItem.Add(new Item { FileName = fileName, FilePath = filePath, FileContent = File.ReadAllText(filePath), CreatedDate = getCreatedDateOfFile(filePath), IsSelected = false });
                }
            }

            return listItem;
        }

        public static Item readZipFile(string zipFilePath, string[] extension)
        {
            try
            {
                // Open the zip file
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    // Iterate through each entry in the zip file
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string fileName = entry.FullName.Split("\\").Last().Split(".").First();
                                string fileExt = entry.FullName.Split("\\").Last().Split(".").Last();

                                if (extension.Contains(fileExt.ToLower()))
                                    return new Item { FileName = fileName, FileContent = reader.ReadToEnd(), CreatedDate = getCreatedDateOfFile(zipFilePath), IsSelected = false };
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }

            return null;
        }

        public static string saveFileDialog(string[] extension, string defaultName = "")
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

        private static string getFilterExtension(string[] extension)
        {
            string resultFilter = "";
            for (int i = 0; i < extension.Length; i++)
            {
                string filter = i == 0 ? "" : "|";
                switch (extension[i])
                {
                    case "json":
                        filter += "Json files (*.json)|*.json";
                        break;
                    case "excel":
                        filter += "Excel Files|*.xls;*.xlsx";
                        break;
                    case "txt":
                        filter += "Text Files|*.txt";
                        break;
                    case "completed":
                        filter += "Completed Files|*.COMPLETED";
                        break;
                    case "zip":
                        filter += "ZIP Files|*.zip";
                        break;
                    default:
                        filter += "All files (*.*)|*.*";
                        break;
                }
                resultFilter += filter;
            }

            return resultFilter;
        }

        public static ObservableCollection<Item> selectAllList(ObservableCollection<Item> items, CheckBox checkBox)
        {
            foreach (Item item in items)
            {
                item.IsSelected = (bool)checkBox.IsChecked;
            }

            return items;
        }

        private static string getCreatedDateOfFile(string filePath)
        {
            string dateCreated = "";
            FileInfo oFileInfo = new FileInfo(filePath);

            // Assign Date Created Properties
            if (oFileInfo.Exists)
            {
                DateTime dtCreationTime = oFileInfo.CreationTime;
                dateCreated = dtCreationTime.ToString();
            }

            return dateCreated;
        }
    }
}
