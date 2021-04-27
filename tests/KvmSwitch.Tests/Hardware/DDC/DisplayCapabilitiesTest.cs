namespace KvmSwitch.Tests.Hardware.DDC
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using KvmSwitch.Hardware.DDC;
    using Xunit;

    public class DisplayCapabilitiesTest
    {
        [Fact]
        public void Ctor_Does_Not_Throw_If_Capabilities_Are_Null()
        {
            var action = new Action(() => new DisplayCapabilities(null));
            action.Should().NotThrow();
        }

        [Fact]
        public void Ctor_Does_Not_Throw_If_Capabilities_Are_Empty()
        {
            var action = new Action(() => new DisplayCapabilities(new List<CapabilityTag>()));
            action.Should().NotThrow();
        }

        [Fact]
        public void Assigns_Model_In_Ctor()
        {
            var displayCapabilities = new DisplayCapabilities(new List<CapabilityTag>
                {new CapabilityTag {Name = "model", Tags = {new CapabilityTag {Name = "test"}}}});

            displayCapabilities.Model.Should().Be("test");
        }

        [Fact]
        public void Assigns_Protocol_In_Ctor()
        {
            var displayCapabilities = new DisplayCapabilities(new List<CapabilityTag>
                {new CapabilityTag {Name = "prot", Tags = {new CapabilityTag {Name = "test"}}}});

            displayCapabilities.Protocol.Should().Be("test");
        }

        [Fact]
        public void Assigns_Type_In_Ctor()
        {
            var displayCapabilities = new DisplayCapabilities(new List<CapabilityTag>
                {new CapabilityTag {Name = "type", Tags = {new CapabilityTag {Name = "test"}}}});

            displayCapabilities.Type.Should().Be("test");
        }

        [Fact]
        public void Assigns_MccsVersion_In_Ctor()
        {
            var displayCapabilities = new DisplayCapabilities(new List<CapabilityTag>
                {new CapabilityTag {Name = "mccs_ver", Tags = {new CapabilityTag {Name = "1.2"}}}});

            displayCapabilities.MccsVersion.Major.Should().Be(1);
            displayCapabilities.MccsVersion.Minor.Should().Be(2);
        }

        [Fact]
        public void Ctor_Does_Not_Throw_On_Invalid_MccsVersion()
        {
            DisplayCapabilities displayCapabilities = null;
            var action = new Action(() => displayCapabilities = new DisplayCapabilities(new List<CapabilityTag>
                {new CapabilityTag {Name = "mccs_ver", Tags = {new CapabilityTag {Name = "alpha beta"}}}}));

            action.Should().NotThrow();

            displayCapabilities.MccsVersion.Should().Be(null);
        }

        [Fact]
        public void Assigns_VirtualControlPanel_In_Ctor()
        {
            var vcpTag = new CapabilityTag { Name = "vcp" };
            var displayCapabilities = new DisplayCapabilities(new List<CapabilityTag> { vcpTag });

            displayCapabilities.VirtualControlPanel.Should().Be(vcpTag);
        }
    }
}
