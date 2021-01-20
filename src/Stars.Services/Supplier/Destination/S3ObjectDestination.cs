using System;
using System.IO;
using System.Net;
using System.Net.S3;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class S3ObjectDestination : IDestination
    {
        private readonly Uri s3Uri;
        private readonly IResource resource;

        private S3ObjectDestination(Uri s3Uri, IResource resource)
        {
            if ( !s3Uri.Scheme.Equals("s3", StringComparison.CurrentCultureIgnoreCase) )
                throw new InvalidDataException("Only s3 URL supported");
            this.s3Uri = s3Uri;
            this.resource = resource;
        }

        public static S3ObjectDestination Create(string s3UriStr, IResource route)
        {
            Uri s3Uri = new Uri(s3UriStr);
            WebRoute s3Route = WebRoute.Create(s3Uri);
            return (S3ObjectDestination)(new S3ObjectDestination(s3Uri, s3Route)).To(route);
        }

        public Uri Uri => s3Uri;

        public void PrepareDestination()
        {
            // S3WebRequest s3WebRequest = (S3WebRequest)WebRequest.Create(s3Uri);
        }

        public IDestination To(IResource subroute, string relPathFix = null)
        {
            // we first integrate the relPath
            string relPath = relPathFix ?? "";

            // we identify the filename
            string filename = Path.GetFileName(subroute.Uri.IsAbsoluteUri ? subroute.Uri.LocalPath : subroute.Uri.ToString());
            if (subroute.ContentDisposition != null && !string.IsNullOrEmpty(subroute.ContentDisposition.FileName))
                filename = subroute.ContentDisposition.FileName;

            // if the relPath requested is null, we will build one from the origin route to the new one
            if (relPathFix == null)
            {
                if (subroute.Uri.IsAbsoluteUri)
                {
                    // Let's see if the 2 routes are relative
                    var relUri = Uri.MakeRelativeUri(subroute.Uri);
                    // If not, let's see if they have a common pattern
                    if (relUri.IsAbsoluteUri)
                    {
                        if (!string.IsNullOrEmpty(Path.GetDirectoryName(subroute.Uri.AbsolutePath)) &&
                            !string.IsNullOrEmpty(Path.GetDirectoryName(Uri.AbsolutePath)) &&
                            Path.GetDirectoryName(subroute.Uri.AbsolutePath).StartsWith(Path.GetDirectoryName(Uri.AbsolutePath)))
                        {
                            relPath = Path.GetDirectoryName(subroute.Uri.AbsolutePath).Replace(Path.GetDirectoryName(Uri.AbsolutePath), "");
                        }
                    }
                    else
                        relPath = Path.GetDirectoryName(relUri.ToString());
                }
                else
                    relPath = Path.GetDirectoryName(subroute.Uri.ToString());
            }
            var newFilePath = Path.Join(Path.GetDirectoryName(s3Uri.AbsolutePath), relPath, filename);
            UriBuilder uriBuilder = new UriBuilder(s3Uri);
            uriBuilder.Path = newFilePath;
            return new S3ObjectDestination(uriBuilder.Uri, subroute);
        }

        public override string ToString()
        {
            return s3Uri.ToString();
        }
    }
}