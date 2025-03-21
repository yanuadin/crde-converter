using CRDEConverterJsonExcel.config;

namespace CRDEConverterJsonExcel.controller
{
    public class Controller
    {
        protected CRDE CRDEConfig;

        public Controller()
        {
            refreshConfig();
        }

        public void refreshConfig()
        {
            CRDEConfig = new CRDE();
        }
    }
}
