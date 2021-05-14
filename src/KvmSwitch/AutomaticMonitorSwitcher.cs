namespace KvmSwitch
{
    using KvmSwitch.Data;
    using KvmSwitch.Hardware;
    using log4net;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal class AutomaticMonitorSwitcher
    {
        private readonly ILog log = LogManager.GetLogger(typeof(AutomaticMonitorSwitcher));
        private readonly IKvmSwitchEnvironment environment;
        private AutoSwitchConfig autoSwitchConfig;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutomaticMonitorSwitcher" /> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public AutomaticMonitorSwitcher(IKvmSwitchEnvironment environment)
        {
            this.environment = environment;
            this.environment.ConfigChanged += this.EnvironmentOnConfigChanged;
            environment.UsbDeviceMonitor.UsbDeviceConnected += this.UsbDeviceMonitorOnUsbDeviceConnected;
            environment.UsbDeviceMonitor.UsbDeviceRemoved += this.UsbDeviceMonitorOnUsbDeviceRemoved;
            this.AssignAutoSwitchConfig();
        }


        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            if (this.autoSwitchConfig == null)
            {
                return;
            }

            var usbDeviceInfo = this.environment.UsbDeviceMonitor.GetUSBDevices()
                .FirstOrDefault(x => x.DeviceId == this.autoSwitchConfig.UsbDeviceId);

            if (usbDeviceInfo == null)
            {
                return;
            }

            this.HandleConnectedUsbDevice(usbDeviceInfo.DeviceId);
        }

        private void UsbDeviceMonitorOnUsbDeviceConnected(object sender, UsbDeviceEventArgs e)
        {
            this.HandleConnectedUsbDevice(e.UsbDevice.DeviceId);
        }

        private void UsbDeviceMonitorOnUsbDeviceRemoved(object sender, UsbDeviceEventArgs e)
        {
            this.HandleDisconnectedUsbDevice(e.UsbDevice.DeviceId);
        }

        private void HandleConnectedUsbDevice(string usbDeviceId)
        {
            this.log.Debug(nameof(HandleConnectedUsbDevice));

            if (this.autoSwitchConfig == null ||
                this.autoSwitchConfig.UsbDeviceId != usbDeviceId ||
                this.autoSwitchConfig.MonitorConfigs == null)
            {
                return;
            }

            this.environment.DisplayDataChannel.EnsureDisplaySourceIsActive(this.autoSwitchConfig.MonitorConfigs);
            this.ExecuteFile(this.autoSwitchConfig.ExecuteOnUsbDeviceConnected);
        }

        private void HandleDisconnectedUsbDevice(string usbDeviceId)
        {
            this.log.Debug(nameof(HandleDisconnectedUsbDevice));

            if (this.autoSwitchConfig == null ||
                this.autoSwitchConfig.UsbDeviceId != usbDeviceId)
            {
                return;
            }

            this.ExecuteFile(this.autoSwitchConfig?.ExecuteOnUsbDeviceDisconnected);
        }

        private void ExecuteFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                this.log.Info($"Executing file '{filename}'");
                Task.Run(() =>
                {
                    try
                    {
                        Process.Start(filename);
                    }
                    catch (Exception e)
                    {
                        this.log.Error($"Error on {nameof(ExecuteFile)} '{filename}': {e}");
                    }
                });
            }
            else
            {
                if (string.IsNullOrEmpty(filename))
                {
                    this.log.Info($"File '{filename}' can not be executed because it does not exists.");
                }
            }
        }

        private void AssignAutoSwitchConfig()
        {
            this.autoSwitchConfig = this.environment.Config?.AutoSwitchConfigs?.FirstOrDefault(x => x.Active);
        }

        private void EnvironmentOnConfigChanged(object sender, EventArgs e)
        {
            this.AssignAutoSwitchConfig();
        }
    }
}
