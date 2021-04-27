namespace KvmSwitch.Hardware
{

    using System;
    using System.Collections.Generic;

    public interface IUsbDeviceMonitor
    {
        event EventHandler<UsbDeviceEventArgs> UsbDeviceConnected;
        event EventHandler<UsbDeviceEventArgs> UsbDeviceRemoved;
        void Start();
        public IEnumerable<UsbDeviceInfo> GetUSBDevices();
    }
}
