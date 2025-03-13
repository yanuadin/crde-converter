using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for MaskingSetting.xaml
    /// </summary>
    public partial class MaskingSetting : UserControl
    {
        CRDE config = new CRDE();
        ObservableCollection<MaskingTemplate> maskingTemplateList = new ObservableCollection<MaskingTemplate>();

        public MaskingSetting()
        {
            InitializeComponent();
            maskingTemplateList = config.getMaskingTemplateList().ToObject<ObservableCollection<MaskingTemplate>>();
            s1_gd_template.ItemsSource = maskingTemplateList;
        }

        private void s1_btn_DeleteTemplateClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                MaskingTemplate maskingTemplate = button.DataContext as MaskingTemplate;
                if (maskingTemplate != null)
                {
                    maskingTemplateList.Remove(maskingTemplate);
                    s1_gd_masking.ItemsSource = new ObservableCollection<Masking>();
                }
            }
        }
        
        private void s1_btn_ShowMaskingListClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            MaskingTemplate maskingTemplate = button.DataContext as MaskingTemplate;
            if (maskingTemplate != null)
                s1_gd_masking.ItemsSource = maskingTemplate.Mask;
        }

        private void s1_btn_DeleteMaskingClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Button button = sender as Button;
                Masking mask = button.DataContext as Masking;
                if (mask != null)
                {
                    var maskingTemplate = maskingTemplateList.FirstOrDefault(mt => mt.Mask.Contains(mask));
                    if (maskingTemplate != null)
                    {
                        maskingTemplate.Mask.Remove(mask);
                        s1_gd_masking.ItemsSource = maskingTemplate.Mask;
                    }
                }
            }
        }

        private void s1_btn_Restore_Click(object sender, RoutedEventArgs e)
        {

            maskingTemplateList = config.getMaskingTemplateList().ToObject<ObservableCollection<MaskingTemplate>>();
            s1_gd_template.ItemsSource = maskingTemplateList;
            s1_gd_masking.ItemsSource = new ObservableCollection<Masking>();
        }

        private void s1_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Save Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (config.setMaskingTemplate(JArray.FromObject(maskingTemplateList)))
                        MessageBox.Show("[SUCCESS]: Masking template has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }
    }
}
