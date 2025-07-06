using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GTRC_WPF
{
    public class ObservableObject : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
