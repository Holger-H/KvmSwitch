namespace KvmSwitch.UI
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using KvmSwitch.Hardware;

    public class UsbDeviceInfoModel : UsbDeviceInfo, INotifyPropertyChanged
    {
        private UsbDeviceState state;

        public UsbDeviceInfoModel(UsbDeviceInfo usbDeviceInfo, UsbDeviceState state) : base(usbDeviceInfo)
        {
            this.State = state;
        }

        public UsbDeviceState State
        {
            get => this.state;
            set
            {
                if (this.state == value)
                {
                    return;
                }

                this.state = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
