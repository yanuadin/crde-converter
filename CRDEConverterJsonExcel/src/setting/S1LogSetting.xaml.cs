using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
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

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for S1LogSetting.xaml
    /// </summary>
    public partial class S1LogSetting : UserControl
    {
        CRDE config = new CRDE();
        ObservableCollection<Env> environmentList = new ObservableCollection<Env>();

        public S1LogSetting()
        {
            InitializeComponent();

            environmentList = config.getEnvironmentList().ToObject<ObservableCollection<Env>>();
            s2_dg_S1Log.ItemsSource = environmentList;
        }

        private void s2_btn_S1Log(object sender, RoutedEventArgs e)
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

        private void s2_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (config.setApiAddress(JArray.FromObject(environmentList)))
                        MessageBox.Show("[SUCCESS]: S1 Log has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }

        private void s2_btn_Restore_Click(object sender, RoutedEventArgs e)
        {
            config = new CRDE();
            environmentList = config.getEnvironmentList().ToObject<ObservableCollection<Env>>();
            s2_dg_S1Log.ItemsSource = environmentList;
        }
    }
}
