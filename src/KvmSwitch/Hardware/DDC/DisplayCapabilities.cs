namespace KvmSwitch.Hardware.DDC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DisplayCapabilities
    {
        public List<CapabilityTag> CapabilityTags { get; }

        /// <summary>
        ///     Gets the protocol. Identifies the generic protocol or device.
        /// </summary>
        public string Protocol { get; }

        /// <summary>
        ///     Gets the device type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     Gets the full model name
        /// </summary>
        public string Model { get; }


        public Version MccsVersion { get; }


        public CapabilityTag VirtualControlPanel { get; }


        public DisplayCapabilities(List<CapabilityTag> capabilityTags)
        {
            this.CapabilityTags = capabilityTags;

            this.Protocol = this.GetValue("prot");
            this.Type = this.GetValue("type");
            this.Model = this.GetValue("model");

            var versionStr = this.GetValue("mccs_ver");

            if (!string.IsNullOrEmpty(versionStr) && Version.TryParse(versionStr, out var parsedMccsVersion))
            {
                this.MccsVersion = parsedMccsVersion;
            }

            this.VirtualControlPanel = this.GetTag("vcp");
        }

        private string GetValue(string name)
        {
            return this.GetTag(name)?.Value;
        }

        private CapabilityTag GetTag(string name)
        {
            return this.CapabilityTags
                ?.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}