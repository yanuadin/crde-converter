using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

namespace CRDEConverterJsonExcel.controller
{
    class S1LogController: Controller
    {
        public JObject getS1Log(string key, string value)
        {
            return CRDEConfig.getConfig("S1_LOG", key, value);
        }

        public JArray getS1LogList()
        {
            return CRDEConfig.config["S1_LOG"].ToObject<JArray>();
        }

        public JArray getS1LogNameList()
        {
            JArray S1LogList = new JArray();
            foreach (JObject log in CRDEConfig.config["S1_LOG"])
                S1LogList.Add(log["Name"].ToString());

            return S1LogList;
        }

        public bool setS1Log(JArray s1Log)
        {
            try
            {
                ObservableCollection<S1Log> S1LogRequest = s1Log.ToObject<ObservableCollection<S1Log>>();

                bool isValidated = false;
                string validateMessage = "";
                foreach (S1Log request in S1LogRequest)
                {
                    var validationErrors = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(request, new ValidationContext(request) { Items = { { "S1LogList", request } } }, validationErrors))
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
                    CRDEConfig.config["S1_LOG"] = s1Log;

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
