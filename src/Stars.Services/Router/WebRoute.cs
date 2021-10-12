using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.S3;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public class WebRoute : IResource, IStreamable
    {
        private readonly WebRequest request;
        private readonly ulong contentLength;

        private WebResponse cachedResponse;

        internal WebRoute(WebRequest request, ulong contentLength = 0)
        {
            this.request = request;
            this.contentLength = contentLength;
        }

        public static WebRoute Create(Uri uri, ulong contentLength = 0, ICredentials credentials = null)
        {
            ICredentials creds = credentials;
            Uri requestUri = uri;
            // URL substitution
            var urlFind = Environment.GetEnvironmentVariable("STARS_URL_FIND");
            var urlReplace = Environment.GetEnvironmentVariable("STARS_URL_REPLACE");
            if (!string.IsNullOrEmpty(urlFind) && !string.IsNullOrEmpty(urlReplace))
            {
                requestUri = new Uri(Regex.Replace(uri.ToString(), urlFind, urlReplace));
            }
            WebRequest request = CreateWebRequest(requestUri, creds);
            if (!string.IsNullOrEmpty(requestUri.UserInfo) && requestUri.UserInfo.Contains(":"))
            {
                request.Credentials = new NetworkCredential(requestUri.UserInfo.Split(':')[0], requestUri.UserInfo.Split(':')[1]);
                try
                {
                    request.PreAuthenticate = true;
                }
                catch { }
            }
            else if (!string.IsNullOrEmpty(requestUri.UserInfo) && requestUri.UserInfo == "preauth")
            {
                request.PreAuthenticate = true;
            }
            // If no credentials provided
            if (!request.PreAuthenticate)
            {
                // Let's check there is a credential recorded to be reused
                UriBuilder uriBuilder = new UriBuilder(requestUri);
                try
                {
                    if (string.IsNullOrEmpty(uriBuilder.UserName))
                        uriBuilder.UserName = "preauth";
                    if (credentials?.GetCredential(uriBuilder.Uri, "Basic") != null)
                    {
                        request.PreAuthenticate = true;
                        SetBasicAuthHeader(request, credentials?.GetCredential(uriBuilder.Uri, "Basic"));
                    }
                }
                catch { }
            }
            if (request is FtpWebRequest && request.Proxy == null && WebRequest.DefaultWebProxy != null)
            {
                request.Proxy = WebRequest.DefaultWebProxy;
            }
            return new WebRoute(request, contentLength);
        }

        public static void SetBasicAuthHeader(WebRequest request, NetworkCredential credential)
        {
            string authInfo = credential.UserName + ":" + credential.Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

        private static WebRequest CreateWebRequest(Uri uri, ICredentials credentials = null)
        {
            var request = WebRequest.Create(uri);
            request.Headers.Set("User-Agent", "Stars/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            if (credentials != null)
                request.Credentials = credentials;
            if ( request is FileWebRequest && !File.Exists((request as FileWebRequest).RequestUri.AbsolutePath) )
                throw new FileNotFoundException("File not found: " + (request as FileWebRequest).RequestUri.AbsolutePath, (request as FileWebRequest).RequestUri.AbsolutePath);
            return request;
        }

        public async Task<Stream> GetStreamAsync()
        {
            var response = await request.CloneRequest(request.RequestUri).GetResponseAsync();
            return response.GetResponseStream();
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType
        {
            get
            {
                string mediaType = null;
                if (request is FileWebRequest)
                    mediaType = MimeTypes.GetMimeType(Path.GetFileName(request.RequestUri.ToString()));

                if (string.IsNullOrEmpty(mediaType) && !string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.ContentType]))
                    mediaType = CachedHeaders[HttpResponseHeader.ContentType];

                if (string.IsNullOrEmpty(mediaType) || mediaType == MimeTypes.FallbackMimeType)
                    mediaType = MimeTypes.GetMimeType(MimeTypes.GetMimeType(ContentDisposition.FileName));

                return new ContentType(mediaType);
            }
        }

        internal async Task Remove()
        {
            if (request is HttpWebRequest)
                throw new NotSupportedException("Cannot remove HTTP object");

            if (request is FileWebRequest)
            {
                FileInfo file = new FileInfo(request.RequestUri.LocalPath);
                file.Delete();
                return;
            }

            if (request is FtpWebRequest)
            {
                var ftpWebRequest = (FtpWebRequest)request.CloneRequest(request.RequestUri);
                ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                using (FtpWebResponse resp = (FtpWebResponse)ftpWebRequest.GetResponse())
                {
                    if (!(resp.StatusCode != FtpStatusCode.CommandOK))
                        throw new Exception(string.Format("FTP server returned an error ({0})", resp.StatusCode));
                }
                return;
            }

            if (request is S3WebRequest)
            {
                var s3WebRequest = (S3WebRequest)request.CloneRequest(request.RequestUri);
                s3WebRequest.Method = S3RequestMethods.DeleteObject;
                using (S3ObjectWebResponse<DeleteObjectResponse> resp = (S3ObjectWebResponse<DeleteObjectResponse>)s3WebRequest.GetResponse())
                {
                    if (!(resp.GetObject().HttpStatusCode == HttpStatusCode.OK))
                        throw new Exception(string.Format("S3 server returned an error ({0})", resp.GetObject().HttpStatusCode));
                }
                return;
            }

            throw new NotSupportedException(string.Format("Cannot remove {0} object", request.GetType()));
        }

        internal IEnumerable<WebRoute> ListFolder()
        {
            if (request is HttpWebRequest) throw new NotImplementedException();
            if (request is FileWebRequest)
            {
                DirectoryInfo dir = new DirectoryInfo(request.RequestUri.LocalPath);
                return dir.GetFiles("*", SearchOption.AllDirectories).Select(f =>
                        WebRoute.Create(new Uri("file://" + f.FullName), Convert.ToUInt64(f.Length)));
            }
            if (request is FtpWebRequest) throw new NotImplementedException();
            if (request is S3WebRequest)
            {
                return ListFolder((S3WebRequest)request.CloneRequest(request.RequestUri));
            }
            return null;
        }

        private IEnumerable<WebRoute> ListFolder(S3WebRequest s3WebRequest)
        {
            s3WebRequest.Method = S3RequestMethods.ListObject;
            using (S3ObjectWebResponse<ListObjectsResponse> resp = (S3ObjectWebResponse<ListObjectsResponse>)s3WebRequest.GetResponse())
            {
                return resp.GetObject().S3Objects.Select(o =>
                {
                    S3WebRequest s3Req = s3WebRequest.Clone(o.Key); ;
                    return new WebRoute(s3Req, Convert.ToUInt64(o.Size));
                });
            }
        }

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength
        {
            get
            {
                if (request is FileWebRequest)
                    return Convert.ToUInt64(new FileInfo(request.RequestUri.ToString().Replace("file://", "")).Length);
                if (contentLength > 0) return contentLength;
                if (request is FtpWebRequest)
                {
                    try
                    {
                        FtpWebRequest ftpWebRequest = request.CloneRequest(request.RequestUri) as FtpWebRequest;
                        ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                        using (var response = ftpWebRequest.GetResponse())
                        {
                            return Convert.ToUInt64(response.ContentLength);
                        }
                    }
                    catch (Exception e)
                    {
                        return 0;
                    }
                }
                if (request is S3WebRequest)
                {
                    try
                    {
                        S3WebRequest s3WebRequest = request.CloneRequest(request.RequestUri) as S3WebRequest;
                        s3WebRequest.Method = S3RequestMethods.ListObject;
                        using (S3ObjectWebResponse<ListObjectsResponse> response = (S3ObjectWebResponse<ListObjectsResponse>)s3WebRequest.GetResponse())
                        {
                            return Convert.ToUInt64(response.GetObject().S3Objects.First().Size);
                        }
                    }
                    catch (Exception e)
                    {
                        return 0;
                    }
                }
                if (!string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.ContentLength]))
                    return Convert.ToUInt64(CachedHeaders[HttpResponseHeader.ContentLength]);
                return 0;
            }
        }

        public ContentDisposition ContentDisposition
        {
            get
            {
                if (!string.IsNullOrEmpty(CachedHeaders["Content-Disposition"]))
                    return new ContentDisposition(CachedHeaders["Content-Disposition"]);
                if (!string.IsNullOrEmpty(CachedHeaders["X-Artifactory-Filename"]))
                    return new ContentDisposition() { FileName = CachedHeaders["X-Artifactory-Filename"] };
                return new ContentDisposition() { FileName = Path.GetFileName(request.RequestUri.ToString()) };
            }

        }

        public bool CanBeRanged
        {
            get
            {
                try
                {
                    if (cachedResponse is FileWebResponse) return true;
                    if (!string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.AcceptRanges]))
                        return (CachedHeaders[HttpResponseHeader.AcceptRanges] == "bytes");
                }
                catch { }
                return false;
            }
        }

        public WebHeaderCollection CachedHeaders
        {
            get
            {
                if (cachedResponse == null)
                    CacheResponse().GetAwaiter().GetResult();
                return cachedResponse?.Headers;
            }
        }

        public bool IsFolder
        {
            get
            {
                if (request is HttpWebRequest) return false;
                if (request is FileWebRequest) return (FileAttributes.Directory & File.GetAttributes(request.RequestUri.ToString())) == FileAttributes.Directory;
                if (request is FtpWebRequest) return IsFtpFolder((FtpWebRequest)request.CloneRequest(request.RequestUri));
                if (request is S3WebRequest) return IsS3Folder((S3WebRequest)request.CloneRequest(request.RequestUri));
                return false;
            }
        }

        private bool IsS3Folder(S3WebRequest s3WebRequest)
        {
            s3WebRequest.Method = S3RequestMethods.ListObject;
            try
            {
                S3ObjectWebResponse<ListObjectsResponse> response = (S3ObjectWebResponse<ListObjectsResponse>)s3WebRequest.GetResponse();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool IsFtpFolder(FtpWebRequest request)
        {
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            try
            {
                request.GetResponse();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private async Task CacheResponse()
        {
            var cacheRequest = request.CloneRequest(request.RequestUri);
            var response = await cacheRequest.GetResponseAsync();
            cachedResponse = new CachedWebResponse(response);
            response.Close();
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            var rangedRequest = request.CloneRequest(request.RequestUri);
            if (rangedRequest is HttpWebRequest)
            {
                if (end == -1)
                    (rangedRequest as HttpWebRequest).AddRange(start);
                else
                    (rangedRequest as HttpWebRequest).AddRange(start, end);
            }
            if (rangedRequest is FtpWebRequest)
            {
                (rangedRequest as FtpWebRequest).ContentOffset = start;
            }
            if (rangedRequest is S3WebRequest)
            {
                (rangedRequest as S3WebRequest).Method = S3RequestMethods.DownloadRangedObject;
            }
            var response = await rangedRequest.GetResponseAsync();
            var stream = response.GetResponseStream();
            if (rangedRequest is FileWebRequest)
            {
                stream.Seek(start, SeekOrigin.Begin);
            }
            return stream;
        }
    }
}