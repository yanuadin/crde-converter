using System.ComponentModel;

namespace CRDEConverterJsonExcel.objectClass
{
    class Item : INotifyPropertyChanged
    {
        private bool _isSelected = false;
        public string FileName { get; set; }
        public string FilePath { get; set; } = "";
        public string FileExt { get; set; } = "";
        public string FileContent { get; set; } = "";
        public string CreatedDate { get; set; } = "";
        public string AdditionalField { get; set; } = "";

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
