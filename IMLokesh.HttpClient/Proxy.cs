using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace IMLokesh.Utilities.HttpClient
{
    public class Proxy
    {
        public Proxy()
        {
        }

        public Proxy(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public Proxy(string ip, int port, string username, string password)
        {
            Ip = ip;
            Port = port;
            Username = username;
            Password = password;
        }

        public string Ip { get; set; } = "";
        public string Password { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Username);

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var p = (Proxy)obj;
            return Ip == p.Ip && Port == p.Port && Username == p.Username && Password == p.Password;
        }

        public static Proxy Parse(string proxy)
        {
            var p = new Proxy();
            if (string.IsNullOrWhiteSpace(proxy)) return p;

            proxy = proxy.Trim();

            if (!proxy.StartsWith("http"))
            {
                var proxyParts = proxy.Split(':');
                if (proxyParts.Length != 2 && proxyParts.Length != 4) throw new ArgumentException("Please use \"ip:port\" or \"ip:port:user:pass\" format. ");

                p.Ip = proxyParts[0];
                p.Port = Convert.ToInt32(proxyParts[1]);

                if (proxyParts.Length == 4)
                {
                    p.Username = proxyParts[2];
                    p.Password = proxyParts[3];
                }
                
            }
            else
            {
                var proxyUri = new Uri(proxy);
                p.Ip = proxyUri.DnsSafeHost;
                p.Port = proxyUri.Port;

                if (!string.IsNullOrWhiteSpace(proxyUri.UserInfo))
                {
                    var proxyUserInfo = proxyUri.UserInfo.Split(':');
                    if (proxyUserInfo.Length == 2)
                    {
                        p.Username = proxyUserInfo[0];
                        p.Password = proxyUserInfo[1];
                    }
                    else
                    {
                        throw new ArgumentException("Invalid user info in proxy uri. ");
                    }
                }
            }
            return p;
        }

        public override string ToString()
        {
            return ToString(ProxyStringFormat.Complete);
        }

        public string ToString(ProxyStringFormat stringFormat)
        {
            switch (stringFormat)
            {
                case ProxyStringFormat.IpPortOnly:
                    return $"{Ip}:{Port}";
                case ProxyStringFormat.Complete:
                    return IsAuthenticated ? $"{Ip}:{Port}:{Username}:{Password}" : $"{Ip}:{Port}";
                case ProxyStringFormat.HttpIpPortOnly:
                    return IsAuthenticated ? $"http://{Username}:{Password}@{Ip}:{Port}" : $"http://{Ip}:{Port}";
                case ProxyStringFormat.HttpComplete:
                    return IsAuthenticated ? $"http://{Username}:{Password}@{Ip}:{Port}" : $"http://{Ip}:{Port}";

                default:
                    throw new FormatException($"The '{stringFormat}' format string is not supported.");
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public enum ProxyStringFormat
        {
            IpPortOnly = 1,
            HttpIpPortOnly = 2,
            Complete = 3,
            HttpComplete = 4
        }
    }
}