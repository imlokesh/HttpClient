namespace IMLokesh.HttpClient.Tests
{
    public class HttpClientTests
    {
        [Theory]
        [MemberData(nameof(HttpVersionTestData))]
        public async Task HttpVersionIsCorrect(Version version, string expectedProtocol)
        {
            var http = new Http() { Version = version };
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
            var http = new Http();
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
    }
}