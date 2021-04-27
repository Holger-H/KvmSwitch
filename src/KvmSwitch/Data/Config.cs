namespace KvmSwitch.Data
{
    using System.Collections.Generic;

    public class Config
    {
        /// <summary>
        ///     Gets or sets the automatic switch configuration hotkey.
        /// </summary>
        public Hotkey AutoSwitchConfigHotkey { get; set; }

        public bool AutoStart { get; set; }

        public string Culture { get; set; }

        public ModernWpf.ApplicationTheme? ApplicationTheme { get; set; }

        /// <summary>
        ///     Gets or sets the automatic switch configs.
        /// </summary>
        public List<AutoSwitchConfig> AutoSwitchConfigs { get; } = new List<AutoSwitchConfig>();
    }
}
