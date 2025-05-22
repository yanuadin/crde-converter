using CRDEConverterJsonExcel.controller;
using CRDEConverterJsonExcel.objectClass;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CRDE_Helper.controller
{
    class OverCharacterVariableController: Controller
    {
        public bool setOverCharacterVariable(JObject overCharacterVariables)
        {
            JArray listOverCharacterVariable = CRDEConfig.getListConfig("OVER_CHARACTER_VARIABLE");
            listOverCharacterVariable.Add(overCharacterVariables);
            CRDEConfig.config["OVER_CHARACTER_VARIABLE"] = listOverCharacterVariable;
            File.WriteAllText(CRDEConfig.getFilePathConfig(), CRDEConfig.config.ToString());

            return true;
        }

        public JArray getListOverCharacterVariable()
        {
            JArray listOverCharacterVariable = CRDEConfig.getListConfig("OVER_CHARACTER_VARIABLE");

            return listOverCharacterVariable;
        }

        public JObject getOverCharacterVariable(string key, string value)
        {
            JObject overCharacterVariable = CRDEConfig.getConfig("OVER_CHARACTER_VARIABLE", key, value);

            return overCharacterVariable;
        }
    }
}
