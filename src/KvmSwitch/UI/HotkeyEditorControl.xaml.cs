namespace KvmSwitch.UI
{
    using Data;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for HotkeyEditorControl.xaml
    /// </summary>
    public partial class HotkeyEditorControl : UserControl
    {
        public static readonly DependencyProperty HotkeyProperty =
            DependencyProperty.Register(nameof(Hotkey), typeof(Hotkey),
                typeof(HotkeyEditorControl),
                new FrameworkPropertyMetadata(default(Hotkey),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Hotkey Hotkey
        {
            get => (Hotkey)this.GetValue(HotkeyProperty);
            set => this.SetValue(HotkeyProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(HotkeyEditorControl), new PropertyMetadata(null));

        public string Header
        {
            get => (string)this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        public HotkeyEditorControl()
        {
            this.Hotkey = new Hotkey(Key.A, ModifierKeys.Control);
            this.InitializeComponent();
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Don't let the event pass further
            // because we don't want standard textbox shortcuts working
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Pressing delete, backspace or escape without modifiers clears the current value
            if (modifiers == ModifierKeys.None &&
                (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                this.Hotkey = null;
                return;
            }

            // If no actual key was pressed - return
            if (key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }

            // Update the value
            this.Hotkey = new Hotkey(key, modifiers);
        }
    }
}
