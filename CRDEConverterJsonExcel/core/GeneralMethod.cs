using CRDEConverterJsonExcel.objectClass;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CRDEConverterJsonExcel.core
{
    class GeneralMethod
    {
        public static string getProjectDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static string getTimeStampNow()
        {
            return DateTime.Now.ToString("dd_MM_yyyy-HH_mm");
        }

        public static List<Item> browseFile(string extension, bool allowedMultipleFiles)
        {
            List<Item> listItem = new List<Item>();

            // Create OpenFileDialog 
            string filter = "";
            switch (extension)
            {
                case "json":
                    filter = "Json files (*.json)|*.json";
                    break;
                case "excel":
                    filter = "Excel Files|*.xls;*.xlsx";
                    break;
                case "completed":
                    filter = "Completed Files|*.COMPLETED";
                    break;
                default:
                    filter = "Json files (*.json)|*.json|Excel Files|*.xls;*.xlsx";
                    break;
            }

            OpenFileDialog dlg = new OpenFileDialog { Filter = filter, Multiselect = allowedMultipleFiles };

            // Display OpenFileDialog by calling ShowDialog method 
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

                        listItem.Add(new Item { fileName = fileName, filePath = filePath, json = File.ReadAllText(filePath), isSelected = false });
                    }
                }
                else
                {
                    fileName = dlg.FileName.Split("\\").Last().Split(".").First();

                    listItem.Add(new Item { fileName = fileName, filePath = dlg.FileName, json = File.ReadAllText(dlg.FileName), isSelected = false });
                }
            }

            return listItem;
        }

        public static List<Item> browseFolder(string extension)
        {
            List<Item> listItem = new List<Item>();

            if (extension == "json")
            {
                OpenFolderDialog folderDialog = new OpenFolderDialog();
                JArray files = new JArray();

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = folderDialog.ShowDialog();

                if (result == true)
                {
                    string[] filePaths = Directory.GetFiles(folderDialog.FolderName);

                    foreach (string filePath in filePaths)
                    {
                        string fileName = filePath.Split("\\").Last().Split(".").First();
                        string fileExt = filePath.Split("\\").Last().Split(".").Last();

                        if (fileExt == extension)
                            listItem.Add(new Item { fileName = fileName, filePath = filePath, json = File.ReadAllText(filePath), isSelected = false });
                    }
                }
            }
            else
            {
                MessageBox.Show("[ERROR]: Extension Is Not Defined");
            }

            return listItem;
        }
    }
}
