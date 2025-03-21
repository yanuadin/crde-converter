using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.controller;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for CRDEAPIAddressSetting.xaml
    /// </summary>
    public partial class CRDEAPIAddressSetting : UserControl
    {
        APIAddressController apiAddressController = new APIAddressController();
        ObservableCollection<APIAddress> apiAddressList = new ObservableCollection<APIAddress>();

        public CRDEAPIAddressSetting()
        {
            InitializeComponent();

            apiAddressList = apiAddressController.getAPIAddressList().ToObject<ObservableCollection<APIAddress>>();
            s3_dg_environment.ItemsSource = apiAddressList;
        }

        private void s3_btn_deleteProcessCode(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                APIAddress api = button.DataContext as APIAddress;
                if (api != null)
                    apiAddressList.Remove(api);
            }
        }

        private void s3_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if(apiAddressController.setAPIAddress(JArray.FromObject(apiAddressList)))
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
            apiAddressController.refreshConfig();
            apiAddressList = apiAddressController.getAPIAddressList().ToObject<ObservableCollection<APIAddress>>();
            s3_dg_environment.ItemsSource = apiAddressList;
        }
    }
}
