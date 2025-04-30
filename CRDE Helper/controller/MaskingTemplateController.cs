using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

namespace CRDEConverterJsonExcel.controller
{
    class MaskingTemplateController: Controller
    {
        public JObject getMaskingTemplate(string key, string value)
        {
            var resultMaskingTemplate = CRDEConfig.config["MASKING_TEMPLATE"].ToObject<JArray>().Children<JObject>().FirstOrDefault(template => template[key] != null && template[key].ToString() == value);

            return resultMaskingTemplate == null ? null : resultMaskingTemplate.ToObject<JObject>();
        }

        public JArray getMaskingTemplateList()
        {
            return CRDEConfig.config["MASKING_TEMPLATE"].ToObject<JArray>();
        }

        public bool setMaskingTemplate(JArray maskingTemplate)
        {
            try
            {
                ObservableCollection<MaskingTemplate> maskingTemplateRequest = maskingTemplate.ToObject<ObservableCollection<MaskingTemplate>>();

                bool isValidated = false;
                string validateMessage = "";
                foreach (MaskingTemplate mTemplate in maskingTemplateRequest)
                {
                    // Validate Masking Template
                    var validationErrors = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(mTemplate, new ValidationContext(mTemplate) { Items = { { "MaskingTemplateList", maskingTemplateRequest } } }, validationErrors))
                    {
                        //Look at all of the validation errors
                        foreach (var error in validationErrors)
                            validateMessage += error.ErrorMessage + Environment.NewLine;

                        isValidated = false;
                        break;
                    }
                    else
                    {
                        // Validate Masking
                        if (mTemplate.Mask.Count == 0)
                        {
                            isValidated = true;
                        }
                        else
                        {
                            foreach (Masking m in mTemplate.Mask)
                            {
                                var validationMaskingError = new List<ValidationResult>();
                                if (!Validator.TryValidateObject(m, new ValidationContext(m) { Items = { { "MaskingList", mTemplate.Mask } } }, validationMaskingError))
                                {
                                    //Look at all of the validation errors
                                    foreach (var error in validationMaskingError)
                                    {
                                        validateMessage += error.ErrorMessage + Environment.NewLine;
                                    }

                                    isValidated = false;
                                    break;
                                }
                                else
                                    isValidated = true;
                            }
                        }
                    }
                }

                if (isValidated)
                {
                    CRDEConfig.config["MASKING_TEMPLATE"] = maskingTemplate;

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
