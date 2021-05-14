namespace KvmSwitch.UI
{
    using Data;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class GeneralConfigViewModel : ViewModelBase
    {
        /// <summary>
        ///     Gets or sets the automatic switch configuration hotkey.
        /// </summary>
        public Hotkey AutoSwitchConfigHotkey { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether automatic start with windows is enabled.
        /// </summary>
        public bool AutoStart { get; set; }

        public CultureInfo SelectedCulture { get; set; }

        public List<CultureInfo> Cultures { get; }

        public AppThemes AppThemes { get; } = new();

        public AppTheme SelectedAppTheme { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeneralConfigViewModel" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public GeneralConfigViewModel(Config config)
        {
            this.AutoSwitchConfigHotkey = config.AutoSwitchConfigHotkey;
            this.AutoStart = config.AutoStart;

            this.Cultures = new List<CultureInfo> { new CultureInfo("de"), new CultureInfo("en") };

            if (!string.IsNullOrEmpty(config.Culture))
            {
                this.SelectedCulture = this.Cultures.FirstOrDefault(x => x.Name == config.Culture)
                                       ?? this.Cultures.FirstOrDefault(x =>
                                           x.IetfLanguageTag ==
                                           config.Culture.Split("-".ToCharArray()).FirstOrDefault());
            }

            // App Theme
            this.SelectedAppTheme = this.AppThemes.FirstOrDefault(x => x.Value == config.ApplicationTheme);
        }

        /// <summary>
        ///     Applies the changes.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void ApplyChanges(Config config)
        {
            config.AutoSwitchConfigHotkey = this.AutoSwitchConfigHotkey;
            config.AutoStart = this.AutoStart;
            config.Culture = this.SelectedCulture?.Name;
            config.ApplicationTheme = this.SelectedAppTheme?.Value;
        }
    }
}
