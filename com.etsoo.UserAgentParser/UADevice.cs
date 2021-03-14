using com.etsoo.Utils.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Device
    /// 设备
    /// </summary>
    public record UADevice (UADeviceFamily Family, string? Company = null, string? Brand = null, string? Model = null)
    {
        /// <summary>
        /// Write to Json
        /// </summary>
        /// <param name="w">Writer</param>
        /// <param name="options">Options</param>
        public void ToJson(Utf8JsonWriter w, JsonSerializerOptions options)
        {
            // Device
            w.WriteStartObject(options.ConvertName("Device"));

            // Family, type
            w.WriteString(options.ConvertName("Family"), Family.ToString());

            if (!string.IsNullOrEmpty(Company))
            {
                w.WriteString(options.ConvertName("Company"), Company);
            }

            if (!string.IsNullOrEmpty(Brand))
            {
                w.WriteString(options.ConvertName("Brand"), Brand);
            }

            if (!string.IsNullOrEmpty(Model))
            {
                w.WriteString(options.ConvertName("Model"), Model);
            }

            w.WriteEndObject();
        }

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
