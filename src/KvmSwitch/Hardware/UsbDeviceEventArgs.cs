namespace KvmSwitch.Hardware
{
    using System;

    public class UsbDeviceEventArgs : EventArgs
    {
        public UsbDeviceEventArgs(UsbDeviceInfo usbDevice)
        {
            this.UsbDevice = usbDevice;
        }

        public UsbDeviceInfo UsbDevice { get; }
    }
}