using com.etsoo.Utils.Serialization;
using System.Text;
using System.Text.Json;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Family item
    /// </summary>
    public record UAFamilyItem(string Family, int? Major, int? Minor, int? Patch)
    {
        /// <summary>
        /// Parse items
        /// 解析项目
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Items</returns>
        public static (string Family, int? Major, int? Minor, int? Patch) Parse(string input)
        {
            int? Major = null, Minor = null, Patch = null;

            var parts = input.Split('/');
            var Family = parts[0];

            if (parts.Length > 1)
            {
                var verions = parts[1].Split(new[] { '.', '_' });
                if (int.TryParse(verions[0], out var major))
                    Major = major;
                if (verions.Length > 1 && int.TryParse(verions[1], out var minor))
                    Minor = minor;
                if (verions.Length > 2 && int.TryParse(verions[2], out var patch))
                    Patch = patch;
            }

            return (Family, Major, Minor, Patch);
        }

        /// <summary>
        /// Write to Json
        /// </summary>
        /// <param name="w">Writer</param>
        /// <param name="options">Options</param>
        public virtual void ToJson(Utf8JsonWriter w, JsonSerializerOptions options)
        {
            w.WriteString(options.ConvertName("Family"), Family);

            if (Major.HasValue)
            {
                w.WriteNumber(options.ConvertName("Major"), Major.Value);
            }

            if (Minor.HasValue)
            {
                w.WriteNumber(options.ConvertName("Major"), Minor.Value);
            }

            if (Patch.HasValue)
            {
                w.WriteNumber(options.ConvertName("Major"), Patch.Value);
            }
        }

        /// <summary>
        /// Override ToString
        /// 重写ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Family);
            // Avoid Windows 10 10 string
            if (!Family.StartsWith("Windows") && Major.HasValue)
            {
                sb.Append(' ');
                sb.Append(Major.Value);

                if (Minor.HasValue)
                {
                    sb.Append('.');
                    sb.Append(Minor.Value);

                    if (Patch.HasValue)
                    {
                        sb.Append('.');
                        sb.Append(Patch.Value);
                    }
                }
            }
            return sb.ToString();
        }
    }
}