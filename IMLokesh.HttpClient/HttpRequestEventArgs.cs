using System;
using System.Net.Http;

namespace IMLokesh.HttpClient
{
    public class HttpRequestEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; set; }
    }
}
