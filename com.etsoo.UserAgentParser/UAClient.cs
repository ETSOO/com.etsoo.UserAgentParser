using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Client
    /// 客户端
    /// </summary>
    public record UAClient(string Family, int? Major, int? Minor, int? Patch, string? Language = null) : UAFamilyItem(Family, Major, Minor, Patch)
    {
        /// <summary>
        /// Write to Json
        /// </summary>
        /// <param name="w">Writer</param>
        /// <param name="options">Options</param>
        public override void ToJson(Utf8JsonWriter w, JsonSerializerOptions options)
        {
            w.WriteStartObject(options.ConvertName("Client"));

            base.ToJson(w, options);

            if (!string.IsNullOrEmpty(Language))
                w.WriteString(options.ConvertName("Language"), Language);

            w.WriteEndObject();
        }

        public override string ToString() => base.ToString();
    }
}
