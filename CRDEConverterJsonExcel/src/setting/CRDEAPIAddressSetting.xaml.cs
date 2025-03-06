using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for CRDEAPIAddressSetting.xaml
    /// </summary>
    public partial class CRDEAPIAddressSetting : UserControl
    {
        CRDE config = new CRDE();
        ObservableCollection<Env> environmentList = new ObservableCollection<Env>();

        public CRDEAPIAddressSetting()
        {
            InitializeComponent();

            environmentList = config.getApiAddressList().ToObject<ObservableCollection<Env>>();
            s3_dg_environment.ItemsSource = environmentList;
        }

        private void s3_btn_deleteProcessCode(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                Env env = button.DataContext as Env;
                if (env != null)
                    environmentList.Remove(env);
            }
        }

        private void s3_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if(config.setApiAddress(JArray.FromObject(environmentList)))
                        MessageBox.Show("[SUCCESS]: API address has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }

        private void s3_btn_Restore_Click(object sender, RoutedEventArgs e)
        {
            environmentList = config.getApiAddressList().ToObject<ObservableCollection<Env>>();
            s3_dg_environment.ItemsSource = environmentList;
        }
    }
}
