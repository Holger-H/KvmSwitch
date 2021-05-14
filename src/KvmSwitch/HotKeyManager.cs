namespace KvmSwitch
{
    using KvmSwitch.Native;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;

    /// <summary>
    ///     Manages global HotKeys.
    /// </summary>
    /// <seealso cref="IDisposable" />
    internal sealed class HotKeyManager : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(HotKeyManager));
        private readonly List<int> registeredHotKeyIds = new();
        private readonly ManualResetEvent windowReadyEvent = new(false);
        private MessageWindow messageWindow;
        private int interlockLocation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HotKeyManager"/> class.
        /// </summary>
        public HotKeyManager()
        {
            this.messageWindow = new MessageWindow(this.windowReadyEvent, (args) => this.OnHotKeyPressed(args));
        }

        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        /// <summary>
        ///     Registers a hot key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <returns>Registration id if registered.</returns>
        public int? RegisterHotKey(Key key, ModifierKeys modifiers)
        {
            return this.RegisterHotKey((Keys)KeyInterop.VirtualKeyFromKey(key), modifiers);
        }

        private int? RegisterHotKey(Keys key, ModifierKeys modifiers)
        {
            this.windowReadyEvent.WaitOne();
            int? result = Interlocked.Increment(ref this.interlockLocation);

            this.messageWindow.Dispatcher.Invoke(() =>
            {
                var id = Interlocked.Increment(ref this.interlockLocation);
                if (!RegisterHotKeyInternal(this.messageWindow.Handle, id, (uint)modifiers, (uint)key))
                {
                    this.log.Error($"Unable to register hotkey ({modifiers} + {key})");
                }
                else
                {
                    this.registeredHotKeyIds.Add(id);
                    result = id;
                }
            });

            return result;
        }

        /// <summary>
        ///     Unregisters a hot key.
        /// </summary>
        /// <param name="id">The identifier (registration id).</param>
        /// <returns></returns>
        public void UnregisterHotKey(int id)
        {
            this.messageWindow.Dispatcher.Invoke(() =>
            {
                this.UnRegisterHotKeyInternal(id);
            });
        }

        public void Dispose()
        {
            if (this.messageWindow != null)
            {
                foreach (var registeredHotKeyId in this.registeredHotKeyIds)
                {
                    this.UnregisterHotKey(registeredHotKeyId);
                }
            }

            this.messageWindow?.Close();
            this.messageWindow = null;
        }

        private static bool RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        {
            return User32.RegisterHotKey(hwnd, id, modifiers, key);
        }

        private void UnRegisterHotKeyInternal(int id)
        {
            if (User32.UnregisterHotKey(this.messageWindow.Handle, id))
            {
                this.registeredHotKeyIds.Remove(id);
            }
        }

        private void OnHotKeyPressed(HotKeyEventArgs e) => HotKeyPressed?.Invoke(this, e);

        private class MessageWindow : Window
        {
            private readonly WindowInteropHelper windowInteropHelper;
            private readonly ManualResetEvent windowReadyEvent;
            private readonly Action<HotKeyEventArgs> callback;
            private HwndSource hwndSource;

            public IntPtr Handle { get; private set; }

            public MessageWindow(ManualResetEvent windowReadyEvent, Action<HotKeyEventArgs> callback)
            {
                this.windowReadyEvent = windowReadyEvent;
                this.callback = callback;

                this.windowInteropHelper = new WindowInteropHelper(this);
                this.windowInteropHelper.EnsureHandle();
            }

            protected override void OnSourceInitialized(EventArgs e)
            {
                base.OnSourceInitialized(e);
                this.hwndSource = HwndSource.FromHwnd(this.windowInteropHelper.Handle);
                this.hwndSource.AddHook(this.HwndHook);
                this.Handle = this.hwndSource.Handle;
                this.windowReadyEvent.Set();
            }

            protected override void OnClosed(EventArgs e)
            {
                this.hwndSource.RemoveHook(this.HwndHook);
                this.hwndSource = null;

                base.OnClosed(e);
            }

            private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                const int WM_HOTKEY = 0x0312;

                if (msg != WM_HOTKEY)
                {
                    return IntPtr.Zero;
                }
                this.callback(new HotKeyEventArgs(lParam));

                return IntPtr.Zero;
            }

        }




    }
}
