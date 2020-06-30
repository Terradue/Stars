using System;
using System.Net;
using System.Threading.Tasks;
using Stars.Router;

namespace Stars
{
    public class ResourceGrabber
    {
        internal IRoute CreateRoute(string input)
        {
            try
            {
                WebRequest request = WebRequest.Create(input);
                return new WebRoute(request);
            }
            catch (NotSupportedException)
            {
                return new UnknownRoute(input);
            }

        }

    }
}