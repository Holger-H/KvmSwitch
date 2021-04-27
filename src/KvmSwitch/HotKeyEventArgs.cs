namespace KvmSwitch
{

    using System;
    using System.Windows.Forms;
    using System.Windows.Input;

    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Keys;
        public readonly Key Key;
        public readonly ModifierKeys Modifiers;

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            var param = (uint)hotKeyParam.ToInt64();
            this.Keys = (Keys)((param & 0xffff0000) >> 16);
            this.Key = KeyInterop.KeyFromVirtualKey((int)this.Keys);
            this.Modifiers = (ModifierKeys)(param & 0x0000ffff);
        }
    }
}