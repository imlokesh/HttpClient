using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IMLokesh.Utilities.HttpClient
{
    public class HttpRequestEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; set; }
    }
}
