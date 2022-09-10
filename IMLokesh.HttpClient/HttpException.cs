using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMLokesh.Utilities.HttpClient
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
