﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CRDEConverterJsonExcel.core;
using System.Diagnostics;
using CRDEConverterJsonExcel.objectClass;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace CRDEConverterJsonExcel.config
{
    class CRDE
    {
        private JObject config;
        public CRDE() {
            readFileConfig();
        }

        public JArray getColorCells()
        {
            return config["COLOR_CELLS"].ToObject<JArray>();
        }

        public JArray getEnvironmentNameList()
        {
            JArray environmentList = new JArray();
            foreach (JObject env in config["ENVIRONMENT"])
                foreach (var envToken in env)
                    environmentList.Add(envToken.Key);

            return environmentList;
        }

        public JObject getEnvironment(string env)
        {
            var envConfig = config["ENVIRONMENT"].Children<JObject>().FirstOrDefault(child =>
            {
                foreach (var ch in child)
                {
                    return ch.Key.ToUpper() == env.ToUpper();
                }
                return false;
            });

            JObject result = null;
            if (envConfig !=  null)
            {
                result = envConfig[env].ToObject<JObject>();
                result["Name"] = env;
            }
            
            return result;
        }

        public JArray getProcessCodeList()
        {
            return config["PROCESS_CODE"].ToObject<JArray>();
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
                    config["PROCESS_CODE"] = processCode;

                    File.WriteAllText(getFilePathConfig(), config.ToString());
                } else
                    MessageBox.Show("[FAILED]: " + Environment.NewLine + validateMessage);

                return isValidated;
            }
            catch (Exception e)
            {
                MessageBox.Show("[ERROR]: " + e.Message);

                return false;
            }
        }

        public JArray getEnvironmentList()
        {
            JArray apiAddressList = new JArray();
            foreach (JObject env in config["ENVIRONMENT"])
            {
                JObject environment = new JObject();
                foreach (var envToken in env)
                {
                    environment["Name"] = envToken.Key;
                    environment["API"] = envToken.Value["API"]?.ToString() ?? "";
                    environment["HostName"] = envToken.Value["HostName"]?.ToString() ?? "";
                    environment["Port"] = envToken.Value["Port"]?.ToString() ?? "";
                    environment["AccessKeyID"] = envToken.Value["AccessKeyID"]?.ToString() ?? "";
                    environment["SecretAccessKey"] = envToken.Value["SecretAccessKey"]?.ToString() != null && envToken.Value["SecretAccessKey"]?.ToString() != "" ? "XXXXX" : "" ?? "";
                    environment["DirectoryS1"] = envToken.Value["DirectoryS1"]?.ToString() ?? "";
                }
                apiAddressList.Add(environment);
            }

            return apiAddressList;
        }

        public bool setApiAddress(JArray environment)
        {
            try
            {
                ObservableCollection<Env> envrontmentRequest = environment.ToObject<ObservableCollection<Env>>();

                bool isValidated = false;
                string validateMessage = "";
                JArray newEnvConfig = new JArray();
                foreach (Env e in envrontmentRequest)
                {
                    var validationErrors = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(e, new ValidationContext(e) { Items = { { "EnvironmentList", envrontmentRequest } }}, validationErrors))
                    {
                        //Look at all of the validation errors
                        foreach (var error in validationErrors)
                            validateMessage += error.ErrorMessage + Environment.NewLine;
                        
                        isValidated = false;
                        break;
                    } else
                    {
                        JObject newEndPoint = new JObject();
                        JObject newEnvName = new JObject();
                        JObject oldEnv = getEnvironment(e.Name);

                        newEndPoint["API"] = e.API;
                        newEndPoint["HostName"] = e.HostName;
                        newEndPoint["Port"] = e.Port;
                        newEndPoint["AccessKeyID"] = e.AccessKeyID;
                        newEndPoint["SecretAccessKey"] = e.SecretAccessKey == "XXXXX" ? oldEnv["SecretAccessKey"] : e.SecretAccessKey;
                        newEndPoint["DirectoryS1"] = e.DirectoryS1;
                        newEnvName[e.Name] = newEndPoint;
                        newEnvConfig.Add(newEnvName);
                        isValidated = true;
                    }
                }

                if (isValidated)
                {
                    config["ENVIRONMENT"] = newEnvConfig;

                    File.WriteAllText(getFilePathConfig(), config.ToString());
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

        public JObject getMaskingTemplate(string maskingTemplateName)
        {
            var resultMaskingTemplate = config["MASKING_TEMPLATE"].ToObject<JArray>().Children<JObject>().FirstOrDefault(template => template["Name"].ToString() == maskingTemplateName);

            return resultMaskingTemplate == null ? null : resultMaskingTemplate.ToObject<JObject>();
        }

        public JArray getMaskingTemplateList()
        {
            return config["MASKING_TEMPLATE"].ToObject<JArray>();
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
                        } else
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
                    config["MASKING_TEMPLATE"] = maskingTemplate;

                    File.WriteAllText(getFilePathConfig(), config.ToString());
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


        private string getFilePathConfig()
        {
            return GeneralMethod.getProjectDirectory() + @"\config\CRDE.json";
        }

        private void readFileConfig()
        {
            string jsonContent = File.ReadAllText(getFilePathConfig());
            config = JObject.Parse(jsonContent);
        }
    }
}
