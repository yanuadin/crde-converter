using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

namespace CRDEConverterJsonExcel.controller
{
    class APIAddressController: Controller
    {
        public JObject getAPIAddress(string key, string value)
        {
            return CRDEConfig.getConfig("API_ADDRESS", key, value);
        }

        public JArray getAPIAddressList()
        {
            return CRDEConfig.config["API_ADDRESS"].ToObject<JArray>();
        }

        public JArray getAPIAddressNameList()
        {
            JArray apiAddressList = new JArray();
            foreach (JObject api in CRDEConfig.config["API_ADDRESS"])
                apiAddressList.Add(api["Name"].ToString());

            return apiAddressList;
        }

        public bool setAPIAddress(JArray apiAddress)
        {
            try
            {
                ObservableCollection<APIAddress> apiAddressRequest = apiAddress.ToObject<ObservableCollection<APIAddress>>();

                bool isValidated = false;
                string validateMessage = "";
                foreach (APIAddress request in apiAddressRequest)
                {
                    var validationErrors = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(request, new ValidationContext(request) { Items = { { "APIAddressList", request } } }, validationErrors))
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
                    CRDEConfig.config["API_ADDRESS"] = apiAddress;

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
