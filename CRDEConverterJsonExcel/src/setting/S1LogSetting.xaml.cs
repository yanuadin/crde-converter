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
    /// Interaction logic for S1LogSetting.xaml
    /// </summary>
    public partial class S1LogSetting : UserControl
    {
        S1LogController s1LogController = new S1LogController();
        ObservableCollection<S1Log> s1LogList = new ObservableCollection<S1Log>();

        public S1LogSetting()
        {
            InitializeComponent();

            s1LogList = s1LogController.getS1LogList().ToObject<ObservableCollection<S1Log>>();
            s2_dg_S1Log.ItemsSource = s1LogList;
        }

        private void s2_btn_S1Log(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                S1Log log = button.DataContext as S1Log;
                if (log != null)
                    s1LogList.Remove(log);
            }
        }

        private void s2_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (s1LogController.setS1Log(JArray.FromObject(s1LogList)))
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
            s1LogController.refreshConfig();
            s1LogList = s1LogController.getS1LogList().ToObject<ObservableCollection<S1Log>>();
            s2_dg_S1Log.ItemsSource = s1LogList;
        }
    }
}
