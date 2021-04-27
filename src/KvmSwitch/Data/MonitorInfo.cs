namespace KvmSwitch.Data
{
    using KvmSwitch.Hardware;

    public class MonitorInfo
    {
        public int MonitorIndex { get; set; }

        public string Name => $"Monitor {this.MonitorIndex + 1}";

        public MonitorSource[] MonitorSources { get; set; }

        public bool UseInProfile { get; set; }

        public MonitorSource SelectedMonitorSource { get; set; }
    }
}