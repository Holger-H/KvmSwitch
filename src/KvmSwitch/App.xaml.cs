namespace KvmSwitch
{
    using System;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using Data;
    using log4net;
    using UI;
    using Application = System.Windows.Application;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILog log = LogManager.GetLogger(typeof(App));

        private NotifyIcon notifyIcon;
        private bool updateConfig;
        private IKvmSwitchEnvironment environment;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;

            this.environment = new KvmSwitchEnvironment();
            this.environment.Initialize();

            this.notifyIcon = new NotifyIcon
            {
                Icon = KvmSwitch.Properties.Resources.TraySymbol,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            this.InitializeNotifyIconContextMenu();
            this.environment.ConfigChanged += (sender, args) =>
            {
                this.UpdateTextOfContextMenuItems();
            };

            this.environment.ConfigChanged += (sender, args) => this.UpdateProfileMenuItems();
        }

        private void UpdateTextOfContextMenuItems()
        {
            this.profileMenuItem.Text = KvmSwitch.Properties.Resources.Profiles;
            this.configMenuItem.Text = KvmSwitch.Properties.Resources.Options;
            this.exitMenuItem.Text = KvmSwitch.Properties.Resources.Exit;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            this.environment.Dispose();
            this.notifyIcon?.Dispose();
        }

        private ToolStripMenuItem profileMenuItem;
        private ToolStripItem configMenuItem;
        private ToolStripItem exitMenuItem;

        private void InitializeNotifyIconContextMenu()
        {
            this.configMenuItem = this.notifyIcon.ContextMenuStrip.Items.Add(KvmSwitch.Properties.Resources.Options,
                KvmSwitch.Properties.Resources.Settings_24x24,
                this.ConfigMenuItem_Click);

            this.profileMenuItem = new ToolStripMenuItem(KvmSwitch.Properties.Resources.Profiles,
                KvmSwitch.Properties.Resources.Bullets_24x24);
            this.profileMenuItem.DropDownClosed += (sender, args) =>
            {
                if (!this.updateConfig)
                {
                    return;
                }

                this.updateConfig = false;

                this.environment.SaveConfiguration(this.environment.Config);
            };
            this.notifyIcon.ContextMenuStrip.Items.Add(this.profileMenuItem);


            this.UpdateProfileMenuItems();

            this.exitMenuItem = this.notifyIcon.ContextMenuStrip.Items.Add(KvmSwitch.Properties.Resources.Exit,
                KvmSwitch.Properties.Resources.Logout_24x24,
                (sender, args) => this.Shutdown());
        }

        private void UpdateProfileMenuItems()
        {
            foreach (ToolStripMenuItem toolStripItem in this.profileMenuItem.DropDownItems)
            {
                toolStripItem.CheckedChanged -= this.AutoSwitchConfigMenuItem_CheckedChanged;
            }

            this.profileMenuItem.DropDownItems.Clear();


            if (this.environment?.Config?.AutoSwitchConfigs == null)
            {
                return;
            }

            foreach (var autoSwitchConfig in this.environment.Config.AutoSwitchConfigs)
            {
                var autoSwitchConfigMenuItem = new ToolStripMenuItem(autoSwitchConfig.Name)
                {
                    CheckOnClick = true,
                    Checked = autoSwitchConfig.Active,
                    Tag = autoSwitchConfig
                };

                autoSwitchConfigMenuItem.CheckedChanged += this.AutoSwitchConfigMenuItem_CheckedChanged;

                this.profileMenuItem.DropDownItems.Add(autoSwitchConfigMenuItem);
            }
        }

        private void AutoSwitchConfigMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.updateConfig = true;

            var changedToolStripMenuItem = sender as ToolStripMenuItem;

            if (changedToolStripMenuItem?.Tag is not AutoSwitchConfig autoSwitchConfig)
            {
                return;
            }

            autoSwitchConfig.Active = changedToolStripMenuItem.Checked;

            if (!changedToolStripMenuItem.Checked)
            {
                return;
            }

            // Disable other Configs

            foreach (ToolStripMenuItem toolStripItem in this.profileMenuItem.DropDownItems)
            {
                if (toolStripItem != changedToolStripMenuItem && toolStripItem.Checked)
                {
                    toolStripItem.Checked = false;
                }
            }
        }

        private void ConfigMenuItem_Click(object sender, EventArgs e)
        {
            var configWindow = new ConfigWindow
            {
                DataContext = new ConfigWindowModel(this.environment)
            };

            configWindow.ShowDialog();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.log.Error("Unhandled Exception: " + e.Exception);
        }
    }
}
