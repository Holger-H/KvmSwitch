namespace KvmSwitch.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class Dxva2
    {
        [DllImport("Dxva2.dll")]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor,
            out uint pdwNumberOfPhysicalMonitors);

        [DllImport("Dxva2.dll")]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize,
            IntPtr pPhysicalMonitorArray);

        [DllImport("Dxva2.dll")]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize,
            IntPtr pPhysicalMonitorArray);

        /// <summary>
        ///     Used for features with continuous values (values that can be anything in [0, maxValue]).
        /// </summary>
        [DllImport("Dxva2.dll")]
        public static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte bVCPCode, ref IntPtr makeNull,
            out int currentValue, out int maxValue);

        /// <summary>
        ///     Retrieves the length of a monitor's capabilities string, including the terminating null character.
        /// </summary>
        [DllImport("Dxva2.dll")]
        public static extern bool GetCapabilitiesStringLength(IntPtr hMonitor, out uint numCharacters);

        /// <summary>
        ///     Retrieves a string describing a monitor's capabilities.
        /// </summary>
        /// <param name="hMonitor">Handle to a physical monitor.</param>
        /// <param name="capabilities">The buffer must include space for the terminating null character. The result is in ASCII.</param>
        /// <param name="capabilitiesLength">Includes the terminating null character.</param>
        [DllImport("Dxva2.dll")]
        public static extern bool CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, StringBuilder capabilities,
            uint capabilitiesLength);

        [DllImport("Dxva2.dll")]
        public static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, int dwNewValue);
    }
}
