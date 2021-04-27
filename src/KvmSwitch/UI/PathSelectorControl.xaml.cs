namespace KvmSwitch.UI
{
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    /// Interaction logic for PathSelectorControl.xaml
    /// </summary>
    public partial class PathSelectorControl : UserControl
    {
        public PathSelectorControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(string),
                typeof(PathSelectorControl),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = true
                });

        public string SelectedPath
        {
            get => (string)this.GetValue(SelectedPathProperty);
            set => this.SetValue(SelectedPathProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(PathSelectorControl), new PropertyMetadata(null));

        public string Header
        {
            get => (string)this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.SelectedPath = fileDialog.FileName;
            }
        }

        private void SelectedPathTxtBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SelectedPath = this.SelectedPathTxtBox.Text;
        }
    }
}
