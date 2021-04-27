namespace KvmSwitch.UI
{
    using System.Windows;
    using System.Windows.Input;
    using KvmSwitch.Data;

    /// <summary>
    ///     ViewModel for the <see cref="ConfigWindow" />.
    /// </summary>
    /// <seealso cref="KvmSwitch.UI.ViewModelBase" />
    public class ConfigWindowModel : ViewModelBase
    {
        private readonly IKvmSwitchEnvironment environment;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigWindowModel" /> class.
        /// </summary>
        public ConfigWindowModel(IKvmSwitchEnvironment environment)
        {
            this.environment = environment;
            this.GeneralConfigViewModel = new GeneralConfigViewModel(environment.Config);
            this.AutoSwitchConfigsViewModel = new AutoSwitchConfigsViewModel(environment.Config.AutoSwitchConfigs, environment);

            this.SaveCommand = new RelayCommand(window => this.SaveConfiguration((Window)window));
            this.CancelCommand = new RelayCommand(window => this.Cancel((Window)window));          
        }

        public GeneralConfigViewModel GeneralConfigViewModel { get; }

        public AutoSwitchConfigsViewModel AutoSwitchConfigsViewModel { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }


        private void SaveConfiguration(Window window)
        {
            var autoSwitchConfigs = this.AutoSwitchConfigsViewModel.GetAutoSwitchConfigs();

            var config = new Config();
            config.AutoSwitchConfigs.Clear();
            config.AutoSwitchConfigs.AddRange(autoSwitchConfigs);

            this.GeneralConfigViewModel.ApplyChanges(config);

            this.environment.SaveConfiguration(config);

            window.DialogResult = true;
            window.Close();
        }

        private void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
