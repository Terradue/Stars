using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Stars.Router
{
    internal class UnknownRoute : IRoute
    {
        private string input;

        public UnknownRoute(string input)
        {
            this.input = input;
        }

        public Uri Uri => new Uri(input);

        public ContentType ContentType => new ContentType("unknown");

        public bool CanGetResource => false;

        public Task<IResource> GetResource()
        {
            throw new NotSupportedException();
        }
    }
}