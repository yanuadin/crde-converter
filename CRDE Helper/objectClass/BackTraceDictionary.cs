using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDE_Helper.objectClass
{
    class BackTraceDictionary
    {
        public string SheetName { get; set; } = "";
        public int RowCount { get; set; } = 0;
        public List<string> Headers { get; set; } = new List<string>();

        public int ChunkStartRow { get; set; } = 3;
        public int ChunkSelectedCol { get; set; } = 3;

        public Int64 ChunkLastId { get; set; } = 1;
        public int ChunkParentCol { get; set; } = 1;
    }
}
