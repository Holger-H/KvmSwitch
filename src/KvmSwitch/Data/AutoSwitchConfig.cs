namespace KvmSwitch.Data
{
    using System.Collections.Generic;

    /// <summary>
    ///     Config for automatic swichting of devices.
    /// </summary>
    public class AutoSwitchConfig
    {
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="AutoSwitchConfig" /> is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active { get; set; }

        /// <summary>
        ///     Gets or sets the usb device identifier, which triggers on connected the monitor switching.
        /// </summary>
        public string UsbDeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the monitor configs.
        /// </summary>
        public List<MonitorConfig> MonitorConfigs { get; set; }

        /// <summary>
        ///     Gets or sets the file to execute on usb device connected.
        /// </summary>
        public string ExecuteOnUsbDeviceConnected { get; set; }

        /// <summary>
        ///     Gets or sets the file to execute on usb device disconnected.
        /// </summary>
        public string ExecuteOnUsbDeviceDisconnected { get; set; }
    }
}