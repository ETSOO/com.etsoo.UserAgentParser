using System.Text;

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
        /// Override ToString
        /// 重写ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Family);
            if (Major.HasValue)
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