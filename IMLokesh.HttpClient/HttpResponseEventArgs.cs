using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMLokesh.Utilities.HttpClient
{
    public class HttpResponseEventArgs : EventArgs
    {
        public HttpResponse Response { get; set; }
    }
}
