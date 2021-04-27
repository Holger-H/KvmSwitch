namespace KvmSwitch.Tests
{
    using System.Collections.Generic;
    using FluentAssertions;
    using KvmSwitch.Data;
    using KvmSwitch.Hardware;
    using Moq;
    using Xunit;

    public class AutomaticMonitorSwitcherTest
    {
        [Fact]
        public void Switches_On_UsbDevice_Connected()
        {
            var ensureDisplaySourceIsActiveCalled = false;

            var ddcMock = new Mock<IDisplayDataChannel>();
            ddcMock.Setup(x => x.EnsureDisplaySourceIsActive(It.IsAny<IEnumerable<MonitorConfig>>())).Callback(() =>
            {
                ensureDisplaySourceIsActiveCalled = true;
            });


            var config = new Config
            {
                AutoSwitchConfigs =
                {
                    new AutoSwitchConfig
                    {
                        Active = true,
                        UsbDeviceId = "UsbDevice 1",
                        MonitorConfigs = new List<MonitorConfig> {new MonitorConfig()}
                    }
                }
            };

            var usbDeviceMonitorMock = new Mock<IUsbDeviceMonitor>();
            var environmentMock = new Mock<IKvmSwitchEnvironment>();
            environmentMock.Setup(x => x.UsbDeviceMonitor).Returns(usbDeviceMonitorMock.Object);
            environmentMock.Setup(x => x.DisplayDataChannel).Returns(ddcMock.Object);
            environmentMock.Setup(x => x.Config).Returns(config);


            var automaticMonitorSwitcher = new AutomaticMonitorSwitcher(environmentMock.Object);
            automaticMonitorSwitcher.Initialize();

            // raise event and this should trigger monitor switching
            usbDeviceMonitorMock.Raise(x => x.UsbDeviceConnected += null,
                new UsbDeviceEventArgs(new UsbDeviceInfo("UsbDevice 1")));

            ensureDisplaySourceIsActiveCalled.Should().BeTrue();
        }
    }
}
