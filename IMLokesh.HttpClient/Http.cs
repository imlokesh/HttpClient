using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace IMLokesh.HttpClient
{
    public class Http
    {
        private Proxy _proxy;

        /// <summary>
        /// Creates an instance of Http and sets properties according to HttpConfig.DefaultConfig
        /// </summary>
        public Http() : this(HttpConfig.DefaultConfig)
        {
        }

        public EventHandler<HttpResponseEventArgs> OnResponse { get; set; }
        public EventHandler<HttpRequestEventArgs> OnRequest { get; set; }

        /// <summary>
        /// Creates an instance of Http and sets properties according to supplied HttpConfig
        /// </summary>
        /// <param name="config"></param>
        public Http(HttpConfig config)
        {
            HttpClientHandler = config.CreateHttpHandler();

            HttpClient = config.CreateHttpClient(HttpClientHandler);

            if (config.ServerCertificateCustomValidationCallback != null)
            {
                HttpClientHandler.ServerCertificateCustomValidationCallback = config.ServerCertificateCustomValidationCallback;
            }

            if (config.AutoRedirect != null)
            {
                HttpClientHandler.AllowAutoRedirect = (bool)config.AutoRedirect;
            }

            if (config.SslProtocols != null)
            {
                HttpClientHandler.SslProtocols = (SslProtocols)config.SslProtocols;
            }

            if (config.AutomaticDecompression != null)
            {
                HttpClientHandler.AutomaticDecompression = (DecompressionMethods)config.AutomaticDecompression;
            }

            if (config.Timeout != null)
            {
                HttpClient.Timeout = (TimeSpan)config.Timeout;
            }

            if (config.Version != null)
            {
                Version = config.Version;
            }

            if (config.SwallowExceptions != null)
            {
                SwallowExceptions = (bool)config.SwallowExceptions;
            }
            else
            {
                SwallowExceptions = true;
            }

            if (config.OnResponse != null)
            {
                OnResponse = config.OnResponse;
            }

            if (config.OnRequest != null)
            {
                OnRequest = config.OnRequest;
            }

            Proxy = null;
            Cookies = config.CreateCookieContainer();

            Headers = HttpHelperMethods.NewHttpRequestHeaders();

            foreach (var header in config.Headers)
            {
                Headers.Add(header.Key, header.Value);
            }
        }

        public DecompressionMethods AutomaticDecompression
        {
            get => HttpClientHandler.AutomaticDecompression;
            set => HttpClientHandler.AutomaticDecompression = value;
        }

        public bool AutoRedirect
        {
            get => HttpClientHandler.AllowAutoRedirect;
            set => HttpClientHandler.AllowAutoRedirect = value;
        }

        public CookieContainer Cookies
        {
            get => HttpClientHandler.CookieContainer;
            private set
            {
                HttpClientHandler.UseCookies = true;
                HttpClientHandler.CookieContainer = value;
            }
        }

        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        public HttpRequestHeaders Headers { get; }
        public System.Net.Http.HttpClient HttpClient { get; set; }
        public HttpClientHandler HttpClientHandler { get; set; }

        public Proxy Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value ?? new Proxy();

                if (_proxy.Ip == null)
                {
                    HttpClientHandler.Proxy = null;
                    HttpClientHandler.UseProxy = false;
                    return;
                }

                if (_proxy.Ip == string.Empty)
                {
                    HttpClientHandler.Proxy = null;
                    HttpClientHandler.UseProxy = true;
                    return;
                }

                var webProxy = new WebProxy(new Uri(_proxy.ToString(Proxy.ProxyStringFormat.HttpIpPortOnly)));
                if (_proxy.IsAuthenticated)
                {
                    webProxy.Credentials = new NetworkCredential(_proxy.Username, _proxy.Password);
                }

                HttpClientHandler.Proxy = webProxy;
                HttpClientHandler.UseProxy = true;
            }
        }

        public string ProxyAddress
        {
            get => Proxy.ToString();
            set => Proxy = Proxy.Parse(value);
        }

        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback
        {
            get => HttpClientHandler.ServerCertificateCustomValidationCallback;
            set => HttpClientHandler.ServerCertificateCustomValidationCallback = value;
        }

        public SslProtocols SslProtocols
        {
            get => HttpClientHandler.SslProtocols;
            set => HttpClientHandler.SslProtocols = value;
        }

        public bool SwallowExceptions { get; set; }

        public TimeSpan Timeout
        {
            get => HttpClient.Timeout;
            set => HttpClient.Timeout = value;
        }

        public Version Version { get; set; } = null;

        public void ClearCookies()
        {
            Cookies.Clear();
        }

        public async Task<HttpResponse> RequestAsync(string url,
            HttpMethod method = null,
            string referer = null,
            string content = null,
            string contentType = null,
            HttpRequestHeaders headers = null,
            HttpContent httpContent = null,
            bool sendDefaultHeaders = true,
            string downloadFileName = null
            )
        {
            var req = new HttpRequestMessage(method ?? HttpMethod.Get, url);

            // Add default headers
            if (sendDefaultHeaders)
            {
                foreach (var header in Headers)
                {
                    req.Headers.Set(header.Key, header.Value);
                }
            }

            // Add Additional headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    req.Headers.Set(header.Key, header.Value);
                }
            }

            // Add referer header
            if (!string.IsNullOrWhiteSpace(referer))
            {
                req.Headers.Set("referer", referer);
            }

            if (Version != null)
            {
                req.Version = Version;
            }

            // Set request data            
            if (content != null && httpContent != null)
            {
                throw new ArgumentException("Either provide content or httpContent, not both at same time. ", nameof(content));
            }

            if (content != null)
            {
                httpContent = new StringContent(content);
                if (contentType != null)
                {
                    httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            if (httpContent != null)
            {
                req.Content = httpContent;
            }

            OnRequest?.Invoke(this, new HttpRequestEventArgs() { Request = req });

            var sw = Stopwatch.StartNew();
            var httpResponse = new HttpResponse();
            try
            {
                using (var res = await HttpClient.SendAsync(req, CancellationToken).ConfigureAwait(false))
                {
                    if (downloadFileName != null)
                    {
                        using (var resStream = await res.Content.ReadAsStreamAsync())
                        {
                            using (var fs = new FileStream(downloadFileName, FileMode.CreateNew))
                            {
                                await resStream.CopyToAsync(fs);
                            }
                        }
                    }
                    else
                    {
                        httpResponse.Text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    httpResponse.ContentHeaders = res.Content.Headers;
                    httpResponse.ResponseHeaders = res.Headers;
                    httpResponse.StatusCode = res.StatusCode;
                    httpResponse.ReasonPhrase = res.ReasonPhrase;
                    httpResponse.RequestUri = res.RequestMessage.RequestUri;
                    httpResponse.RequestMethod = res.RequestMessage.Method;
                    httpResponse.Version = res.Version;
                }
            }
            catch (TaskCanceledException e)
            {
                if (!CancellationToken.IsCancellationRequested)
                {
                    httpResponse.StatusCode = HttpStatusCode.RequestTimeout;
                    httpResponse.ReasonPhrase = "Operation timed out.";
                    httpResponse.Exception = new HttpRequestException("The http operation did not complete in time. ", e);
                }
                else
                {
                    httpResponse.StatusCode = HttpStatusCode.RequestTimeout;
                    httpResponse.ReasonPhrase = "Canceled by user. ";
                    httpResponse.Exception = e;
                }
            }
            catch (Exception e)
            {
                httpResponse.Exception = e;
            }

            httpResponse.TimeElapsed = sw.Elapsed;

            OnResponse?.Invoke(this, new HttpResponseEventArgs() { Response = httpResponse });

            if (!SwallowExceptions)
            {
                httpResponse.ConfirmSuccess();
            }

            return httpResponse;
        }
    }
}