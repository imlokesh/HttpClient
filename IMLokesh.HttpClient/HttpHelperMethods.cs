using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace IMLokesh.HttpClient
{
    public static class HttpHelperMethods
    {
        public static HttpRequestHeaders NewHttpRequestHeaders()
        {
            using (var req = new HttpRequestMessage())
            {
                return req.Headers;
            }
        }

        public static HttpRequestHeaders ToHttpRequestHeaders(this IEnumerable<string> headers)
        {
            var h = NewHttpRequestHeaders();
            if (headers == null)
            {
                return h;
            }
            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header)) continue;
                var parsed = header.Split(new[] { ':' }, 2);
                h.Add(parsed[0].Trim(), parsed.Length == 1 ? "" : parsed[1].Trim());
            }

            return h;
        }

        public static void Set(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, value);
        }

        public static void Set(this HttpRequestHeaders headers, string name, IEnumerable<string> values)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, values);
        }

        public static List<Cookie> GetAllCookies(this CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

            foreach (var pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                foreach (Cookie c in colCookies) lstCookies.Add(c);
            }

            return lstCookies;
        }

        public static List<Uri> GetAllUris(this CookieContainer cc)
        {
            return cc.GetAllCookies().Select(c => c.Domain).Select(d =>
            {
                d = d.TrimStart('.');
                return new[] {new Uri("http://" + d), new Uri("https://" + d)};
            }).SelectMany(u => u).ToList();

        }

        public static void Clear(this CookieContainer cc)
        {
            foreach (var url in cc.GetAllUris())
            {
                cc.GetCookies(url)
                    .Cast<Cookie>()
                    .ToList()
                    .ForEach(c => c.Expired = true);
            }
        }

        public static Cookie GetCookieByName(this CookieContainer cc, string name)
        {
            return GetCookiesByName(cc, name).FirstOrDefault();
        }

        public static IEnumerable<Cookie> GetCookiesByName(this CookieContainer cc, string name)
        {
            return cc.GetAllCookies().Where(c => c.Name == name);
        }
    }
}