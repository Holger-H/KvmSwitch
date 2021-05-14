namespace KvmSwitch.UI
{
    using ModernWpf;
    using System.Collections.Generic;

    public class AppThemes : List<AppTheme>
    {
        public AppThemes()
        {
            this.Add(new AppTheme(Properties.Resources.Theme_Light, ApplicationTheme.Light));
            this.Add(new AppTheme(Properties.Resources.Theme_Dark, ApplicationTheme.Dark));
            this.Add(new AppTheme(Properties.Resources.Theme_SystemDefault, null));
        }
    }
}
