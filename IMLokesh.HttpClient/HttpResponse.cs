using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace IMLokesh.Utilities.HttpClient
{
    public class HttpResponse
    {
        [JsonIgnore] private IHtmlDocument _document;

        [JsonIgnore] private JObject _jObject;

        [JsonIgnore]
        private static Lazy<object> AngleSharpContext { get; set; } = new Lazy<object>(() => BrowsingContext.New(Configuration.Default));

        [JsonIgnore]
        private static Lazy<object> AngleSharpHtmlParser { get; set; } = new Lazy<object>(() => (AngleSharpContext.Value as IBrowsingContext).GetService<IHtmlParser>());

        public HttpContentHeaders ContentHeaders { get; set; }

        [JsonIgnore]
        public IHtmlDocument Document
        {
            get => _document ?? (_document = (AngleSharpHtmlParser.Value as IHtmlParser).ParseDocument(Text));
            set => _document = value;
        }

        public Exception Exception { get; set; }

        [JsonIgnore]
        public JObject JObject
        {
            get
            {
                if (_jObject != null)
                {
                    return _jObject;
                }

                try
                {
                    _jObject = JObject.Parse(Text);
                    return _jObject;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set => _jObject = value;
        }

        public string GetRelativeUrl(string path)
        {
            return new Uri(RequestUri, path).ToString();
        }

        public string ReasonPhrase { get; set; }
        public HttpMethod RequestMethod { get; set; }
        public Uri RequestUri { get; set; }
        public HttpResponseHeaders ResponseHeaders { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public int StatusCodeInt => (int)StatusCode;
        public string Text { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        public Version Version { get; set; }

        public void ConfirmSuccess()
        {
            var ex = GetException();
            if (ex != null)
            {
                throw ex;
            }
        }

        public HttpException GetException()
        {
            if (Exception != null)
            {
                return new HttpException(this, Exception);
            }

            if (StatusCode < HttpStatusCode.OK || StatusCode >= HttpStatusCode.MultipleChoices)
            {
                return new HttpException(this, new HttpRequestException(StatusCodeInt + (string.IsNullOrWhiteSpace(ReasonPhrase) ? "" : " - " + ReasonPhrase)));
            }

            return null;
        }
    }
}