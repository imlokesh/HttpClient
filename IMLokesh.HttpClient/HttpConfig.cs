using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IMLokesh.HttpClient
{
    public class HttpConfig
    {
        public static HttpConfig DefaultConfig = new HttpConfig();

        /// <summary>
        /// Defaults to DecompressionMethods.None
        /// </summary>
        public DecompressionMethods AutomaticDecompression { get; set; }

        /// <summary>
        /// AutoRedirect requests. Default is true. Please note that status codes like 301 and 302 will be considered errors. So use with SwallowExceptions = true.
        /// </summary>
        public bool AutoRedirect { get; set; } = true;

        /// <summary>
        /// Delegate that will be called for initializing a CookieContainer. 
        /// </summary>
        public Func<CookieContainer> CreateCookieContainer => () => new CookieContainer(int.MaxValue, int.MaxValue, 40960);

        /// <summary>
        /// Delegate that will be called for initializing System.Net.Http.HttpClient 
        /// </summary>
        public Func<HttpClientHandler, System.Net.Http.HttpClient> CreateHttpClient { get; set; } = (handler) => new System.Net.Http.HttpClient(handler);

        /// <summary>
        /// Delegate that will be called for initializing HttpClientHandler. 
        /// </summary>
        public Func<HttpClientHandler> CreateHttpHandler { get; set; } = () => new HttpClientHandler();

        /// <summary>
        /// Default headers that will be sent with every request. 
        /// </summary>
        public HttpRequestHeaders Headers { get; set; } = HttpHelperMethods.NewHttpRequestHeaders();

        /// <summary>
        /// Event will be called when an HttpResponse is available. 
        /// </summary>
        public EventHandler<HttpResponseEventArgs> OnResponse { get; set; }

        /// <summary>
        /// Event will be called just before sending a request.
        /// </summary>
        public EventHandler<HttpRequestEventArgs> OnRequest { get; set; }

        /// <summary>
        /// Callback to handle server certificate validation. 
        /// </summary>
        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback { get; set; } = null;

        /// <summary>
        /// Default is true. If false, exceptions will be automatically thrown when status code is not between 200-299. You can check HttpResponse.Exception when this is true.
        /// </summary>
        public bool SwallowExceptions { get; set; } = true;

        /// <summary>
        /// Timeout for requests. Default is 90 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);

        /// <summary>
        /// HttpVersion of requests. Default is 2.0.
        /// </summary>
        public Version Version { get; set; } = new Version(2, 0);
    }
}