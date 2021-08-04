using System;
using System.IO;
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
        private readonly FileInfo file;
        private readonly IResource resource;
        private readonly char[] WRONG_FILENAME_STARTING_CHAR = new char[] { ' ', '.', '-', '$', '&' };

        private LocalFileDestination(FileInfo file, IResource resource)
        {
            this.file = file;
            this.resource = resource;
        }

        public Uri Uri => new Uri("file://" + file.FullName);

        public bool Exists => file.Exists;

        public static LocalFileDestination Create(string directory, IResource route)
        {
            var filename = Path.GetFileName(route.Uri.ToString());
            if (route.ContentDisposition != null && !string.IsNullOrEmpty(route.ContentDisposition.FileName))
                filename = route.ContentDisposition.FileName;
            if (string.IsNullOrEmpty(filename))
                filename = Guid.NewGuid().ToString("N");
            return new LocalFileDestination(new FileInfo(Path.Join(directory, filename)), route);
        }

        public void PrepareDestination()
        {
            file.Directory.Create();
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
            if (WRONG_FILENAME_STARTING_CHAR.Contains(filename[0]) && subroute.ResourceType == ResourceType.Item)
                filename = (subroute as IItem).Id + ".zip";

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
            var newFilePath = Path.Join(file.Directory.FullName, relPath, filename);
            return new LocalFileDestination(new FileInfo(newFilePath), subroute);
        }

        public override string ToString()
        {
            return Uri.LocalPath;
        }
    }
}