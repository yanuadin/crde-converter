using CRDEConverterJsonExcel.core;
using CRDEConverterJsonExcel.objectClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for CallingCRDEAPI.xaml
    /// </summary>
    public partial class CallingCRDEAPI : UserControl
    {
        private ObservableCollection<Item> lb_JSONRequestItems = new ObservableCollection<Item>();
        private ObservableCollection<Item> lb_JSONResponseItems = new ObservableCollection<Item>();

        public CallingCRDEAPI()
        {
            InitializeComponent();
        }

        private void t5_btn_BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONRequestItems = GeneralMethod.browseFile("json", true);
                t5_lb_RequestList.ItemsSource = lb_JSONRequestItems;
                t5_tb_folder.Text = string.Join(@"\", lb_JSONRequestItems.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open file");
            }
        }

        private void t5_btn_BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_JSONRequestItems = GeneralMethod.browseFolder("json");
                t5_lb_RequestList.ItemsSource = lb_JSONRequestItems;
                t5_tb_folder.Text = string.Join(@"\", lb_JSONRequestItems.First<Item>().FilePath.Split(@"\")[0..^1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: Failed to open folder");
            }
        }

        private void t5_btn_ClearListBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void t5_cb_SelectAllRequest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void t5_btn_Run_Click(object sender, RoutedEventArgs e)
        {

        }

        private void t5_cb_SelectAllResponse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void t5_btn_ConvertToExcel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
