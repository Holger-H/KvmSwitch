
namespace KvmSwitch.Hardware
{
    public class MonitorSource
    {
        public int Code { get; set; }
        public string Name { get; set; }

        #region Overrides of Object

        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}