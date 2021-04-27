namespace KvmSwitch.Hardware
{
    public class UsbDeviceInfo
    {
        public UsbDeviceInfo(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public UsbDeviceInfo(string deviceID, string description)
        {
            this.DeviceId = deviceID;
            this.Description = description;
        }

        public UsbDeviceInfo(UsbDeviceInfo usbDeviceInfo)
        {
            this.DeviceId = usbDeviceInfo.DeviceId;
            this.Description = usbDeviceInfo.Description;
        }

        public string DeviceId { get; }

        public string Description { get; internal set; }

        public override string ToString()
        {
            return $"{this.Description} (DeviceId: {this.DeviceId})";
        }
    }
}
