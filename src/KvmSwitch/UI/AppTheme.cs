namespace KvmSwitch.UI
{
    using ModernWpf;

    public class AppTheme
    {
        public AppTheme(string name, ApplicationTheme? value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }

        public ApplicationTheme? Value { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
