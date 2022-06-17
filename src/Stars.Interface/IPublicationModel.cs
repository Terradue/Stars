using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface IPublicationModel
    {
        string Url { get; }

        List<StacLink> AdditionalLinks { get; set; }

        string Collection { get; }
    }
}