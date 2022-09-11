namespace IMLokesh.HttpClient.Tests
{
    public class HttpClientTests
    {
        [Theory]
        [MemberData(nameof(HttpVersionTestData))]
        public async Task HttpVersionIsCorrect(Version version, string expectedProtocol)
        {
            var http = new Http() { Version = version, SwallowExceptions = false };
            var res = await http.RequestAsync("https://http2.pro/api/v1");
            var actualProtocol = res.JObject["protocol"]?.Value<string>();
            Assert.Equal(expectedProtocol, actualProtocol);
        }

        public static IEnumerable<object[]> HttpVersionTestData()
        {
            yield return new object[] { new Version(2, 0), "HTTP/2.0" };
            yield return new object[] { null, "HTTP/1.1" };
            yield return new object[] { new Version(1, 0), "HTTP/1.0" };
            yield return new object[] { new Version(1, 1), "HTTP/1.1" };
        }

        [Theory]
        [MemberData(nameof(HttpMethodTestData))]
        public async Task HttpMethodIsCorrect(HttpMethod method, string expectedMethod)
        {
            var http = new Http() { SwallowExceptions = false };
            var res = await http.RequestAsync("https://httpbin.org/anything", method);
            var actualMethod = res.JObject["method"]?.Value<string>();
            Assert.Equal(actualMethod, expectedMethod);
        }

        public static IEnumerable<object[]> HttpMethodTestData()
        {
            yield return new object[] { null, "GET" };
            yield return new object[] { HttpMethod.Get, "GET" };
            yield return new object[] { HttpMethod.Post, "POST" };
            yield return new object[] { HttpMethod.Put, "PUT" };
            yield return new object[] { HttpMethod.Patch, "PATCH" };
            yield return new object[] { HttpMethod.Delete, "DELETE" };
        }

        [Fact]
        public async Task DownloadFileTest()
        {
            var fileName = "downloadTest.txt";

            File.Delete(fileName);

            var http = new Http() { SwallowExceptions = false };
            var url = "https://httpbin.org/anything";
            await http.RequestAsync(url, downloadFileName: fileName);

            Assert.True(File.Exists(fileName));

            var actualUrl = JObject.Parse(File.ReadAllText(fileName))["url"]?.Value<string>();
            Assert.Equal(url, actualUrl);
        }

        [Fact]
        public async Task DownloadPngFileTest()
        {
            var fileName = "placeholder.png";

            File.Delete(fileName);

            var http = new Http() { SwallowExceptions = false };

            var headers = @"sec-ch-ua: ""Google Chrome"";v=""105"", ""Not)A;Brand"";v=""8"", ""Chromium"";v=""105""
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: ""Windows""
upgrade-insecure-requests: 1
user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36
accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
sec-fetch-site: none
sec-fetch-mode: navigate
sec-fetch-user: ?1
sec-fetch-dest: document
accept-encoding: gzip, deflate, br
accept-language: en-US,en;q=0.9";

            var url = "https://via.placeholder.com/500";
            await http.RequestAsync(url, downloadFileName: fileName, headers: headers.ToHttpRequestHeaders());

            Assert.True(File.Exists(fileName));
        }


    }
}