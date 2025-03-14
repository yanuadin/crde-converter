using OfficeOpenXml;
using System.Windows;
using System.Windows.Controls;
using CRDEConverterJsonExcel.objectClass;
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
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new JSONConverter() });
                break;
            case "t2":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new ExcelConverter() });
                break;
            case "t3":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new JSONMasking() });
                break;
            case "t4":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new S1LogExtractionLocal() });
                break;
            case "t5":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new CallingCRDEAPI() });
                break;
            case "t6":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new JSONDateTimeToDate() });
                break;
            case "s1":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new MaskingSetting() });
                break;
            case "s2":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new S1LogSetting() });
                break;
            case "s3":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new CRDEAPIAddressSetting() });
                break;
            case "s4":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new ProcessCodeSetting() });
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
        var oldItemControl = tabItemControls.FirstOrDefault(item => item.Header == itemControl.Header);
        if (oldItemControl == null)
        {
            tabItemControls.Add(itemControl);
            subMenu.SelectedItem = itemControl;
        } else
            subMenu.SelectedItem = oldItemControl;

        subMenu.ItemsSource = tabItemControls;
    }
}