using NUnit.Framework;
using System.Collections.Generic;

namespace com.etsoo.UserAgentParser.Tests
{
    public class UAParserTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static IEnumerable<TestCaseData> ParseData
        {
            get
            {
                yield return new TestCaseData("Mozilla/5.0 (iPhone; CPU iPhone OS 5_1_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B206 Safari/7534.48.3", false, UADeviceFamily.Mobile, "iOS", "Safari");
                yield return new TestCaseData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36 OPR/38.0.2220.41", false, UADeviceFamily.Computer, "Linux", "Opera");
                yield return new TestCaseData("Opera/9.60 (Windows NT 6.0; U; en) Presto/2.1.1", false, UADeviceFamily.Computer, "Windows Vista", "Opera");
                yield return new TestCaseData("PostmanRuntime/6.7.1", false, UADeviceFamily.Computer, null, "Postman Runtime");
                yield return new TestCaseData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", true, UADeviceFamily.Bot, null, "Googlebot");
                yield return new TestCaseData("Googlebot/2.1 (+http://www.google.com/bot.html)", true, UADeviceFamily.Bot, null, "Googlebot");
                yield return new TestCaseData("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0)", false, UADeviceFamily.Computer, "Windows 10", "MSIE");
                yield return new TestCaseData("Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_3 like Mac OS X) AppleWebKit/603.3.8 (KHTML, like Gecko) Mobile/14G60 wxwork/2.1.5 MicroMessenger/6.3.22", false, UADeviceFamily.Mobile, "iOS", "MicroMessenger");
                yield return new TestCaseData("Mozilla/5.0 (Linux; Android 10; LM-X420) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.86 Mobile Safari/537.36", false, UADeviceFamily.Mobile, "Android", "Chrome");
                yield return new TestCaseData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_2_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36", false, UADeviceFamily.Computer, "Mac OS X", "Chrome");
                yield return new TestCaseData("Mozilla/5.0 (Linux; Android 7.0; SAMSUNG SM-T585 Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/7.4 Chrome/59.0.3071.125 Safari/537.36", false, UADeviceFamily.Tablet, "Android", "SamsungBrowser");
                yield return new TestCaseData("Mozilla/5.0 (SMART-TV; Linux; Tizen 2.4.0) AppleWebkit/538.1 (KHTML, like Gecko) SamsungBrowser/1.1 TV Safari/538.1", false, UADeviceFamily.TV, "Tizen", "SamsungBrowser");
            }
        }

        [Test, TestCaseSource(nameof(ParseData))]
        public void Parse_Bulk(string userAgent, bool isBot, UADeviceFamily device, string os, string client)
        {
            // Arrange & act
            var parser = new UAParser(userAgent);

            var result = parser.ToString();

            // Assert
            Assert.IsTrue(parser.IsBot == isBot, $"IsBot not equal to {isBot}");
            Assert.IsTrue(parser.Device?.Family == device, $"Device Family {parser.Device?.Family} not equal to {device}");
            Assert.IsTrue(parser.OS?.Family == os, $"OS Family {parser.OS?.Family} not equal to {os}");
            Assert.IsTrue(parser.Client?.Family == client, $"Client Family {parser.Client?.Family} not equal to {client}");
        }
    }
}