using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDEConverterJsonExcel.objectClass
{
    class Item
    {
        public string fileName { get; set; }
        public string filePath { get; set; }
        public string json { get; set; }
        public bool isSelected { get; set; }
    }
}
