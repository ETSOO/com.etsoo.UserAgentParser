using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.String;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.etsoo.UserAgentParser
{
    /// <summary>
    /// User-Agent parser
    /// 用户代理信息解析器
    /// </summary>
    public class UAParser
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference
        // http://regexstorm.net/tester
        // First level items splitter through space outside of parenthesis
        // 通过非括号内的空字符穿，对第一层项目拆分
        private static readonly Regex itemsRegex = new(@"\s+(?![^\(]+\))", RegexOptions.Compiled);

        // Second level items splitter
        // 第二层项目拆分
        private static readonly Regex secondItemsRegex = new(@"\s*;\s*");

        // Word replacement
        // 单词替换
        private static readonly Regex wordRegex = new(@"(?<!(^|\s))([A-Z])");

        // Language match like en, zh-CN
        // 语言匹配
        private static readonly Regex languageRegex = new(@"[a-z]{2}(-[A-Z]{2})?", RegexOptions.Compiled);

        // Version splitter
        // 版本拆分
        private static readonly Regex versionRegex = new(@"(?<=[\s/])\d+(?:[\._]\d+)*", RegexOptions.Compiled);

        /// <summary>
        /// Device detectors
        /// </summary>
        public static readonly SortedDictionary<string, List<(Regex reg, UADeviceFamily family, string? company, string? brand)>> Devices = new()
        {
            {
                "LM",
                new()
                {
                    (new Regex(@"^LM-X\d+$", RegexOptions.Compiled), UADeviceFamily.Mobile, "LG", "K40")
                }
            },
            {
                "SM", new()
                {
                    (new Regex(@"^SM-T\d+$", RegexOptions.Compiled), UADeviceFamily.Tablet, "SAMSUNG", "Galaxy Tab")
                }
            }
        };

        /// <summary>
        /// Is user agent not null and empty and could be parsed
        /// 用户代理是否不为null和空并且可以解析
        /// </summary>
        public bool Valid { get; }

        /// <summary>
        /// Is bot
        /// 是否为网上机器人
        /// </summary>
        public bool IsBot { get; }

        /// <summary>
        /// Is mobile
        /// 是否为手机
        /// </summary>
        public bool IsMobile { get; }

        /// <summary>
        /// Device
        /// 设备
        /// </summary>
        public UADevice? Device { get; }

        /// <summary>
        /// Operation system
        /// 操作系统
        /// </summary>
        public UAOS? OS { get; }

        /// <summary>
        /// Client
        /// 客户端
        /// </summary>
        public UAClient? Client { get; }

        /// <summary>
        /// Source data
        /// 原始解析数据
        /// </summary>
        public string? Source { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="userAgent">User agent string</param>
        public UAParser(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return;

            // Remove line breaks
            userAgent = userAgent.Replace('\r', '\0').Replace('\n', '\0');

            Valid = true;
            Source = userAgent;

            // Split parts
            var parts = itemsRegex.Split(userAgent);
            var pLen = parts.Length;
            if (pLen == 1)
            {
                // Computer
                Device = new UADevice(UADeviceFamily.Computer);

                // Simplese case, like "PostmanRuntime/6.7.1", or "Windows-Media-Player/11.0.5721.5145"
                var client = wordRegex.Replace(parts[0].Replace('-', ' '), " $2");
                var (Family, Major, Minor, Patch) = UAFamilyItem.Parse(client);
                Client = new UAClient(Family, Major, Minor, Patch);
            }
            else
            {
                // OS and client
                string? os = null, client = null, language = null, chrome = null, company = null, brand = null, model = null;

                // Device family
                var family = UADeviceFamily.Computer;

                var p = 0;
                while (p < pLen)
                {
                    // Current part
                    var part = parts[p];

                    if (p == 0)
                    {
                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent
                        // First part
                        // Mozilla/*.0 is the general token that says the browser is Mozilla-compatible.
                        // For historical reasons, almost every browser today sends it.
                        if (!part.StartsWith("Mozilla/"))
                        {
                            // For some old browsers, first part is the client
                            // Like "Opera/9.60 (Windows NT 6.0; U; en) Presto/2.1.1"
                            client = part;
                        }
                    }
                    else if (p == 1 && part.StartsWith('(') && part.EndsWith(')'))
                    {
                        // OS and device
                        var secondParts = secondItemsRegex.Split(part.TrimStart('(').TrimEnd(')'));
                        var sLen = secondParts.Length;
                        if (sLen == 1 && secondParts[0].StartsWith("+http"))
                        {
                            // Googlebot/2.1 (+http://www.google.com/bot.html)
                            family = UADeviceFamily.Bot;
                        }
                        else
                        {
                            var s = 0;
                            while (s < sLen)
                            {
                                var spart = secondParts[s];

                                if (spart.StartsWith("+http"))
                                {
                                    // Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)
                                    family = UADeviceFamily.Bot;
                                    if (s > 0)
                                        client = secondParts[s - 1];
                                }
                                else if ((spart.Length == 2 || spart.Length == 5) && languageRegex.IsMatch(spart))
                                {
                                    // Language may passed here
                                    language = spart;
                                }
                                else if (spart.StartsWith("Windows "))
                                {
                                    os = spart switch
                                    {
                                        "Windows NT 5.0" => "Windows 2000",
                                        "Windows NT 5.1" => "Windows XP",
                                        "Windows NT 5.2" => "Windows Server 2003",
                                        "Windows NT 6.0" => "Windows Vista",
                                        "Windows NT 6.1" => "Windows 7",
                                        "Windows NT 6.2" => "Windows 8",
                                        "Windows NT 6.3" => "Windows 8.1",
                                        "Windows NT 10.0" => "Windows 10",
                                        _ => spart
                                    };

                                    // For a convenient version
                                    os += '/' + spart.Split(' ').Last();
                                }
                                else if (spart.StartsWith("MSIE "))
                                {
                                    client = spart.Replace(' ', '/');
                                }
                                else if (spart.StartsWith("Trident/"))
                                {
                                    var tridents = spart.Split('/');
                                    if (decimal.TryParse(tridents[1], out var version))
                                        client = "MSIE/" + (version + 4);
                                }
                                else if (spart.Equals("Mobile", StringComparison.OrdinalIgnoreCase))
                                {
                                    family = UADeviceFamily.Mobile;
                                }
                                else if (spart.Equals("Tablet", StringComparison.OrdinalIgnoreCase))
                                {
                                    family = UADeviceFamily.Tablet;
                                }
                                else if (spart.Equals("SMART-TV", StringComparison.OrdinalIgnoreCase))
                                {
                                    family = UADeviceFamily.TV;
                                }
                                else if (spart.Equals("Apple TV", StringComparison.OrdinalIgnoreCase))
                                {
                                    company = "Apple";
                                    family = UADeviceFamily.TV;
                                }
                                else if (spart.Equals("Macintosh", StringComparison.OrdinalIgnoreCase))
                                {
                                    company = "Apple";
                                    brand = spart;
                                    s++;
                                    os = parseAppleOS(secondParts[s]);
                                }
                                else if (spart.Equals("iPhone", StringComparison.OrdinalIgnoreCase))
                                {
                                    family = UADeviceFamily.Mobile;
                                    company = "Apple";
                                    brand = spart;
                                    s++;
                                    os = parseAppleOS(secondParts[s]);
                                }
                                else if (spart.Equals("iPad", StringComparison.OrdinalIgnoreCase) || spart.Equals("iPod", StringComparison.OrdinalIgnoreCase))
                                {
                                    family = UADeviceFamily.Tablet;
                                    company = "Apple";
                                    brand = spart;
                                    s++;
                                    os = parseAppleOS(secondParts[s]);
                                }
                                else if (spart == "X11")
                                {
                                    s++;
                                    os = parseLinuxOs(secondParts[s]);
                                }
                                else if (os == null && spart.StartsWith("Linux"))
                                {
                                    os = parseLinuxOs(secondParts[s]);
                                }
                                else if (spart.StartsWith("Android"))
                                {
                                    family = UADeviceFamily.Mobile;

                                    os = spart.Replace(' ', '/');
                                }
                                else if (spart.StartsWith("rv:"))
                                {
                                    // Release version
                                }
                                else if (s + 1 == sLen)
                                {
                                    // Last item, os or brand and model
                                    if (versionRegex.IsMatch(spart))
                                    {
                                        // Like Mozilla/5.0 (SMART-TV; Linux; Tizen 2.4.0)
                                        os = spart.Replace(' ', '/');
                                    }
                                    else
                                    {
                                        var buildPos = spart.IndexOf(" Build/");
                                        if (buildPos != -1)
                                            spart = spart.Substring(0, buildPos);

                                        // Like SAMSUNG SM-T585
                                        var lastParts = spart.Split(' ');
                                        if (lastParts.Length > 1)
                                        {
                                            company = lastParts[0].AsSpan().ToPascalWord().ToString();
                                        }

                                        // SM-T585
                                        model = lastParts.Last();

                                        // Match collection, first 2 letters match to reduce unnecessary calculation
                                        if (Devices.TryGetValue(model.Substring(0, 2), out var regItems))
                                        {
                                            foreach (var regItem in regItems)
                                            {
                                                if (regItem.reg.IsMatch(model))
                                                {
                                                    family = regItem.family;
                                                    if (regItem.company != null)
                                                        company = regItem.company;
                                                    if (regItem.brand != null)
                                                        brand = regItem.brand;

                                                    // One time match is enough
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                s++;
                            }
                        }
                    }
                    else if (part.StartsWith("Language/"))
                    {
                        // Wechat pass way
                        language = part.Split('/')[1];
                    }
                    else if (part.StartsWith("Chrome/"))
                    {
                        chrome = part;
                    }
                    else if (part.StartsWith("FxiOS/"))
                    {
                        client = part.Replace("FxiOS", "iOS Firefox");
                    }
                    else if (part.StartsWith("SamsungBrowser/"))
                    {
                        client = part;
                    }
                    else if (family == UADeviceFamily.Computer && part.Contains("Mobile"))
                    {
                        family = UADeviceFamily.Mobile;
                    }
                    else if (family == UADeviceFamily.Computer && part.Contains("Tablet"))
                    {
                        family = UADeviceFamily.Tablet;
                    }
                    else if (family == UADeviceFamily.Computer && part.Contains("TV"))
                    {
                        family = UADeviceFamily.TV;
                    }

                    if (p > 1 && p + 1 == pLen && client == null)
                    {
                        // Last item
                        if (part.StartsWith("Safari/"))
                        {
                            if (chrome == null)
                                client = part;
                            else
                                client = chrome;
                        }
                        else
                        {
                            // Replace abbr with full name of the client
                            client = part
                                .Replace("OPR/", "Opera/")
                                .Replace("Edg/", "Edge/");
                        }
                    }

                    p++;
                }

                // Device
                Device = new UADevice(family, company, brand, model);
                IsBot = family == UADeviceFamily.Bot;
                IsMobile = family == UADeviceFamily.Mobile;

                // OS
                if (os != null)
                {
                    var (Family, Major, Minor, Patch) = UAFamilyItem.Parse(os);
                    OS = new UAOS(Family, Major, Minor, Patch);
                }

                // Client
                if (client != null)
                {
                    var (Family, Major, Minor, Patch) = UAFamilyItem.Parse(client);
                    Client = new UAClient(Family, Major, Minor, Patch, language);
                }
            }
        }

        private string? parseAppleOS(string part)
        {
            // Version match
            var versionMatch = versionRegex.Match(part);
            if (versionMatch.Success)
            {
                var index = part.LastIndexOf("Mac OS", versionMatch.Index);
                if (index == -1)
                    return "iOS/" + versionMatch.Value;
                else
                    return part.Substring(index, versionMatch.Index - 1 - index) + '/' + versionMatch.Value;
            }

            return null;
        }

        private string? parseLinuxOs(string part)
        {
            // Version match
            var versionMatch = versionRegex.Match(part);
            if (versionMatch.Success)
            {
                return part.Substring(0, versionMatch.Index - 1) + '/' + versionMatch.Value;
            }
            else
            {
                return part.Split(' ').First();
            }
        }

        /// <summary>
        /// To Json string
        /// 获取Json字符串
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="includeSource">Include the source UA</param>
        /// <returns>Json string</returns>
        public async Task<string> ToJsonAsync(JsonSerializerOptions? options = null, bool includeSource = false)
        {
            var bw = new ArrayBufferWriter<byte>();
            await ToJsonAsync(bw, options, includeSource);
            return Encoding.UTF8.GetString(bw.WrittenSpan);
        }

        /// <summary>
        /// To Json data
        /// 获取Json数据
        /// </summary>
        /// <param name="writer">Json writer</param>
        /// <param name="options">Options</param>
        /// <param name="includeSource">Include the source UA</param>
        /// <returns>Json string</returns>
        public async Task ToJsonAsync(IBufferWriter<byte> writer, JsonSerializerOptions? options = null, bool includeSource = false)
        {
            // Default options
            options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

            // Utf8JsonWriter
            using var w = options.CreateJsonWriter(writer);

            // Object start
            w.WriteStartObject();

            if (includeSource)
            {
                w.WriteString(options.ConvertName("Source"), Source);
            }

            if (Device != null)
            {
                Device.ToJson(w, options);

                if (OS != null)
                {
                    OS.ToJson(w, options);
                }

                if (Client != null)
                {
                    Client.ToJson(w, options);
                }
            }

            // Object end
            w.WriteEndObject();

            // Flush & dispose
            await w.DisposeAsync();
        }

        /// <summary>
        /// To short name
        /// 获取短名称
        /// </summary>
        /// <returns>Name</returns>
        public string ToShortName()
        {
            if (!Valid || Device == null)
                return string.Empty;

            var items = new List<string>();

            var d = Device.ToString();
            if (!string.IsNullOrEmpty(d))
                items.Add(d);

            if (OS != null)
            {
                items.Add(OS.Family);
            }

            if (Client != null)
            {
                items.Add(Client.Family);
            }

            return string.Join(' ', items);
        }

        /// <summary>
        /// To readable string
        /// 获取可读的字符串
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            if (!Valid || Device == null)
                return string.Empty;

            var items = new List<string>();

            var d = Device.ToString();
            if (!string.IsNullOrEmpty(d))
                items.Add(d);

            if (OS != null)
            {
                items.Add(OS.ToString());
            }

            if (Client != null)
            {
                items.Add(Client.ToString());
            }

            return string.Join(' ', items);
        }
    }
}
