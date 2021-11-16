using System;
using System.IO;
using System.Linq;
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
        private readonly char[] WRONG_FILENAME_STARTING_CHAR = new char[] { ' ', '.', '-', '$', '&' };

        private S3ObjectDestination(Uri s3Uri, IResource resource = null)
        {
            if (!s3Uri.Scheme.Equals("s3", StringComparison.CurrentCultureIgnoreCase))
                throw new InvalidDataException("Only s3 URL supported");
            this.s3Uri = s3Uri;
            this.resource = resource;
        }

        public string BucketName => S3UriParser.GetBucketName(s3Uri);

        public static S3ObjectDestination Create(string s3UriStr, IResource route = null)
        {
            Uri s3Uri = new Uri(s3UriStr);
            var dest = new S3ObjectDestination(s3Uri, route);
            if (route != null) dest = (S3ObjectDestination)dest.To(route);
            return dest;
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

            // to avoid wrong filename such as '$value'
            if (WRONG_FILENAME_STARTING_CHAR.Contains(filename[0]) && subroute.ResourceType == ResourceType.Asset)
            {
                if (resource != null && resource.ResourceType == ResourceType.Item)
                    filename = (resource as IItem).Id + ".zip";
                else
                    filename = "asset.zip";
            }

            // if the relPath requested is null, we will build one from the origin route to the new one
            if (relPathFix == null)
            {
                if (subroute.Uri.IsAbsoluteUri)
                {
                    // Let's see if the 2 routes are relative
                    var relUri = subroute.Uri.MakeRelativeUri(Uri);
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
                if (relPath == null || relPath.StartsWith(".."))
                    relPath = relPathFix ?? "";
            }
            string newFilePath = filename;
            if (!string.IsNullOrEmpty(relPath))
                newFilePath = Path.Combine(relPath, filename);
            Uri newUri = new Uri(string.Format("s3:/" + Path.GetFullPath(Path.Combine(
                                               "/" + S3UriParser.GetBucketName(s3Uri),
                                               Path.GetDirectoryName(S3UriParser.GetKey(s3Uri)) ?? "",
                                               newFilePath))));
            return new S3ObjectDestination(newUri);
        }

        public override string ToString()
        {
            return s3Uri.ToString();
        }
    }
}