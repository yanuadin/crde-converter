using OfficeOpenXml;
using System.Windows;
using System.Windows.Controls;
using CRDEConverterJsonExcel.objectClass;
using System.Collections.ObjectModel;
using CRDEConverterJsonExcel.src.tools;
using CRDEConverterJsonExcel.src.setting;
using System.Runtime.CompilerServices;
using MaterialDesignThemes.Wpf;

namespace CRDEConverterJsonExcel;

public partial class MainWindow : Window
{
    ObservableCollection<TabItemControl> tabItemControls = new ObservableCollection<TabItemControl>();
    LoginAsAdmin loginAsAdmin = new LoginAsAdmin();
    bool isLogin = false;

    MenuItem toolsMenu = new MenuItem { Header = "Tools" };
    MenuItem settingMenu = new MenuItem { Header = "Setting", Visibility = Visibility.Hidden };

    public MainWindow()
    {
        InitializeComponent();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        mainMenu.Items.Add(toolsMenu);
        mainMenu.Items.Add(settingMenu);

        addMenuItem();
        // Subscribe to the event
        loginAsAdmin.DataReady += (sender, data) =>
        {
            isLogin = data;
            if (isLogin)
            {
                addAdminMenuItem();
                loginAsAdmin.pb_password.Visibility = Visibility.Hidden;
                loginAsAdmin.t8_btn_Login.Visibility = Visibility.Hidden;
                loginAsAdmin.t8_btn_Logout.Visibility = Visibility.Visible;
            } else
            {
                addMenuItem();
                loginAsAdmin.pb_password.Visibility = Visibility.Visible;
                loginAsAdmin.t8_btn_Login.Visibility = Visibility.Visible;
                loginAsAdmin.t8_btn_Logout.Visibility = Visibility.Hidden;
            }
        };
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
            case "t7":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new S1LogExtractionServer() });
                break;
            case "t8":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = loginAsAdmin });
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
            case "s5":
                addTabItem(new TabItemControl { Header = menuItem.Header.ToString(), Content = new AdminSetting() });
                break;
            default:
                MessageBox.Show("[ERROR]: Menu is not available");
                break;
        }

        subMenu.ItemsSource = tabItemControls;
    }

    private void addMenuItem()
    {
        toolsMenu.Items.Clear();
        settingMenu.Items.Clear();

        settingMenu.Visibility = Visibility.Hidden;

        MenuItem t1 = new MenuItem { Header = "JSON Converter", Tag = "t1" };
        MenuItem t2 = new MenuItem { Header = "Excel Converter", Tag = "t2" };
        MenuItem t6 = new MenuItem { Header = "JSON Datetime to Date", Tag = "t6" };
        MenuItem t3 = new MenuItem { Header = "JSON Masking", Tag = "t3" };
        MenuItem t5 = new MenuItem { Header = "Calling CRDE API", Tag = "t5" };
        MenuItem t8 = new MenuItem { Header = "Login As Admin", Tag = "t8" };

        t1.Click += mi_Control_Click;
        t2.Click += mi_Control_Click;
        t6.Click += mi_Control_Click;
        t3.Click += mi_Control_Click;
        t5.Click += mi_Control_Click;
        t8.Click += mi_Control_Click;

        toolsMenu.Items.Add(t1);
        toolsMenu.Items.Add(t2);
        toolsMenu.Items.Add(t6);
        toolsMenu.Items.Add(t3);
        toolsMenu.Items.Add(t5);
        toolsMenu.Items.Add(t8);
    }

    private void addAdminMenuItem()
    {
        addMenuItem();

        MenuItem t4 = new MenuItem { Header = "S1 Log Extraction (Local)", Tag = "t4" };
        MenuItem t7 = new MenuItem { Header = "S1 Log Extraction (Server)", Tag = "t7" };

        MenuItem s1 = new MenuItem { Header = "Masking", Tag = "s1" };
        MenuItem s2 = new MenuItem { Header = "S1 Logs", Tag = "s2" };
        MenuItem s3 = new MenuItem { Header = "CRDE API Address", Tag = "s3" };
        MenuItem s4 = new MenuItem { Header = "Process Code", Tag = "s4" };
        MenuItem s5 = new MenuItem { Header = "Admin Credential", Tag = "s5" };

        t4.Click += mi_Control_Click;
        t7.Click += mi_Control_Click;

        s1.Click += mi_Control_Click;
        s2.Click += mi_Control_Click;
        s3.Click += mi_Control_Click;
        s4.Click += mi_Control_Click;
        s5.Click += mi_Control_Click;

        toolsMenu.Items.Add(t4);
        toolsMenu.Items.Add(t7);

        settingMenu.Items.Add(s1);
        settingMenu.Items.Add(s2);
        settingMenu.Items.Add(s3);
        settingMenu.Items.Add(s4);
        settingMenu.Items.Add(s5);

        settingMenu.Visibility = Visibility.Visible;
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