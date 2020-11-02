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

        public LocalFileDestination(FileInfo file)
        {
            this.file = file;
        }

        public Uri Uri => new Uri(file.FullName);

        public static LocalFileDestination Create(string destination)
        {
            FileInfo file = new FileInfo(destination);
            return new LocalFileDestination(file);
        }

        public void Create()
        {
            file.Directory.Create();
        }

        public IDestination RelativeTo(IRoute route, IRoute subroute)
        {
            string relPath = "";
            if (subroute.Uri.IsAbsoluteUri)
            {
                var relUri = route.Uri.MakeRelativeUri(subroute.Uri);
                if (relUri.IsAbsoluteUri)
                {
                    if (!string.IsNullOrEmpty(Path.GetDirectoryName(subroute.Uri.AbsolutePath)) &&
                        !string.IsNullOrEmpty(Path.GetDirectoryName(route.Uri.AbsolutePath)) &&
                        Path.GetDirectoryName(subroute.Uri.AbsolutePath).Contains(Path.GetDirectoryName(route.Uri.AbsolutePath)))
                    {
                        var startIndex = Path.GetDirectoryName(subroute.Uri.AbsolutePath).IndexOf(Path.GetDirectoryName(route.Uri.AbsolutePath));
                        relPath = Path.GetDirectoryName(subroute.Uri.AbsolutePath).Substring(startIndex).Replace(Path.GetDirectoryName(route.Uri.AbsolutePath), "");
                    }
                    else
                        relPath = string.Format("{0}{1}", relUri.Host, relUri.AbsolutePath);
                }
                else
                    relPath = Path.GetDirectoryName(relUri.ToString());
            }
            else
                relPath = Path.GetDirectoryName(subroute.Uri.ToString());
            var newDirPath = Path.Join(file.FullName, relPath);
            DirectoryInfo dir = new DirectoryInfo(newDirPath);
            return new LocalDirectoryDestination(dir);
        }

        public IDestination To(IRoute route)
        {
            string relPath = "";
            if (route.Uri.IsAbsoluteUri)
            {
                var relUri = route.Uri.MakeRelativeUri(route.Uri);
                if (relUri.IsAbsoluteUri)
                {
                    if (!string.IsNullOrEmpty(Path.GetDirectoryName(route.Uri.AbsolutePath)) &&
                        !string.IsNullOrEmpty(Path.GetDirectoryName(Uri.AbsolutePath)) &&
                        Path.GetDirectoryName(route.Uri.AbsolutePath).Contains(Path.GetDirectoryName(route.Uri.AbsolutePath)))
                    {
                        var startIndex = Path.GetDirectoryName(route.Uri.AbsolutePath).IndexOf(Path.GetDirectoryName(Uri.AbsolutePath));
                        relPath = Path.GetDirectoryName(route.Uri.AbsolutePath).Substring(startIndex).Replace(Path.GetDirectoryName(Uri.AbsolutePath), "");
                    }
                    else
                        relPath = string.Format("{0}{1}", relUri.Host, relUri.AbsolutePath);
                }
                else
                    relPath = Path.GetDirectoryName(relUri.ToString());
            }
            else
                relPath = Path.GetDirectoryName(route.Uri.ToString());
            var newDirPath = Path.Join(file.Directory.FullName, relPath);
            DirectoryInfo dir = new DirectoryInfo(newDirPath);
            return new LocalDirectoryDestination(dir);
        }

        public IDestination To(string relPath)
        {
            var newDirPath = Path.Join(file.Directory.FullName, relPath);
            DirectoryInfo dir = new DirectoryInfo(newDirPath);
            return new LocalDirectoryDestination(dir);
        }
    }
}