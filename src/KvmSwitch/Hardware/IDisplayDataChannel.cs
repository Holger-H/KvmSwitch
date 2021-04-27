namespace KvmSwitch.Hardware
{
    using System;
    using System.Collections.Generic;
    using KvmSwitch.Data;

    public interface IDisplayDataChannel : IDisposable
    {
        /// <summary>
        ///     Initializes the display data channel.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Returns a list of MonitorSources, one per monitor.
        /// </summary>
        List<MonitorSource[]> GetSupportedSourcesOfMonitors();

        void EnsureDisplaySourceIsActive(IEnumerable<MonitorConfig> monitorConfigs);

        void EnsureDisplaySourceIsActive(int monitorIndex, string sourceName,
            List<MonitorSource[]> monitorSupportedSources);
    }
}
