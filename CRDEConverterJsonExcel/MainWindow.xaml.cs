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
    public MainWindow()
    {
        InitializeComponent();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                t4_uc_S1LogExtractionLocal.refreshConfig();
                break;
            case "t5":
                toolsSubMenu.Visibility = Visibility.Visible;
                settingSubMenu.Visibility = Visibility.Hidden;
                t4_uc_CallingCRDEAPI.refreshConfig();
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

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Refreshing user control that use config
        t4_uc_S1LogExtractionLocal.refreshConfig();
        t4_uc_CallingCRDEAPI.refreshConfig();
    }
}