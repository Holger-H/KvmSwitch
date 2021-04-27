namespace KvmSwitch.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Management;

    public class UsbDeviceMonitor : IUsbDeviceMonitor
    {
        private readonly BackgroundWorker bgwDriveDetector = new();

        public event EventHandler<UsbDeviceEventArgs> UsbDeviceConnected;
        public event EventHandler<UsbDeviceEventArgs> UsbDeviceRemoved;

        public void Start()
        {
            this.bgwDriveDetector.DoWork += this.bgwDriveDetector_DoWork;
            this.bgwDriveDetector.RunWorkerAsync();
        }

        public IEnumerable<UsbDeviceInfo> GetUSBDevices()
        {
            var devices = new List<UsbDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
            {
                collection = searcher.Get();
            }

            foreach (var device in collection)
            {
                devices.Add(CreateUsbDeviceInfo(device));
            }

            collection.Dispose();
            return devices;
        }

        private void bgwDriveDetector_DoWork(object sender, DoWorkEventArgs e)
        {
            var insertQuery = new WqlEventQuery(
                "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            var insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += this.DeviceInsertedEvent;
            insertWatcher.Start();

            var removeQuery = new WqlEventQuery(
                "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            var removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += this.DeviceRemovedEvent;
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            var usbDeviceInfo = MapEventArgs(e);
            UsbDeviceConnected?.Invoke(this, new UsbDeviceEventArgs(usbDeviceInfo));
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var usbDeviceInfo = MapEventArgs(e);
            UsbDeviceRemoved?.Invoke(this, new UsbDeviceEventArgs(usbDeviceInfo));
        }

        private static UsbDeviceInfo MapEventArgs(EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            return CreateUsbDeviceInfo(instance);
        }

        private static UsbDeviceInfo CreateUsbDeviceInfo(ManagementBaseObject device)
        {
            var deviceId = (string)device.GetPropertyValue("DeviceID");
            var description = (string)device.GetPropertyValue("Description");
            return new UsbDeviceInfo(deviceId) { Description = description };
        }
    }
}
