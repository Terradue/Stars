// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3ObjectDestination.cs

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class S3ObjectDestination : IDestination
    {
        private readonly S3Url s3Url;
        private readonly IResource resource;
        private readonly char[] WRONG_FILENAME_STARTING_CHAR = new char[] { ' ', '.', '-', '$', '&' };

        private S3ObjectDestination(Uri s3Uri, IResource resource = null)
        {
            s3Url = S3Url.ParseUri(s3Uri);
            this.resource = resource;
        }

        public string BucketName => s3Url.Bucket;

        public static S3ObjectDestination Create(string s3UriStr, IResource route = null)
        {
            Uri s3Uri = new Uri(s3UriStr);
            var dest = new S3ObjectDestination(s3Uri, route);
            if (route != null) dest = (S3ObjectDestination)dest.To(route);
            return dest;
        }

        public Uri Uri => s3Url.Uri;

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
                if (relPath == null || relPath.StartsWith(".."))
                    relPath = relPathFix ?? "";
            }

            string newFilePath = filename.TrimStart('/');
            if (!string.IsNullOrEmpty(relPath))
                newFilePath = Path.Combine(relPath, filename.TrimStart('/'));

            // Remove parentheses and their content (e.g. in Copernicus OData URLs like ".../v1/Assets(abc...)/...")
            newFilePath = Regex.Replace(newFilePath, @"\([^/]*\)", String.Empty);

            S3Url newS3Url = (S3Url)s3Url.Clone();
            newS3Url.Key = Path.Combine(Path.GetDirectoryName(s3Url.Key) ?? String.Empty, newFilePath);
            return new S3ObjectDestination(newS3Url.Uri);
        }

        public override string ToString()
        {
            return s3Url.ToString();
        }
    }
}
