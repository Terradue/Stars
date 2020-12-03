using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace Terradue.Stars.Services
{
    public static class WebRequestExtensions
    {
        public static HttpWebRequest CloneRequest(this HttpWebRequest originalRequest, Uri newUri)
        {
            return CloneHttpWebRequest(originalRequest, newUri);
        }

        public static WebRequest CloneRequest(this WebRequest originalRequest, Uri newUri)
        {
            var httpWebRequest = originalRequest as HttpWebRequest;
            if (httpWebRequest != null) return CloneHttpWebRequest(httpWebRequest, newUri);
            return CloneWebRequest(originalRequest, newUri);
        }

        private static HttpWebRequest CloneHttpWebRequest(HttpWebRequest old, Uri newUri)
        {
            var @new = (HttpWebRequest)WebRequest.Create(newUri);
            CopyWebRequestProperties(old, @new);
            CopyHttpWebRequestProperties(old, @new);
            CopyHttpWebRequestHeaders(old, @new);
            return @new;
        }

        private static WebRequest CloneWebRequest(WebRequest old, Uri newUri)
        {
            var @new = WebRequest.Create(newUri);
            CopyWebRequestProperties(old, @new);
            CopyWebRequestHeaders(old, @new);
            return @new;
        }

        private static void CopyWebRequestProperties(WebRequest old, WebRequest @new)
        {
            @new.AuthenticationLevel = old.AuthenticationLevel;
            @new.CachePolicy = old.CachePolicy;
            @new.ConnectionGroupName = old.ConnectionGroupName;
            try
            {
                @new.ContentType = old.ContentType;
            }
            catch { }
            try
            {
                @new.UseDefaultCredentials = old.UseDefaultCredentials;
            }
            catch { }
            @new.Credentials = old.Credentials;
            @new.ImpersonationLevel = old.ImpersonationLevel;
            @new.Method = old.Method;
            try
            {
                @new.PreAuthenticate = old.PreAuthenticate;
            }
            catch { }
            @new.Proxy = old.Proxy;
            @new.Timeout = old.Timeout;

            if (old.ContentLength > 0) @new.ContentLength = old.ContentLength;
        }

        private static void CopyWebRequestHeaders(WebRequest old, WebRequest @new)
        {
            string[] allKeys = old.Headers.AllKeys;
            foreach (var key in allKeys)
            {
                @new.Headers[key] = old.Headers[key];
            }
        }

        private static void CopyHttpWebRequestProperties(HttpWebRequest old, HttpWebRequest @new)
        {
            @new.Accept = old.Accept;
            @new.AllowAutoRedirect = old.AllowAutoRedirect;
            @new.AllowWriteStreamBuffering = old.AllowWriteStreamBuffering;
            @new.AutomaticDecompression = old.AutomaticDecompression;
            @new.ClientCertificates = old.ClientCertificates;
            @new.SendChunked = old.SendChunked;
            @new.TransferEncoding = old.TransferEncoding;
            // @new.Connection = old.Connection;
            @new.ContentType = old.ContentType;
            @new.ContinueDelegate = old.ContinueDelegate;
            @new.CookieContainer = old.CookieContainer;
            @new.Date = old.Date;
            @new.Expect = old.Expect;
            @new.Host = old.Host;
            // @new.IfModifiedSince = old.IfModifiedSince;
            @new.KeepAlive = old.KeepAlive;
            @new.MaximumAutomaticRedirections = old.MaximumAutomaticRedirections;
            @new.MaximumResponseHeadersLength = old.MaximumResponseHeadersLength;
            @new.MediaType = old.MediaType;
            @new.Pipelined = old.Pipelined;
            @new.ProtocolVersion = old.ProtocolVersion;
            @new.ReadWriteTimeout = old.ReadWriteTimeout;
            @new.Referer = old.Referer;
            @new.Timeout = old.Timeout;
            @new.UnsafeAuthenticatedConnectionSharing = old.UnsafeAuthenticatedConnectionSharing;
            @new.UserAgent = old.UserAgent;
        }

        private static void CopyHttpWebRequestHeaders(HttpWebRequest old, HttpWebRequest @new)
        {
            var allKeys = old.Headers.AllKeys;
            foreach (var key in allKeys)
            {
                switch (key.ToLower(CultureInfo.InvariantCulture))
                {
                    // Skip all these reserved headers because we have to set them through properties
                    case "accept":
                    case "connection":
                    case "content-length":
                    case "content-type":
                    case "date":
                    case "expect":
                    case "host":
                    case "if-modified-since":
                    case "range":
                    case "referer":
                    case "transfer-encoding":
                    case "user-agent":
                    case "proxy-connection":
                        break;
                    default:
                        @new.Headers[key] = old.Headers[key];
                        break;
                }
            }
        }
    }
}