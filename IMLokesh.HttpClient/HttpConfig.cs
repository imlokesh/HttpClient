using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace IMLokesh.HttpClient
{
    public class HttpConfig
    {
        public static HttpConfig DefaultConfig = new HttpConfig();

        public DecompressionMethods? AutomaticDecompression { get; set; } = null;

        public bool? AutoRedirect { get; set; } = null;
        public Func<CookieContainer> CreateCookieContainer => () => new CookieContainer(int.MaxValue, int.MaxValue, 40960);

        public Func<HttpClientHandler, System.Net.Http.HttpClient> CreateHttpClient { get; set; } = (handler) => new System.Net.Http.HttpClient(handler);
        public Func<HttpClientHandler> CreateHttpHandler { get; set; } = () => new HttpClientHandler();

        public HttpRequestHeaders Headers { get; set; } = HttpHelperMethods.NewHttpRequestHeaders();
        public EventHandler<HttpResponseEventArgs> OnResponse { get; set; }
        public EventHandler<HttpRequestEventArgs> OnRequest { get; set; }

        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback { get; set; } = null;

        public SslProtocols? SslProtocols { get; set; } = null;
        public bool? SwallowExceptions { get; set; } = null;

        public TimeSpan? Timeout { get; set; } = null;

        public Version Version { get; set; } = new Version(2, 0);
    }
}