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
    /// Interaction logic for MaskingSetting.xaml
    /// </summary>
    public partial class MaskingSetting : UserControl
    {
        MaskingTemplateController maskingTemplateController = new MaskingTemplateController();
        ObservableCollection<MaskingTemplate> maskingTemplateList = new ObservableCollection<MaskingTemplate>();
        ObservableCollection<Masking> maskingList = new ObservableCollection<Masking>();

        public MaskingSetting()
        {
            InitializeComponent();
            maskingTemplateList = maskingTemplateController.getMaskingTemplateList().ToObject<ObservableCollection<MaskingTemplate>>();
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
            {
                maskingList = maskingTemplate.Mask;
                s1_gd_masking.ItemsSource = maskingList;
            }
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
            maskingTemplateController.refreshConfig();
            maskingTemplateList = maskingTemplateController.getMaskingTemplateList().ToObject<ObservableCollection<MaskingTemplate>>();
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
                    if (maskingTemplateController.setMaskingTemplate(JArray.FromObject(maskingTemplateList)))
                        MessageBox.Show("[SUCCESS]: Masking template has been saved successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[FAILED]: " + ex.Message);
            }
        }

        private void s1_tb_SearchMaskingTemplateList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = maskingTemplateList.Where(mTemplate => mTemplate.Name.Contains(s1_tb_SearchMaskingTemplateList.Text, StringComparison.OrdinalIgnoreCase)).ToList<MaskingTemplate>();

            if (search != null)
                s1_gd_template.ItemsSource = search;
        }

        private void s1_tb_SearchMaskingList_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = maskingList.Where(masking => masking.Variable.Contains(s1_tb_SearchMaskingList.Text, StringComparison.OrdinalIgnoreCase) || masking.Value.Contains(s1_tb_SearchMaskingList.Text, StringComparison.OrdinalIgnoreCase)).ToList<Masking>();

            if (search != null)
                s1_gd_masking.ItemsSource = search;
        }
    }
}
