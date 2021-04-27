namespace KvmSwitch
{
    using System.IO;
    using KvmSwitch.Data;
    using log4net;
    using Newtonsoft.Json;

    internal class ConfigProvider
    {
        private const string ConfigFileName = "Config.json";
        private readonly ILog log = LogManager.GetLogger(typeof(ConfigProvider));

        /// <summary>
        ///     Loads the Configuration
        /// </summary>
        public Config Load()
        {
            this.log.Info("Load Config");
            return File.Exists(ConfigFileName)
                ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFileName))
                : new Config();
        }

        /// <summary>
        ///     Saves the Configuration
        /// </summary>
        public void Save(Config config)
        {
            var configString = JsonConvert.SerializeObject(config, new JsonSerializerSettings());
            File.WriteAllText(ConfigFileName, configString);
            this.log.Info("Config saved");
        }
    }
}
