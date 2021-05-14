
namespace KvmSwitch.Data
{
    public class MonitorConfig
    {
        /// <summary>
        ///     Gets the name of the monitor.
        /// </summary>
        public string Name => $"Monitor {this.MonitorIndex + 1}";

        /// <summary>
        ///     Gets or sets the index of the monitor.
        /// </summary>
        public int MonitorIndex { get; set; }

        /// <summary>
        ///     Gets or sets the name of the monitor input.
        /// </summary>
        public string Input { get; set; }
    }
}