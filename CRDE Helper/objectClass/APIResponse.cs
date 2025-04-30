using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CRDEConverterJsonExcel.objectClass
{
    class APIResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string data { get; set; }
    }
}
