using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.controller;
using CRDEConverterJsonExcel.core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CRDEConverterJsonExcel.src.tools
{
    /// <summary>
    /// Interaction logic for LoginAsAdmin.xaml
    /// </summary>
    public partial class LoginAsAdmin : UserControl
    {
        // Define a custom event
        public event EventHandler<bool> DataReady;
        private bool isLogin = false;
        private AdminController adminController = new AdminController();

        public LoginAsAdmin()
        {
            InitializeComponent();
        }

        private void t8_btn_Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pb_password.Password.ToString() != "")
                {
                    if (pb_password.Password.ToString().Equals(AESEncryption.Decrypt(adminController.getAdminLoginPassword())))
                    {
                        pb_password.Password = string.Empty;
                        isLogin = true;
                        DataReady?.Invoke(this, isLogin);
                        MessageBox.Show("[SUCCESS]: Login as Admin");
                    }
                    else
                    {
                        MessageBox.Show("[FAILED]: Password is incorrect");
                    }
                } else
                    MessageBox.Show("[FAILED]: Please input the password");
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }

        private void t8_btn_Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isLogin = false;
                DataReady?.Invoke(this, isLogin);
                MessageBox.Show("[SUCCESS]: Has been logged out");
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ERROR]: " + ex.Message);
            }
        }
    }
}
