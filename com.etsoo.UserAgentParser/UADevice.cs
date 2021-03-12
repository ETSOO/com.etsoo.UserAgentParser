using System.Collections.Generic;
using System.Text;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Device
    /// 设备
    /// </summary>
    public record UADevice (UADeviceFamily Family, string? Company = null, string? Brand = null, string? Model = null)
    {
        /// <summary>
        /// Override ToString
        /// 重写ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            var items = new List<string>();
            if (!string.IsNullOrEmpty(Company))
            {
                items.Add(Company);
            }
            if (!string.IsNullOrEmpty(Brand))
            {
                items.Add(Brand);
            }
            if (!string.IsNullOrEmpty(Model))
            {
                items.Add(Model);
            }
            return string.Join(' ', items);
        }
    }
}
