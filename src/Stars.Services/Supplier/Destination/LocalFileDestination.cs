using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class LocalFileDestination : IDestination
    {
        private readonly FileInfo file;
        private readonly IRoute route;

        private LocalFileDestination(FileInfo file, IRoute route)
        {
            this.file = file;
            this.route = route;
        }

        public Uri Uri => new Uri(file.FullName);

        public bool Exists => file.Exists;

        public static LocalFileDestination Create(string directory, IRoute route)
        {
            var filename = Path.GetFileName(route.Uri.ToString());
            if (route.ContentDisposition != null && !string.IsNullOrEmpty(route.ContentDisposition.FileName))
                filename = route.ContentDisposition.FileName;
            if ( string.IsNullOrEmpty(filename) )
                filename = Guid.NewGuid().ToString("N");
            return new LocalFileDestination(new FileInfo(Path.Join(directory, filename)), route);
        }

        public void PrepareDestination()
        {
            file.Directory.Create();
        }

        public IDestination To(IRoute subroute, string relPathFix = null)
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
                    var relUri = route.Uri.MakeRelativeUri(subroute.Uri);
                    // If not, let's see if they have a common pattern
                    if (relUri.IsAbsoluteUri)
                    {
                        if (!string.IsNullOrEmpty(Path.GetDirectoryName(subroute.Uri.AbsolutePath)) &&
                            !string.IsNullOrEmpty(Path.GetDirectoryName(route.Uri.AbsolutePath)) &&
                            Path.GetDirectoryName(subroute.Uri.AbsolutePath).StartsWith(Path.GetDirectoryName(route.Uri.AbsolutePath)))
                        {
                            relPath = Path.GetDirectoryName(subroute.Uri.AbsolutePath).Replace(Path.GetDirectoryName(route.Uri.AbsolutePath), "");
                        }
                    }
                    else
                        relPath = Path.GetDirectoryName(relUri.ToString());
                }
                else
                    relPath = Path.GetDirectoryName(subroute.Uri.ToString());
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