namespace KvmSwitch
{
    using System.Reflection;
    using log4net;
    using Microsoft.Win32;

    internal class AutoStartManager
    {
        private readonly ILog log = LogManager.GetLogger(typeof(AutoStartManager));

        /// <summary>
        ///     Enables the automatic start of the Application.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> auto start will be enabled.</param>
        public void EnableAutoStart(bool enable)
        {
            this.log.Info($"{nameof(EnableAutoStart)} {enable}");

            var registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (registryKey == null)
            {
                this.log.Error("RegistryKey 'SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run' does not not exist.");
                return;
            }

            const string valueName = "KvmSwitch";

            if (enable)
            {
                registryKey.SetValue(valueName, Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                registryKey.DeleteValue(valueName, false);
            }
        }
    }
}
