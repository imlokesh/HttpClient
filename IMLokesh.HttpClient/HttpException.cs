using System;

namespace IMLokesh.HttpClient
{
    public class HttpException : Exception
    {
        public HttpException(HttpResponse res, Exception e) : base(e?.Message, e)
        {
            Response = res;
        }

        public HttpResponse Response { get; set; }
    }
}
