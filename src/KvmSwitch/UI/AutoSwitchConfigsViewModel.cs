namespace KvmSwitch.UI
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using KvmSwitch.Data;

    public class AutoSwitchConfigsViewModel : ViewModelBase
    {
        private AutoSwitchConfigViewModel selectedAutoSwitchConfigViewModel;
        private readonly IKvmSwitchEnvironment environment;

        public AutoSwitchConfigsViewModel(List<AutoSwitchConfig> autoSwitchConfigs, IKvmSwitchEnvironment environment)
        {
            this.environment = environment;

            this.AddConfigCommand = new RelayCommand(args =>
            {
                var newConfig = this.CreateAutoSwitchConfigViewModel(new AutoSwitchConfig { Name = "New" });
                this.AutoSwitchConfigs.Add(newConfig);

                this.SelectedAutoSwitchConfigViewModel = newConfig;
            });

            this.DeleteConfigCommand = new RelayCommand(
                args => { this.AutoSwitchConfigs.Remove(this.SelectedAutoSwitchConfigViewModel); },
                x => this.SelectedAutoSwitchConfigViewModel != null);

            this.AutoSwitchConfigs = new ObservableCollection<AutoSwitchConfigViewModel>();
            foreach (var config in autoSwitchConfigs)
            {
                this.AutoSwitchConfigs.Add(this.CreateAutoSwitchConfigViewModel(config));
            }

            this.SelectedAutoSwitchConfigViewModel = this.AutoSwitchConfigs.FirstOrDefault();
        }

        public ICommand AddConfigCommand { get; }

        public ICommand DeleteConfigCommand { get; }

        public bool AutoSwitchConfigViewEnabled => this.SelectedAutoSwitchConfigViewModel != null;

        /// <summary>
        ///     Gets the automatic switch configuration view model.
        /// </summary>
        public AutoSwitchConfigViewModel SelectedAutoSwitchConfigViewModel
        {
            get => this.selectedAutoSwitchConfigViewModel;
            set
            {
                if (this.selectedAutoSwitchConfigViewModel == value)
                {
                    return;
                }

                this.selectedAutoSwitchConfigViewModel = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.AutoSwitchConfigViewEnabled));
            }
        }

        /// <summary>
        ///     Gets the automatic switch configurations.
        /// </summary>
        public ObservableCollection<AutoSwitchConfigViewModel> AutoSwitchConfigs { get; }

        public IList<AutoSwitchConfig> GetAutoSwitchConfigs()
        {
            return this.AutoSwitchConfigs
                .Select(autoSwitchConfigViewModel => autoSwitchConfigViewModel.CreateConfig())
                .ToList();
        }

        private AutoSwitchConfigViewModel CreateAutoSwitchConfigViewModel(AutoSwitchConfig autoSwitchConfig)
        {
            var autoSwitchConfigViewModel = new AutoSwitchConfigViewModel(autoSwitchConfig, this.environment);
            autoSwitchConfigViewModel.PropertyChanged += this.AutoSwitchConfigViewModel_PropertyChanged;

            return autoSwitchConfigViewModel;
        }

        private void AutoSwitchConfigViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AutoSwitchConfigViewModel.Active))
            {
                return;
            }

            var autoSwitchConfigViewModel = (AutoSwitchConfigViewModel)sender;

            if (!autoSwitchConfigViewModel.Active)
            {
                return;
            }

            foreach (var switchConfigViewModel in this.AutoSwitchConfigs.Where(x => x != autoSwitchConfigViewModel))
            {
                switchConfigViewModel.Active = false;
            }
        }
    }
}
