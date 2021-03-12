using System.Text;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Operating system
    /// 操作系统
    /// </summary>
    public record UAOS(string Family, int? Major, int? Minor, int? Patch) : UAFamilyItem(Family, Major, Minor, Patch)
    {
        public override string ToString() => base.ToString();
    }
}
