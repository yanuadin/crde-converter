﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CRDEConverterJsonExcel.core;
using System.Diagnostics;

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

        public JArray getEnvironmentList()
        {
            JArray environmentList = new JArray();
            foreach (JObject env in config["ENVIRONMENT"])
                foreach (var envToken in env)
                    environmentList.Add(envToken.Key);

            return environmentList;
        }

        public JObject getEnvironment(string env)
        {
            JObject envConfig = config["ENVIRONMENT"].Children<JObject>().FirstOrDefault(child =>
            {
                foreach (var ch in child)
                {
                    return ch.Key.ToUpper() == env.ToUpper();
                }
                return false;
            });
            
            return envConfig == null ? null : envConfig[env].ToObject<JObject>();
        }

        public JArray getProcessCode()
        {
            return config["PROCESS_CODE"].ToObject<JArray>();
        }

        public void setProcessCode(JArray processCode)
        {
            config["PROCESS_CODE"] = processCode;
            
            File.WriteAllText(getFilePathConfig(), config.ToString());
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
