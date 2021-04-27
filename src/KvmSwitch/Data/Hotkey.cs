namespace KvmSwitch.Data
{
    using System.Text;
    using System.Windows.Input;

    public class Hotkey
    {
        public Key Key { get; }

        public ModifierKeys Modifiers { get; }

        public Hotkey(Key key, ModifierKeys modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            if (this.Modifiers.HasFlag(ModifierKeys.Control))
            {
                str.Append("Ctrl + ");
            }

            if (this.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                str.Append("Shift + ");
            }

            if (this.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                str.Append("Alt + ");
            }

            if (this.Modifiers.HasFlag(ModifierKeys.Windows))
            {
                str.Append("Win + ");
            }

            str.Append(this.Key);

            return str.ToString();
        }
    }
}
