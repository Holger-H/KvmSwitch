namespace KvmSwitch.UI
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Data;
    using Hardware;

    public class AutoSwitchConfigViewModel : ViewModelBase
    {
        private readonly IKvmSwitchEnvironment environment;
        private bool active;
        private string name;
        private MonitorInfo selectedMonitorInfo;
        private UsbDeviceInfoModel selectedUsbDevice;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoSwitchConfigViewModel" /> class.
        /// </summary>
        /// <param name="autoSwitchConfig">The automatic switch configuration.</param>
        /// <param name="environment"></param>
        public AutoSwitchConfigViewModel(AutoSwitchConfig autoSwitchConfig, IKvmSwitchEnvironment environment)
        {
            this.environment = environment;
            this.Name = autoSwitchConfig.Name;
            this.Active = autoSwitchConfig.Active;
            this.ExecuteOnUsbDeviceConnected = autoSwitchConfig.ExecuteOnUsbDeviceConnected;
            this.ExecuteOnUsbDeviceDisconnected = autoSwitchConfig.ExecuteOnUsbDeviceDisconnected;

            this.InitUsbDevices(autoSwitchConfig);

            // Displays
            var supportedMonitorSources = environment.DisplayDataChannel.GetSupportedSourcesOfMonitors();

            this.MonitorInfos = new List<MonitorInfo>();

            for (var monitorIndex = 0; monitorIndex < supportedMonitorSources.Count; ++monitorIndex)
            {
                var monitorInfo = new MonitorInfo
                {
                    MonitorIndex = monitorIndex,
                    MonitorSources = supportedMonitorSources[monitorIndex]
                };

                var existingMonitorConfig =
                    autoSwitchConfig.MonitorConfigs?.FirstOrDefault(x => x.Name == monitorInfo.Name);


                MonitorSource monitorSource = null;

                if (existingMonitorConfig != null)
                {
                    monitorSource =
                        monitorInfo.MonitorSources.FirstOrDefault(x => x.Name == existingMonitorConfig.Input);
                    monitorInfo.UseInProfile = true;
                }


                if (monitorSource == null)
                {
                    monitorSource = monitorInfo.MonitorSources.FirstOrDefault();
                }

                monitorInfo.SelectedMonitorSource = monitorSource;
                this.MonitorInfos.Add(monitorInfo);
            }
        }

        public List<MonitorInfo> MonitorInfos { get; }

        public ObservableCollection<UsbDeviceInfoModel> UsbDevices { get; } = new ObservableCollection<UsbDeviceInfoModel>();

        public MonitorSource[] ActiveMonitorSources { get; private set; }

        public MonitorInfo SelectedMonitorInfo
        {
            get => this.selectedMonitorInfo;
            set
            {
                this.selectedMonitorInfo = value;
                this.ActiveMonitorSources = this.selectedMonitorInfo.MonitorSources;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ActiveMonitorSources));
            }
        }

        public UsbDeviceInfoModel SelectedUsbDevice
        {
            get => this.selectedUsbDevice;
            set
            {
                if (this.selectedUsbDevice == value)
                {
                    return;
                }

                if (this.selectedUsbDevice != null)
                {
                    this.selectedUsbDevice.State = UsbDeviceState.Existing;
                }

                this.selectedUsbDevice = value;

                if (this.selectedUsbDevice != null)
                {
                    this.selectedUsbDevice.State = UsbDeviceState.Active;
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.name = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="AutoSwitchConfigViewModel" /> is active.
        /// </summary>
        public bool Active
        {
            get => this.active;
            set
            {
                if (this.active == value)
                {
                    return;
                }

                this.active = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the file to execute on usb device connected.
        /// </summary>
        public string ExecuteOnUsbDeviceConnected { get; set; }

        /// <summary>
        ///     Gets or sets the file to execute on usb device disconnected.
        /// </summary>
        public string ExecuteOnUsbDeviceDisconnected { get; set; }

        private void InitUsbDevices(AutoSwitchConfig autoSwitchConfig)
        {
            // Usb Devices
            this.environment.UsbDeviceMonitor.UsbDeviceConnected += (sender, args) =>
            {
                var existingUsbDevice = this.UsbDevices.FirstOrDefault(x => x.DeviceId == args.UsbDevice.DeviceId);
                if (existingUsbDevice == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    this.UsbDevices.Add(
                        new UsbDeviceInfoModel(args.UsbDevice, UsbDeviceState.Added)));
                }
                else if (existingUsbDevice.DeviceId == autoSwitchConfig.UsbDeviceId)
                {
                    existingUsbDevice.Description = args.UsbDevice.Description;
                    existingUsbDevice.State = UsbDeviceState.Added;
                }
            };

            this.environment.UsbDeviceMonitor.UsbDeviceRemoved += (sender, args) =>
            {
                var usbDeviceToRemove = this.UsbDevices.FirstOrDefault(x => x.DeviceId == args.UsbDevice.DeviceId);

                if (usbDeviceToRemove != null && usbDeviceToRemove.DeviceId != autoSwitchConfig.UsbDeviceId)
                {
                    Application.Current.Dispatcher.Invoke(() => this.UsbDevices.Remove(usbDeviceToRemove));
                }
            };

            // add all Usb devices
            foreach (var usbDevice in this.environment.UsbDeviceMonitor.GetUSBDevices()
                .Where(usbDevice => this.UsbDevices.All(x => x.DeviceId != usbDevice.DeviceId)))
            {
                this.UsbDevices.Add(new UsbDeviceInfoModel(usbDevice, UsbDeviceState.Existing));
            }

            // add usb device from config if not in List
            if (!string.IsNullOrEmpty(autoSwitchConfig.UsbDeviceId) &&
                this.UsbDevices.All(x => x.DeviceId != autoSwitchConfig.UsbDeviceId))
            {
                this.UsbDevices.Add(
                    new UsbDeviceInfoModel(
                        new UsbDeviceInfo(autoSwitchConfig.UsbDeviceId),
                        UsbDeviceState.Active));
            }

            this.SelectedUsbDevice = this.UsbDevices.FirstOrDefault(x => x.DeviceId == autoSwitchConfig.UsbDeviceId);
        }

        public AutoSwitchConfig CreateConfig()
        {
            var monitorConfigs = this.MonitorInfos
                .Where(x => x.UseInProfile)
                .Select(monitorInfo =>
                    new MonitorConfig
                    {
                        MonitorIndex = monitorInfo.MonitorIndex,
                        Input = monitorInfo.SelectedMonitorSource?.Name
                    })
                .ToList();

            return new AutoSwitchConfig
            {
                Name = this.Name,
                Active = this.Active,
                UsbDeviceId = this.SelectedUsbDevice?.DeviceId,
                MonitorConfigs = monitorConfigs,
                ExecuteOnUsbDeviceConnected = this.ExecuteOnUsbDeviceConnected,
                ExecuteOnUsbDeviceDisconnected = this.ExecuteOnUsbDeviceDisconnected
            };
        }
    }
}
