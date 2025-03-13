using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for ProcessCodeSetting.xaml
    /// </summary>
    public partial class ProcessCodeSetting : UserControl
    {
        private CRDE config = new CRDE();
        ObservableCollection<ProcessCode> processCodeList = new ObservableCollection<ProcessCode>();

        public ProcessCodeSetting()
        {
            InitializeComponent();

            processCodeList = config.getProcessCodeList().ToObject<ObservableCollection<ProcessCode>>();
            s4_lb_ProcessCode.ItemsSource = processCodeList;
        }

        private void s4_btn_deleteProcessCode(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                ProcessCode processCode = button.DataContext as ProcessCode;
                if (processCode != null)
                    processCodeList.Remove(processCode);
            }
        }

        private void s4_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (config.setProcessCode(JArray.FromObject(processCodeList)))
                        MessageBox.Show("[SUCCESS]: Process code has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }

        private void s4_btn_Restore_Click(object sender, RoutedEventArgs e)
        {
            processCodeList = config.getProcessCodeList().ToObject<ObservableCollection<ProcessCode>>();
            s4_lb_ProcessCode.ItemsSource = processCodeList;
        }
    }
}
