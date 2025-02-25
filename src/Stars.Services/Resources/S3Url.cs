﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3Url.cs

using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Terradue.Stars.Services.Resources
{
    public class S3Url : ICloneable
    {
        // DefaultRegion contains a default region for an S3 bucket, when a region
        // cannot be determined, for example when the s3:// schema is used or when
        // path style URL has been given without the region component in the
        // fully-qualified domain name.
        private const string DefaultRegion = "us-east-1";

        private const string ErrBucketNotFound = "bucket name could not be found";
        private const string ErrHostnameNotFound = "hostname could not be found";
        private const string ErrInvalidS3Endpoint = "an invalid S3 endpoint URL";

        // Used to denote type of the S3 bucket.
        private const string accelerated = "accelerated";

        private const string dualStack = "dualstack";

        private const string website = "website";

        // Part of the amazonaws.com domain name.  Set when no region
        // could be ascertain correctly using the S3 endpoint URL.
        private const string amazonAWS = "amazonaws";

        // Part of the query parameters.  Used when retrieving S3
        // object (key) of a particular version.
        private const string versionID = "versionId";

        // Pattern used to parse multiple path and host style S3 endpoint URLs.
        private static readonly Regex s3URLPattern = new Regex(@"^(.+\.)?s3[.-](?:(accelerated|dualstack|website)[.-])?([a-z0-9-]+)\.(?:[a-z0-9-\.]+)");

        // Pattern to extract S3 bucket
        private static readonly Regex s3BucketRegex = new Regex(@"^s3://(?'bucket'[^/]+).*");

        public S3Url(string bucketName, string key)
        {
            Scheme = "s3";
            Bucket = bucketName;
            Key = key;
            PathStyle = true;
            HostStyle = false;
        }

        internal S3Url()
        {
        }

        public bool HostStyle { get; private set; }
        public bool PathStyle { get; private set; }
        public bool Accelerated { get; private set; }
        public bool DualStack { get; private set; }
        public bool Website { get; private set; }

        public string Scheme { get; private set; }

        public string Endpoint { get; private set; }

        public string Bucket { get; set; }

        public string Key { get; set; }

        public string VersionID { get; private set; }

        public string Region { get; private set; }

        public Uri Uri => new Uri(string.Format("{0}://{1}{2}{3}",
                            Scheme,
                            Endpoint == null ? null : Endpoint + "/",
                            PathStyle ? Bucket + "/" : null,
                            Key));

        public Uri EndpointUrl => Endpoint == null ? null : new Uri(string.Format("{0}://{1}", Scheme, Endpoint));

        public static S3Url Parse(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri s3Uri))
            {
                throw new FormatException($"unable to parse url : {url}");
            }
            return ParseUri(s3Uri);
        }

        public static S3Url ParseUri(Uri uri)
        {
            S3Url s3Url = new S3Url();

            switch (uri.Scheme)
            {
                case "s3":
                case "http":
                case "https":
                    s3Url.Scheme = uri.Scheme;
                    break;
                default:
                    throw new FormatException($"unable to parse schema type: {uri.Scheme}");
            }

            // Handle S3 endpoint URL with the schema s3:// that is neither
            // the host style nor the path style.
            if (uri.Scheme == "s3")
            {
                if (uri.Host == "")
                {
                    throw new EntryPointNotFoundException($"bucket not found in the url : {uri}");
                }
                // .NET transform hostnames to lowercase.
                // This is fine with correctly named buckets (all lowercase),
                // but buckets can also be wrongly contain uppercase letters.
                // s3Url.Bucket = uri.Host;
                Match bucketMatch = s3BucketRegex.Match(uri.OriginalString);
                if (bucketMatch.Success) s3Url.Bucket = bucketMatch.Groups["bucket"].Value;
                else s3Url.Bucket = uri.Host;

                if (!string.IsNullOrEmpty(uri.LocalPath) && uri.LocalPath != "/")
                {
                    s3Url.Key = uri.LocalPath.Substring(1);
                }
                s3Url.Region = DefaultRegion;
                s3Url.PathStyle = true;

                return s3Url;

            }

            s3Url.Endpoint = uri.Host;

            if (string.IsNullOrEmpty(uri.Host))
            {
                throw new FormatException(ErrHostnameNotFound);
            }

            var match = s3URLPattern.Match(uri.Host);

            if (!match.Success)
            {
                throw new FormatException(ErrInvalidS3Endpoint);
            }

            var prefix = match.Groups[1].Value;
            var usage = match.Groups[2].Value;
            var region = match.Groups[3].Value;

            if (string.IsNullOrEmpty(prefix))
                s3Url.PathStyle = true;


            var localpath = uri.LocalPath;
            if (!string.IsNullOrEmpty(uri.LocalPath) && uri.LocalPath != "/" && string.IsNullOrEmpty(prefix))
            {
                localpath = uri.LocalPath.Substring(1);
                var index = localpath.IndexOf("/");

                if (index == -1)
                {
                    s3Url.Bucket = localpath;
                }
                else if (index == localpath.Length - 1)
                {
                    s3Url.Bucket = localpath.Substring(0, index);
                }
                else
                {
                    s3Url.Bucket = localpath.Substring(0, index);
                    s3Url.Key = localpath.Substring(index + 1);
                }
            }
            else
            {
                s3Url.HostStyle = true;
                s3Url.Bucket = prefix.TrimEnd('.');

                if (!string.IsNullOrEmpty(uri.LocalPath) && uri.LocalPath != "/")
                {
                    s3Url.Key = uri.LocalPath.Substring(1);
                }
            }

            // An S3 bucket can be either accelerated or website endpoint,
            // but not both.
            if (usage == accelerated)
            {
                s3Url.Accelerated = true;
            }
            else if (usage == website)
            {
                s3Url.Website = true;
            }

            // An accelerated S3 bucket can also be dualstack.
            if (usage == dualStack || region == dualStack)
            {
                s3Url.DualStack = true;
            }

            // Handle the special case of an accelerated dualstack S3
            // endpoint URL:
            //   <BUCKET>.s3-accelerated.dualstack.amazonaws.com/<KEY>.
            // As there is no way to accertain the region solely based on
            // the S3 endpoint URL.
            if (usage != accelerated)
            {
                s3Url.Region = DefaultRegion;
                if (region != amazonAWS)
                {
                    s3Url.Region = region;
                }
            }

            // Query string used when requesting a particular version of a given
            // S3 object (key).
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(query[versionID]))
            {
                s3Url.VersionID = query[versionID];
            }

            return s3Url;

        }

        public object Clone()
        {
            return ParseUri(Uri);
        }

        public void NormalizeKey()
        {
            Key = Key.TrimEnd('/');
        }

        public S3Url WithScheme(string scheme)
        {
            Scheme = scheme;
            return this;
        }

        public S3Url WithBucket(string bucket)
        {
            Bucket = bucket;
            return this;
        }

        public S3Url WithKey(string key)
        {
            Key = key;
            return this;
        }


        public S3Url WithVersionId(string versionId)
        {
            VersionID = versionId;
            return this;
        }


        public S3Url WithRegion(string region)
        {
            Region = region;
            return this;
        }

        public override string ToString()
        {
            return Uri.ToString();
        }

    }
}


