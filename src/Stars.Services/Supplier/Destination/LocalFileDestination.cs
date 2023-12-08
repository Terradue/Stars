using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class LocalFileDestination : IDestination
    {
        private readonly IFileInfo file;
        private readonly IResource resource;
        private readonly char[] WRONG_FILENAME_STARTING_CHAR = new char[] { ' ', '.', '-', '$', '&' };

        private LocalFileDestination(IFileInfo file, IResource resource)
        {
            this.file = file;   // note: file can be the directory containing the zip file in case of nested zips
            this.resource = resource;
        }

        public Uri Uri => new Uri("file://" + file.FullName);

        public bool Exists => file.Exists;

        // Field 'file' can refer to a directory, in that case base directory is that directory,
        // otherwise the parent directory of 'file'
        public string BaseDirectory => Directory.Exists(file.FullName) ? file.FullName : file.Directory.FullName;

        public static LocalFileDestination Create(IDirectoryInfo directory, IResource route, bool ignoreEmptyFilename = false)
        {
            var filename = Path.GetFileName(route.Uri.ToString());
            if (route.ContentDisposition != null && !string.IsNullOrEmpty(route.ContentDisposition.FileName))
                filename = route.ContentDisposition.FileName;
            if (string.IsNullOrEmpty(filename) && !ignoreEmptyFilename)
                filename = Guid.NewGuid().ToString("N") + "/";
            string path = filename == null ? directory.FullName : Path.Combine(directory.FullName, filename);
            return new LocalFileDestination(directory.FileSystem.FileInfo.FromFileName(path), route);
        }

        public void PrepareDestination()
        {
            if ( !file.Directory.Exists)
                file.Directory.Create();
        }

        public IDestination To(IResource subroute, string relPathFix = null)
        {
            // we first integrate the relPath
            string relPath = relPathFix ?? String.Empty;

            // we identify the filename
            string filename = Path.GetFileName(subroute.Uri.IsAbsoluteUri ? subroute.Uri.LocalPath : subroute.Uri.ToString());
            if (subroute.ContentDisposition != null && !string.IsNullOrEmpty(subroute.ContentDisposition.FileName))
                filename = subroute.ContentDisposition.FileName;

            if (String.IsNullOrEmpty(filename) && subroute.ResourceType == ResourceType.Asset)
                filename = "asset.zip";

            // to avoid wrong filename such as '$value'
            if (WRONG_FILENAME_STARTING_CHAR.Contains(filename[0]) && subroute.ResourceType == ResourceType.Asset){
                if ( resource != null && resource.ResourceType == ResourceType.Item)
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
            if (relPath.StartsWith("/")) relPath = relPath.Substring(1);
            if (filename.StartsWith("/")) filename = filename.Substring(1);
            var newFilePath = Path.Combine(BaseDirectory, relPath, filename);
            return new LocalFileDestination(file.FileSystem.FileInfo.FromFileName(newFilePath), subroute);
        }

        public override string ToString()
        {
            return Uri.LocalPath;
        }
    }
}