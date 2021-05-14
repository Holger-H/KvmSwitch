namespace KvmSwitch
{
    using KvmSwitch.Data;
    using KvmSwitch.Hardware;
    using log4net;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    internal class KvmSwitchEnvironment : IKvmSwitchEnvironment
    {
        private readonly ILog log = LogManager.GetLogger(typeof(KvmSwitchEnvironment));
        private int? autoSwitchConfigHotkeyId;
        private bool disposedValue;

        internal KvmSwitchEnvironment()
        {
            this.DisplayDataChannel = new DisplayDataChannel();
            this.UsbDeviceMonitor = new UsbDeviceMonitor();
            this.ConfigProvider = new ConfigProvider();
            this.MonitorSwitcher = new AutomaticMonitorSwitcher(this);
        }

        public IUsbDeviceMonitor UsbDeviceMonitor { get; }

        public Config Config { get; private set; }
        public IDisplayDataChannel DisplayDataChannel { get; }

        private AutomaticMonitorSwitcher MonitorSwitcher { get; }

        private ConfigProvider ConfigProvider { get; }

        private AutoStartManager AutoStartManager { get; } = new AutoStartManager();

        public event EventHandler ConfigChanged;

        public HotKeyManager HotKeyManager { get; } = new();

        public void SaveConfiguration(Config config)
        {
            if (this.autoSwitchConfigHotkeyId.HasValue)
            {
                this.HotKeyManager.UnregisterHotKey(this.autoSwitchConfigHotkeyId.Value);
            }

            this.ConfigProvider.Save(config);
            this.Config = config;
            this.OnConfigChanged();
        }

        public void Initialize()
        {
            this.log.Info(nameof(Initialize));

            this.DisplayDataChannel.Initialize();

            this.Config = this.ConfigProvider.Load();
            if (string.IsNullOrEmpty(this.Config.Culture))
            {
                this.Config.Culture = Thread.CurrentThread.CurrentUICulture.Name;
            }

            this.OnConfigChanged();

            this.UsbDeviceMonitor.Start();
            this.MonitorSwitcher.Initialize();

            this.HotKeyManager.HotKeyPressed += (sender, args) =>
            {
                var autoSwitchConfig = this.Config?.AutoSwitchConfigs?.FirstOrDefault(x => x.Active);
                if (autoSwitchConfig?.MonitorConfigs != null && this.Config?.AutoSwitchConfigHotkey != null)
                {
                    this.DisplayDataChannel.EnsureDisplaySourceIsActive(autoSwitchConfig.MonitorConfigs);
                }
            };

            // This improves performance on first load of Config Dialog or first switch in active profile
            // Existing sources are cached and this will avoid a later delay of 3 seconds
            _ = this.DisplayDataChannel.GetSupportedSourcesOfMonitors();

            this.log.Info($"{nameof(this.Initialize)} completed");
        }

        private void OnConfigChanged()
        {
            if (this.Config == null)
            {
                return;
            }

            this.AutoStartManager.EnableAutoStart(this.Config.AutoStart);

            if (this.Config.AutoSwitchConfigHotkey != null)
            {
                this.autoSwitchConfigHotkeyId = this.HotKeyManager.RegisterHotKey(this.Config.AutoSwitchConfigHotkey.Key,
                    this.Config.AutoSwitchConfigHotkey.Modifiers);
            }

            if (this.Config.Culture != null && Thread.CurrentThread.CurrentUICulture.Name != this.Config.Culture)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.Config.Culture);
            }

            ModernWpf.ThemeManager.Current.ApplicationTheme = this.Config.ApplicationTheme;

            this.ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.HotKeyManager?.Dispose();
                    this.DisplayDataChannel?.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
