namespace KvmSwitch
{
    using System;
    using KvmSwitch.Data;
    using KvmSwitch.Hardware;

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
