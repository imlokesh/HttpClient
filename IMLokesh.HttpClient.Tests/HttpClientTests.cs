namespace IMLokesh.HttpClient.Tests
{
    public class HttpClientTests
    {
        [Theory]
        [MemberData(nameof(HttpVersionTestData))]
        public async Task HttpVersionIsCorrect(Version version, string result)
        {
            var http = new Http() { Version = version };
            var res = await http.RequestAsync("https://http2.pro/api/v1");
            var protocol = res.JObject["protocol"]?.Value<string>();
            Assert.Equal(result, protocol);
        }

        public static IEnumerable<object[]> HttpVersionTestData()
        {
            yield return new object[] { new Version(2, 0), "HTTP/2.0" };
            yield return new object[] { null, "HTTP/1.1" };
            yield return new object[] { new Version(1, 0), "HTTP/1.0" };
            yield return new object[] { new Version(1, 1), "HTTP/1.1" };
        }
    }
}