using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

namespace CRDEConverterJsonExcel.controller
{
    class ProcessCodeController: Controller
    {
        public JArray getProcessCodeList()
        {
            return CRDEConfig.config["PROCESS_CODE"].ToObject<JArray>();
        }

        public bool setProcessCode(JArray processCode)
        {
            try
            {
                ObservableCollection<ProcessCode> processCodeRequest = processCode.ToObject<ObservableCollection<ProcessCode>>();

                bool isValidated = false;
                string validateMessage = "";
                foreach (ProcessCode pCode in processCodeRequest)
                {
                    var validationErrors = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(pCode, new ValidationContext(pCode), validationErrors))
                    {
                        //Look at all of the validation errors
                        foreach (var error in validationErrors)
                            validateMessage += error.ErrorMessage + Environment.NewLine;

                        isValidated = false;
                        break;
                    }
                    else
                        isValidated = true;
                }

                if (isValidated)
                {
                    CRDEConfig.config["PROCESS_CODE"] = processCode;

                    File.WriteAllText(CRDEConfig.getFilePathConfig(), CRDEConfig.config.ToString());
                }
                else
                    MessageBox.Show("[FAILED]: " + Environment.NewLine + validateMessage);

                return isValidated;
            }
            catch (Exception e)
            {
                MessageBox.Show("[ERROR]: " + e.Message);

                return false;
            }
        }
    }
}
