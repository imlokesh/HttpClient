using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace IMLokesh.HttpClient
{
    public static class QueryHelper
    {
        public static NameValueCollection New()
        {
            return HttpUtility.ParseQueryString("");
        }

        public static NameValueCollection FromString(string str)
        {
            return HttpUtility.ParseQueryString(str);
        }

        public static string ToStringEncoded(this NameValueCollection collection)
        {
            if (collection == null || collection.Count == 0)
            {
                return "";
            }

            var items = collection.AllKeys.SelectMany(collection.GetValues, (k, v) => new { Key = k, Value = v });
            var encoded = items.Select(el => string.Concat(WebUtility.UrlEncode(el.Key), "=", WebUtility.UrlEncode(el.Value)));
            return string.Join("&", encoded).Replace("%20", "+");
        }
    }
}
