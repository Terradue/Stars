using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface
{
    public interface IItemCollection : ICollection<IItem>
    {
        IReadOnlyList<IResourceLink> GetLinks();
    }
}