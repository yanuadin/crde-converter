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
using System.Collections.ObjectModel;
using CRDEConverterJsonExcel.src.tools;
using CRDEConverterJsonExcel.src.setting;

namespace CRDEConverterJsonExcel;

public partial class MainWindow : Window
{
    ObservableCollection<TabItemControl> tabItemControls = new ObservableCollection<TabItemControl>();
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
                addTabItem(new TabItemControl { Header = "JSON Converter", Content = new JSONConverter() });
                break;
            case "t2":
                addTabItem(new TabItemControl { Header = "Excel Converter", Content = new ExcelConverter() });
                break;
            case "t3":
                addTabItem(new TabItemControl { Header = "JSON Masking", Content = new UserControl() });
                break;
            case "t4":
                addTabItem(new TabItemControl { Header = "S1 Log Extraction", Content = new S1LogExtractionLocal() });
                refreshAllComponentConfig();
                break;
            case "t5":
                addTabItem(new TabItemControl { Header = "Calling CRDE API", Content = new CallingCRDEAPI() });
                refreshAllComponentConfig();
                break;
            case "s1":
                addTabItem(new TabItemControl { Header = "Masking", Content = new UserControl() });
                break;
            case "s2":
                addTabItem(new TabItemControl { Header = "S1 Logs", Content = new UserControl() });
                break;
            case "s3":
                addTabItem(new TabItemControl { Header = "CRDE API Address", Content = new CRDEAPIAddressSetting() });
                break;
            case "s4":
                addTabItem(new TabItemControl { Header = "Process Code", Content = new ProcessCodeSetting() });
                break;
            default:
                MessageBox.Show("[ERROR]: Menu is not available");
                break;
        }

        subMenu.ItemsSource = tabItemControls;
    }

    private void btn_DeleteTabClick(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        TabItemControl tabItemControl = button.DataContext as TabItemControl;
        if (tabItemControl != null)
            tabItemControls.Remove(tabItemControl);

        subMenu.ItemsSource = tabItemControls;
    }

    private void addTabItem(TabItemControl itemControl)
    {
        if (tabItemControls.FirstOrDefault(item => item.Header == itemControl.Header) == null)
        {
            tabItemControls.Add(itemControl);
            subMenu.SelectedItem = itemControl;
        }

        subMenu.ItemsSource = tabItemControls;
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        refreshAllComponentConfig();
    }

    private void refreshAllComponentConfig()
    {
        // Refreshing user control that use config
        foreach (TabItemControl itemControl in tabItemControls)
        {
            var method = itemControl.Content.GetType().GetMethod("refreshConfig");
            if (method != null)
            {
                method.Invoke(itemControl.Content, null);
            }
        }
    }
}