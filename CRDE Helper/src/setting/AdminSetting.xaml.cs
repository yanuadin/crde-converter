using CRDEConverterJsonExcel.controller;
using CRDEConverterJsonExcel.core;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.setting
{
    /// <summary>
    /// Interaction logic for AdminSetting.xaml
    /// </summary>
    public partial class AdminSetting : UserControl
    {
        private AdminController adminController = new AdminController();

        public AdminSetting()
        {
            InitializeComponent();
        }

        private void t8_btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pb_password.Password.ToString() != "")
                {
                    adminController.setPassword(AESEncryption.Encrypt(pb_password.Password.ToString()));

                    MessageBox.Show("[SUCCESS]: Password has been saved");
                } else
                    MessageBox.Show("[FAILED]: Please input the password");
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }
    }
}
