using System;
using System.IO;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;

namespace Stars.Service.Supply.Destination
{
    public class LocalDirectoryDestination : IDestination
    {
        private readonly DirectoryInfo directory;

        public LocalDirectoryDestination(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public Uri Uri => new Uri(directory.FullName);

        public static LocalDirectoryDestination Create(string destination)
        {
            DirectoryInfo dir = new DirectoryInfo(destination);
            return new LocalDirectoryDestination(dir);
        }

        public void Create()
        {
            directory.Create();
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
            var newDirPath = Path.Join(directory.FullName, relPath);
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
            var newDirPath = Path.Join(directory.FullName, relPath);
            DirectoryInfo dir = new DirectoryInfo(newDirPath);
            return new LocalDirectoryDestination(dir);
        }

        public IDestination To(string relPath)
        {
            var newDirPath = Path.Join(directory.FullName, relPath);
            DirectoryInfo dir = new DirectoryInfo(newDirPath);
            return new LocalDirectoryDestination(dir);
        }
    }
}