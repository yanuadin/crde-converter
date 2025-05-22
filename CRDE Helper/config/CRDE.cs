using Newtonsoft.Json.Linq;
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
    public class CRDE
    {
        public JObject config;

        public CRDE() {
            string jsonContent = File.ReadAllText(getFilePathConfig());
            config = JObject.Parse(jsonContent);
        }

        public JArray getColorCells()
        {
            return config["COLOR_CELLS"].ToObject<JArray>();
        }

        public JObject getConfig(string configKey, string variableKey, string variableValue)
        {
            var envConfig = config[configKey].Children<JObject>().FirstOrDefault(child => child[variableKey] != null && child[variableKey].ToString().Equals(variableValue));

            JObject result = null;
            if (envConfig !=  null)
                result = envConfig.ToObject<JObject>();

            return result;
        }

        public string getFilePathConfig()
        {
            return GeneralMethod.getProjectDirectory() + @"\config\CRDE.json";
        }

        public JArray getListConfig(string configKey)
        {
            return config[configKey].ToObject<JArray>();
        }
    }
}
