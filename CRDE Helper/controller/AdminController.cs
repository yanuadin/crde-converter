using System.IO;
using System.Windows;

namespace CRDEConverterJsonExcel.controller
{
    class AdminController: Controller
    {
        public string getAdminLoginPassword()
        {
            return CRDEConfig.config["ADMIN_LOGIN"]?["Password"]?.ToString();
        }

        public string getSKEY()
        {
            return CRDEConfig.config["S_KEY"]?.ToString();
        }

        public string getSIV()
        {
            return CRDEConfig.config["S_IV"]?.ToString();
        }

        public bool setPassword(string password)
        {
            try
            {
                CRDEConfig.config["ADMIN_LOGIN"]["Password"] = password;
                File.WriteAllText(CRDEConfig.getFilePathConfig(), CRDEConfig.config.ToString());

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("[ERROR]: " + e.Message);

                return false;
            }
        }
    }
}
