namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// Client
    /// 客户端
    /// </summary>
    public record UAClient(string Family, int? Major, int? Minor, int? Patch, string? Language = null) : UAFamilyItem(Family, Major, Minor, Patch)
    {
        public override string ToString() => base.ToString();
    }
}
