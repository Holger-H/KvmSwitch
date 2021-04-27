namespace KvmSwitch.Hardware.DDC
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal class CapabilityTag
    {
        public string Name { get; set; }

        public List<CapabilityTag> Tags { get; } = new List<CapabilityTag>();

        /// <summary>
        ///     Gets the value (<see cref="Name" /> of the first child (<see cref="Tags" />)).
        /// </summary>
        public string Value => this.Tags.FirstOrDefault()?.Name;

        public int ValueAsInt => int.TryParse(this.Name, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out var num)
            ? num
            : -1;


        public override string ToString()
        {
            return this.Name;
        }
    }
}