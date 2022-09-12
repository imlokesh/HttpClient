using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        /// <summary>
        /// Event will be called when an HttpResponse is available. 
        /// </summary>
        public EventHandler<HttpResponseEventArgs> OnResponse { get; set; }

        /// <summary>
        /// Event will be called just before sending a request.
        /// </summary>
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

            HttpClientHandler.AllowAutoRedirect = config.AutoRedirect;
            HttpClientHandler.AutomaticDecompression = config.AutomaticDecompression;
            HttpClient.Timeout = config.Timeout;
            Version = config.Version;
            SwallowExceptions = config.SwallowExceptions;
            Proxy = null;
            Headers = HttpHelperMethods.NewHttpRequestHeaders();
            Cookies = config.CreateCookieContainer();

            foreach (var header in config.Headers)
            {
                Headers.Add(header.Key, header.Value);
            }

            if (config.OnResponse != null)
            {
                OnResponse = config.OnResponse;
            }

            if (config.OnRequest != null)
            {
                OnRequest = config.OnRequest;
            }
        }

        /// <summary>
        /// CookieContainer associated with this client. 
        /// </summary>
        public CookieContainer Cookies
        {
            get => HttpClientHandler.CookieContainer;
            private set
            {
                HttpClientHandler.UseCookies = true;
                HttpClientHandler.CookieContainer = value;
            }
        }

        /// <summary>
        /// This cancellation token will be passed to all requests for this client. 
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Headers to be sent with each request. 
        /// </summary>
        public HttpRequestHeaders Headers { get; }

        /// <summary>
        /// System.Net.Http.HttpClient instance associated with this client. 
        /// </summary>
        public System.Net.Http.HttpClient HttpClient { get; set; }

        /// <summary>
        /// HttpClientHandler instance associated with this client. 
        /// </summary>
        public HttpClientHandler HttpClientHandler { get; set; }

        /// <summary>
        /// Proxy object associated with this client. 
        /// </summary>
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

        /// <summary>
        /// ProxyAddress associated with this client.
        /// </summary>
        public string ProxyAddress
        {
            get => Proxy.ToString();
            set => Proxy = Proxy.Parse(value);
        }

        /// <summary>
        /// If false, exceptions will be automatically thrown when status code is not between 200-299. You can check HttpResponse.Exception when this is true. Default is true.
        /// </summary>
        public bool SwallowExceptions { get; set; }

        /// <summary>
        /// Timeout for requests. Default is 90 seconds. 
        /// </summary>
        public TimeSpan Timeout
        {
            get => HttpClient.Timeout;
            set => HttpClient.Timeout = value;
        }

        /// <summary>
        /// HttpVersion of requests. Default is 2.0.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Sends request to the specified url.
        /// </summary>
        /// <param name="url">Url to send the request to. </param>
        /// <param name="method">HttpMethod for the request. Default is HttpMethod.Get</param>
        /// <param name="referer">Referer header for the request. </param>
        /// <param name="content">String content for the request. </param>
        /// <param name="contentType">Content type for the request when content is set. </param>
        /// <param name="headers">Custom headers associated with the request. </param>
        /// <param name="httpContent">HttpContent to send custom content with he request. Use only one of content or httpContent parameter at once.</param>
        /// <param name="sendDefaultHeaders">Defaults to true. Set false to not send default headers associated witht he client. </param>
        /// <param name="downloadFileName">If set, response will be redirected to a file. HttpResponse.Text property will not be set in that event. </param>
        /// <returns>HttpResponse</returns>
        /// <exception cref="HttpException"></exception>
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