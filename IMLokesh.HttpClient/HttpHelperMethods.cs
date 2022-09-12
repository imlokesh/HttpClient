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

        /// <summary>
        /// Returns an instance of HttpRequestHeaders as there is no direct constructor. 
        /// </summary>
        /// <returns></returns>
        public static HttpRequestHeaders NewHttpRequestHeaders()
        {
            using (var req = new HttpRequestMessage())
            {
                return req.Headers;
            }
        }

        /// <summary>
        /// Converts an IEnumerable to HttpRequestHeaders. 
        /// </summary>
        /// <param name="headers">An IEnumerable with headers in the form <em>key: value</em>.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts raw header string to HttpRequestHeaders. 
        /// </summary>
        /// <param name="headers">Raw http headers, one per line, in <em>key: value</em> format</param>
        /// <returns></returns>        
        public static HttpRequestHeaders ToHttpRequestHeaders(this string headers)
        {
            var h = NewHttpRequestHeaders();
            if (string.IsNullOrWhiteSpace(headers))
            {
                return h;
            }

            var lines = headers.Replace("\r\n", "\n").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var header in lines)
            {
                if (string.IsNullOrWhiteSpace(header)) continue;
                var parsed = header.Split(new[] { ':' }, 2);
                h.Add(parsed[0].Trim(), parsed.Length == 1 ? "" : parsed[1].Trim());
            }

            return h;
        }

        /// <summary>
        /// Sets a header value overwriting any existing value. 
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, value);
        }

        /// <summary>
        /// Sets a header value overwriting any existing value.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public static void Set(this HttpRequestHeaders headers, string name, IEnumerable<string> values)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, values);
        }

        /// <summary>
        /// Returns all cookies in a specific CookieContainer.
        /// </summary>
        /// <param name="cc">CookieContainer to get cookies from. </param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets all uris with at least one cookie in the cookie container. 
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static List<Uri> GetAllUris(this CookieContainer cc)
        {
            return cc.GetAllCookies().Select(c => c.Domain).Select(d =>
            {
                d = d.TrimStart('.');
                return new[] { new Uri("http://" + d), new Uri("https://" + d) };
            }).SelectMany(u => u).ToList();

        }

        /// <summary>
        /// Deletes all cookies from a CookieContainer. 
        /// </summary>
        /// <param name="cc"></param>
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

        /// <summary>
        /// Returns the first cookie with a specific name inside the CookieContainer irrespective of the url of the cookie. 
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Cookie GetCookieByName(this CookieContainer cc, string name)
        {
            return GetCookiesByName(cc, name).FirstOrDefault();
        }

        /// <summary>
        /// Returns all cookies with a specific name inside the CookieContainer irrespective of the url of the cookie. 
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<Cookie> GetCookiesByName(this CookieContainer cc, string name)
        {
            return cc.GetAllCookies().Where(c => c.Name == name);
        }
    }
}