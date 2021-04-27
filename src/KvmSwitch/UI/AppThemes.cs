namespace KvmSwitch.UI
{
    using System.Collections.Generic;
    using ModernWpf;

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
