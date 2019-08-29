using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BMC
{
    abstract class BindableProperty: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void ChangeProperty<T>(ref T target, T newValue, string[] dependencies = null, [CallerMemberName] string propertyName = "")
        {
            target = newValue;
            RaisePropertyChanged(propertyName);

            // For computed properties that depend on the current property
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    RaisePropertyChanged(dependency);
                }
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
