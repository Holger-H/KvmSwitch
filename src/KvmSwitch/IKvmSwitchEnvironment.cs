namespace KvmSwitch
{
    using KvmSwitch.Data;
    using KvmSwitch.Hardware;
    using System;

    public interface IKvmSwitchEnvironment : IDisposable
    {
        IUsbDeviceMonitor UsbDeviceMonitor { get; }
        Config Config { get; }

        IDisplayDataChannel DisplayDataChannel { get; }

        event EventHandler ConfigChanged;
        void SaveConfiguration(Config config);
        void Initialize();
    }
}
