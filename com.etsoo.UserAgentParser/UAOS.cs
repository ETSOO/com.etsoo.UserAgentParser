using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Operating system
    /// 操作系统
    /// </summary>
    public record UAOS(string Family, int? Major, int? Minor, int? Patch) : UAFamilyItem(Family, Major, Minor, Patch)
    {
        /// <summary>
        /// Write to Json
        /// </summary>
        /// <param name="w">Writer</param>
        /// <param name="options">Options</param>
        public override void ToJson(Utf8JsonWriter w, JsonSerializerOptions options)
        {
            w.WriteStartObject(options.ConvertName("OS"));

            base.ToJson(w, options);

            w.WriteEndObject();
        }

        public override string ToString() => base.ToString();
    }
}
