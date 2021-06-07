using System;
using System.Collections.Generic;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Persistence
{
    public abstract class PersistenceMapper
    {
        public virtual LinkedList<IResource> GetAncestorsList(IResource resource)
        {
            LinkedList<IResource> ancestors = new LinkedList<IResource>();
            ancestors.AddFirst(resource);
            return AddParent(ancestors);
        }

        protected LinkedList<IResource> AddParent(LinkedList<IResource> ancestors)
        {
            if ( ancestors.Last.Value.Parent != null && ancestors.Last.Value.Parent != ancestors.Last.Value ){
                ancestors.AddLast(ancestors.Last.Value.Parent);
                AddParent(ancestors);
            }
            return ancestors;
        }
    }
}