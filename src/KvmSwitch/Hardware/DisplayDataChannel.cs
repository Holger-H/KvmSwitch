namespace KvmSwitch.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using KvmSwitch.Data;
    using KvmSwitch.Hardware.DDC;
    using KvmSwitch.Native;
    using log4net;

    internal class DisplayDataChannel : IDisplayDataChannel
    {
        private readonly ILog log = LogManager.GetLogger(typeof(DisplayDataChannel));

        private readonly Dictionary<IntPtr, MonitorSource[]> monitorSourcesByPhysicalMonitor = new();
        private bool initialized;

        public void Initialize()
        {
            if (this.initialized)
            {
                return;
            }

            this.initialized = true;
            this.physicalMonitors = new List<PhysicalMonitor[]>();

            var enumMonitorsDelegate = new User32.EnumMonitorsDelegate(this.EnumMonitorsInit);
            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, enumMonitorsDelegate, IntPtr.Zero);
        }

        private void Cleanup()
        {
            if (!this.initialized)
            {
                return;
            }

            this.initialized = false;
            this.monitorSourcesByPhysicalMonitor.Clear();

            foreach (var monitors in this.physicalMonitors)
            {
                var sizeofPhysicalMonitor = Marshal.SizeOf(typeof(PhysicalMonitor));
                var pUnmanagedMonitorArray = Marshal.AllocHGlobal(sizeofPhysicalMonitor * monitors.Length);

                var pUnmanagedMonitorElement = new IntPtr(pUnmanagedMonitorArray.ToInt64());
                foreach (var monitor in monitors)
                {
                    Marshal.StructureToPtr(monitor, pUnmanagedMonitorElement, false);
                    pUnmanagedMonitorElement += sizeofPhysicalMonitor;
                }

                Dxva2.DestroyPhysicalMonitors((uint)monitors.Length, pUnmanagedMonitorArray);
                Marshal.FreeHGlobal(pUnmanagedMonitorArray);
            }
        }

        public List<MonitorSource[]> GetSupportedSourcesOfMonitors()
        {
            var sourcesList = new List<MonitorSource[]>();

            foreach (var monitorArray in this.physicalMonitors)
            {
                sourcesList.AddRange(
                    monitorArray.Select(monitor =>
                        this.GetSupportedSourcesOfMonitor(monitor.hPhysicalMonitor)));
            }

            return sourcesList;
        }

        public void EnsureDisplaySourceIsActive(IEnumerable<MonitorConfig> monitorConfigs)
        {
            this.log.Debug($"{nameof(EnsureDisplaySourceIsActive)}");

            var monitorSupportedSources = this.GetSupportedSourcesOfMonitors();

            foreach (var monitorConfig in monitorConfigs)
            {
                this.EnsureDisplaySourceIsActive(monitorConfig.MonitorIndex, monitorConfig.Input, monitorSupportedSources);
            }
        }

        public void EnsureDisplaySourceIsActive(int monitorIndex, string sourceName,
            List<MonitorSource[]> monitorSupportedSources)
        {
            this.log.Debug($"{nameof(EnsureDisplaySourceIsActive)} Monitor: {monitorIndex}; Input:  {sourceName}");

            if (monitorIndex < 0 || monitorIndex >= monitorSupportedSources.Count)
            {
                this.log.Info(monitorIndex + " is not valid");
                return;
            }

            var monitorSource = monitorSupportedSources[monitorIndex].FirstOrDefault(x => x.Name == sourceName);

            if (monitorSource == null)
            {
                this.log.Info($"MonitorSource '{sourceName}' for monitor '{monitorIndex}' does not exists.");
                return;
            }

            var currentMonitorIndex = -1;
            foreach (var monitorArray in this.physicalMonitors)
            {
                foreach (var monitor in monitorArray)
                {
                    currentMonitorIndex++;

                    if (currentMonitorIndex != monitorIndex)
                    {
                        continue;
                    }

                    var activeMonitorInputSource = this.GetActiveMonitorInputSource(monitor);

                    if (activeMonitorInputSource != monitorSource.Code)
                    {
                        this.log.Debug(
                        $"{nameof(EnsureDisplaySourceIsActive)}-{nameof(Dxva2.SetVCPFeature)} current input of monitor {monitorIndex} is '{activeMonitorInputSource}' and will be switched to '{monitorSource.Code} ({monitorSource.Name})'");
                        Dxva2.SetVCPFeature(monitor.hPhysicalMonitor, (byte)VCPCodes.InputSource, monitorSource.Code);
                    }

                    break;
                }
            }
        }

        private int GetActiveMonitorInputSource(PhysicalMonitor monitor)
        {
            this.log.Debug($"{nameof(GetActiveMonitorInputSource)}");
            var nullVal = IntPtr.Zero;
            Dxva2.GetVCPFeatureAndVCPFeatureReply(monitor.hPhysicalMonitor, (byte)VCPCodes.InputSource, ref nullVal,
                out var currentValue, out _);

            // Get the Input source from the first 1 byte
            return currentValue & 0xff;
        }


        private const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;

        private enum VCPCodes : byte
        {
            InputSource = 0x60
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PhysicalMonitor
        {
            public readonly IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PHYSICAL_MONITOR_DESCRIPTION_SIZE)]
            public readonly string szPhysicalMonitorDescription;
        }


        // Sources in MCCS v2.0 == v2.1, and both are a subset of 2.2, so we use a single array to cover them all.
        // Note that the standards use one-based indexing, so we just add a dummy element at the start.
        private static readonly string[] sourceNamesMccsV2 =
        {
            "**undefined**",
            "VGA 1",
            "VGA 2",
            "DVI 1",
            "DVI 2",
            "Composite 1",
            "Composite 2",
            "S-video 1",
            "S-video 2",
            "Tuner 1",
            "Tuner 2",
            "Tuner 3",
            "Component 1",
            "Component 2",
            "Component 3",
            "DisplayPort 1",
            "DisplayPort 2",
            "HDMI 1",
            "HDMI 2"
        };

        // Note that MCCS v3.0 was not well adopted, so 2.2a has become the active standard.
        // Note that the standards use one-based indexing, so we just add a dummy element at the start.
        private static readonly string[] sourceNamesMccsV3 =
        {
            "**undefined**",
            "VGA 1",
            "VGA 2",
            "DVI 1",
            "DVI 2",
            "Composite 1",
            "Composite 2",
            "S-video 1",
            "S-video 2",
            "Tuner - Analog 1",
            "Tuner - Analog 2",
            "Tuner - Digital 1",
            "Tuner - Digital 2",
            "Component 1",
            "Component 2",
            "Component 3",
            "**Unrecognized**",
            "DisplayPort 1",
            "DisplayPort 2"
        };

        private List<PhysicalMonitor[]> physicalMonitors;
        private bool disposedValue;

        private bool EnumMonitorsInit(IntPtr hMonitor, IntPtr hdcMonitor, ref User32.Rect lprcMonitor,
            IntPtr dwData)
        {
            Dxva2.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out _);

            var success = GetPhysicalMonitorsFromHMONITOR(hMonitor, out var physicalMonitors);
            if (success)
            {
                this.physicalMonitors.Add(physicalMonitors);
            }

            return true; // Return true to continue enumeration.
        }

        private static bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out PhysicalMonitor[] physicalMonitors)
        {
            // Allocate unmanaged memory.
            Dxva2.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var physicalMonitorCount);

            var sizeofPhysicalMonitor = Marshal.SizeOf(typeof(PhysicalMonitor));
            var pUnmanagedMonitorArray = Marshal.AllocHGlobal(sizeofPhysicalMonitor * (int)physicalMonitorCount);

            // Fetch data.
            var fetchSuccess =
                Dxva2.GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorCount, pUnmanagedMonitorArray);

            // Copy data.
            physicalMonitors = new PhysicalMonitor[physicalMonitorCount];
            if (fetchSuccess)
            {
                var pUnmanagedMonitorElement = new IntPtr(pUnmanagedMonitorArray.ToInt64());
                for (var i = 0; i < physicalMonitors.Length; ++i)
                {
                    physicalMonitors[i] =
                        (PhysicalMonitor)Marshal.PtrToStructure(pUnmanagedMonitorElement, typeof(PhysicalMonitor));
                    pUnmanagedMonitorElement += sizeofPhysicalMonitor;
                }
            }

            // Free unmanaged memory.
            Marshal.FreeHGlobal(pUnmanagedMonitorArray);

            return fetchSuccess;
        }

        private MonitorSource[] GetSupportedSourcesOfMonitor(IntPtr hMonitor)
        {
            // cached?

            if (this.monitorSourcesByPhysicalMonitor.ContainsKey(hMonitor))
            {
                return this.monitorSourcesByPhysicalMonitor[hMonitor];
            }

            var values = new int[0];

            Dxva2.GetCapabilitiesStringLength(hMonitor, out var strSize);

            var capabilities = new StringBuilder((int)strSize);
            Dxva2.CapabilitiesRequestAndCapabilitiesReply(hMonitor, capabilities, strSize);

            var capabilitiesStr = capabilities.ToString();
            this.log.Debug($"Monitor capabilities: {capabilitiesStr}");

            var displayCapabilities = CapabilitiesParser.ParseToDisplayCapabilities(capabilitiesStr);

            var inputSourceTag = displayCapabilities.VirtualControlPanel?.Tags.FirstOrDefault(x => x.Name == "60");

            if (inputSourceTag != null)
            {
                values = inputSourceTag.Tags.Select(x => x.ValueAsInt).ToArray();
            }

            // MCCS version.
            string[] sourceNames;
            if (displayCapabilities.MccsVersion != null)
            {
                sourceNames = displayCapabilities.MccsVersion.Major < 3
                    ? sourceNamesMccsV2
                    : sourceNamesMccsV3;
            }
            else
            {
                sourceNames = new string[0];
            }

            var sources = new List<MonitorSource>();
            for (var i = 0; i < values.Length; ++i)
            {
                if (0 <= values[i] && values[i] < sourceNames.Length)
                {
                    sources.Add(
                        new MonitorSource
                        {
                            Code = values[i],
                            Name = sourceNames[values[i]]
                        });
                }
            }

            this.monitorSourcesByPhysicalMonitor.Add(hMonitor, sources.ToArray());

            return sources.ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                this.Cleanup();

                this.disposedValue = true;
            }
        }


        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
