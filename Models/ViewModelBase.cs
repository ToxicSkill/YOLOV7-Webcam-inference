using System.Collections.Generic;
using System.ComponentModel;

namespace YoloV7WebCamInference.Models
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public string? PageName { get; set; }

        public ViewModelBase(string? pageName = null)
        {
            PageName = pageName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual Dictionary<string, object> GetObjects()
        {
            return default;
        }

        public virtual void SetObjects(Dictionary<string, object> objects)
        {
            return;
        }
    }
}