using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using static OfficeOpenXml.ExcelErrorValue;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.PortableExecutable;
using System;
using System.Windows.Markup;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using CRDEConverterJsonExcel.core;
using System.Net.Http.Json;
using System.Xml.Linq;
using System.Collections;
using CRDEConverterJsonExcel.config;
using System.Data;
using System.Net;
using CRDEConverterJsonExcel.objectClass;
using System.Security.Cryptography;

namespace CRDEConverterJsonExcel;

public partial class MainWindow : Window
{
    Converter converter = new Converter();
    List<Item> lb_requestItems = new List<Item>();
    private CRDE config = new CRDE();

    public MainWindow()
    {
        InitializeComponent();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus
        // Initialize Endpoint Combobox
        //X: cb_endpoint.Items.Add(config.getEnvironment()["ENDPOINT_REQUEST"]);
    }

    private void btn_ExtractLogsToJSON_Click(object sender, RoutedEventArgs e)
    {
        // Browse for the Excel file
        //JArray files = btn_BrowseFile_Click(sender, e, "completed", true);
        JArray files = new JArray();
        string processCode = "CIMBNiaga_Mortgage";

        foreach (JObject file in files)
        {
            try
            {
                string filePath = file["path"].ToString();
                string fileName = file["name"].ToString();
                string jsonContent = File.ReadAllText(filePath);
                JArray contentFile = new JArray();

                using (TextReader reader = new StreamReader(filePath))
                {
                    string line;
                    int lineNumber = 1;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // RUNID
                        if (lineNumber % 3 == 1)
                        {
                            string[] splitLine = line.Split("#");
                            JObject arrangeRunId = new JObject();
                            foreach (string runId in splitLine)
                            {
                                string[] runIdSplit = runId.Split(":");
                                if (runIdSplit.Count() > 1)
                                {
                                    arrangeRunId[runIdSplit[0]] = runIdSplit[1];
                                }
                            }
                            contentFile.Add(arrangeRunId);
                        }

                        // REQ

                        // IO
                        if (lineNumber % 3 == 0)
                        {
                            string[] splitLine = line.Split('\t');

                            if (splitLine.Count() > 1)
                            {
                                // Get JSON String
                                JArray jsonColletion = new JArray();
                                for (int i = 2; i < splitLine.Count(); i++)
                                {
                                    if (splitLine[i] != "")
                                        jsonColletion.Add(splitLine[i]);
                                }
                                contentFile[lineNumber / 3 - 1]["IO_JSON"] = jsonColletion;
                            }
                        }

                        lineNumber++;
                    }

                    // Convert IO_JSON to JSON File
                    foreach (JObject content in contentFile)
                    {
                        if (content["PROCESSCODE"].ToString() == processCode)
                        {
                            for (int i = 0; i < content["IO_JSON"].Count(); i++)
                            {
                                // Save Response to JSON File
                                string typeJSON = i == 0 ? "req" : "res";
                                string typeOutputFolder = i == 0 ? "request" : "response";
                                string formattingIndentJSON = JsonConvert.SerializeObject(content["IO_JSON"][i], Formatting.Indented);

                                converter.saveTextFile(@"\output\json\" + typeOutputFolder + @"\" + content["REQUESTID"] + ".json", formattingIndentJSON, typeJSON);
                            }
                        }
                    }

                    MessageBox.Show(@"[SUCCESS]: File was saved at \output\json\request and \output\json\response");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }
    }

    private void btnSendRequestToAPI_Click(object sender, RoutedEventArgs e)
    {
        //X: if (cb_endpoint.Text == "")
        if (true)
        {
            MessageBox.Show("[WARNING]: Please select an endpoint!");
        }
        else
        {
            // Flush response list item
            //X: lb_responseList.Items.Clear();

            // Send Request to API
            List<Item> selectedRequestItem = lb_requestItems.FindAll(item => item.isSelected == true);
            int iterator = 0;
            if (selectedRequestItem.Count > 0)
            {
                foreach (Item it in selectedRequestItem)
                {
                    //X: Api.postRequestCRDE(cb_endpoint.Text, it.json, it.fileName, iterator);
                }
            }
            else
            {
                MessageBox.Show("[WARNING]: Please select at least one request to send!");
            }
        }
    }

    private void mi_Control_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;

        switch (menuItem.Tag)
        {
            case "t1":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                break;
            case "t2":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                break;
            case "t3":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                break;
            case "t4":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                break;
            case "t5":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                break;
            case "s1":
                settingSubMenu.Visibility = Visibility.Visible;
                toolsSubMenu.Visibility = Visibility.Hidden;
                break;
            case "s2":
                settingSubMenu.Visibility = Visibility.Visible;
                toolsSubMenu.Visibility = Visibility.Hidden;
                break;
            case "s3":
                settingSubMenu.Visibility = Visibility.Visible;
                toolsSubMenu.Visibility = Visibility.Hidden;
                break;
            case "s4":
                settingSubMenu.Visibility = Visibility.Visible;
                toolsSubMenu.Visibility = Visibility.Hidden;
                break;
            default:
                MessageBox.Show("[ERROR]: Menu is not available");
                break;
        }
    }
}