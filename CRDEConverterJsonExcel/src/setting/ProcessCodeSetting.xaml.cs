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
    /// Interaction logic for ProcessCodeSetting.xaml
    /// </summary>
    public partial class ProcessCodeSetting : UserControl
    {
        private CRDE config = new CRDE();
        ObservableCollection<ProcessCode> processCodeList = new ObservableCollection<ProcessCode>();

        public ProcessCodeSetting()
        {
            InitializeComponent();

            processCodeList = config.getProcessCode().ToObject<ObservableCollection<ProcessCode>>();
            s4_lb_ProcessCode.ItemsSource = processCodeList;
        }

        private void s4_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    config.setProcessCode(JArray.FromObject(processCodeList));
                    MessageBox.Show("[SUCCESS]: Process code has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
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

        private void s4_btn_Restore_Click(object sender, RoutedEventArgs e)
        {
            processCodeList = config.getProcessCode().ToObject<ObservableCollection<ProcessCode>>();
            s4_lb_ProcessCode.ItemsSource = processCodeList;
        }
    }
}
