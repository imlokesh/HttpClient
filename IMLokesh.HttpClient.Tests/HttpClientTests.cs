using System.Net;
using Xunit.Abstractions;

namespace IMLokesh.HttpClient.Tests
{
    public class HttpClientTests
    {
        private readonly ITestOutputHelper Debug;

        public HttpClientTests(ITestOutputHelper output)
        {
            Debug = output;
        }

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
        public async Task CloudflareBlocksRequest()
        {
            var http = new Http() { };

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
            var res = await http.RequestAsync(url, headers: headers.ToHttpRequestHeaders());

            // Debug.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));
            Debug.WriteLine(http.HttpClientHandler.SslProtocols.ToString());

            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Fact]
        public async Task AutoRedirectOptionShouldWork()
        {
            var redirectUrl = "https://api.ipify.org";
            var testUrl = $"https://httpbin.org/redirect-to?url={WebUtility.UrlEncode(redirectUrl)}";


            var http = new Http() { SwallowExceptions = false };

            var res = await http.RequestAsync(testUrl);

            Assert.Equal(redirectUrl, res.RequestUri.ToString().TrimEnd('/'));


            http = new Http(new HttpConfig { AutoRedirect = false });

            res = await http.RequestAsync("https://httpbin.org/redirect-to?url=https%3A%2F%2Fapi.ipify.org");

            Assert.Equal(testUrl, res.RequestUri.ToString());
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.Equal(redirectUrl, res.ResponseHeaders.Get("Location").TrimEnd('/'));
        }

        [Fact]
        public async Task ServerCertificateCustomValidationCallbackTest()
        {
            var http = new Http() { SwallowExceptions = false };

            var ex = await Assert.ThrowsAsync<HttpException>(async () => await http.RequestAsync("https://expired.badssl.com/"));

            Assert.NotNull(ex);
            Assert.NotNull(ex.InnerException?.InnerException);
            Assert.IsType<System.Security.Authentication.AuthenticationException>(ex.InnerException?.InnerException);

            http = new Http(new HttpConfig
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, err) =>
                {
                    return true;
                },
            })
            { SwallowExceptions = false };

            var res = await http.RequestAsync("https://expired.badssl.com/");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
    }
}