// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: RouterService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class RouterService : IStarsService
    {
        public RouterServiceParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly RoutersManager routersManager;
        private readonly ICredentials credentialsManager;

        public RouterService(ILogger<RouterService> logger,
                             IResourceServiceProvider resourceServiceProvider,
                             RoutersManager routersManager,
                             ICredentials credentialsManager)
        {
            Parameters = new RouterServiceParameters();
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            this.routersManager = routersManager;
            this.credentialsManager = credentialsManager;
        }

        public async Task<object> RouteAsync(IResource route, int recursivity, IRouter prevRouter, object state, CancellationToken ct)
        {
            // Stop here if there is no route
            if (route == null)
            {
                return state;
            }

            Exception exception = null;

            // if recursivity threshold is not reached
            if (recursivity <= 0)
            {
                exception = new Exception("Max Depth");
                return await onRoutingExceptionFunction.Invoke(route, prevRouter, exception, state, ct);
            }

            ICatalog catalogNode = route as ICatalog;
            IRouter router = null;
            // If route is not a catalog (there is no more routes from that node)
            if (catalogNode == null)
            {
                // If route is an Item
                if (route is IItem itemNode)
                {
                    // Execute the function for the item and return;
                    return await onItemFunction.Invoke(itemNode, prevRouter, state, ct);
                }
                // Ask the router manager if there is another router available for this route
                router = await routersManager.GetRouterAsync(route);
                // Definitively impossible to Route
                if (router == null)
                {
                    exception = new NotSupportedException(string.Format("Route is unknown : {0}", route.Uri));
                    return await onRoutingExceptionFunction.Invoke(route, prevRouter, exception, state, ct);
                }
                else
                {
                    // New route from new router!
                    var newRoute = await router.RouteAsync(route, ct);
                    // If route is an Item
                    if (newRoute is IItem)
                    {
                        return await onItemFunction.Invoke(newRoute as IItem, prevRouter, state, ct);
                    }
                    // If route is a Collection
                    if (newRoute is ICollection)
                    {
                        // Execute the function for the collection
                        await onCollectionFunction.Invoke(newRoute as ICollection, prevRouter, state, ct);
                    }

                    catalogNode = newRoute as ICatalog;

                    if (catalogNode == null)
                    {
                        exception = new NotSupportedException(string.Format("Route is unknown : {0}", route.Uri));
                        return await onRoutingExceptionFunction.Invoke(route, router, exception, state, ct);
                    }
                }
            }
            else
            {
                // If route is a Collection
                if (catalogNode is ICollection)
                {
                    // Execute the function for the collection
                    await onCollectionFunction.Invoke(catalogNode as ICollection, router, state, ct);
                }
                // If the resource is a catalog, we keep the previous router
                router = prevRouter;
            }

            // Let's get sub routes
            ICollection<IResource> subroutes = catalogNode.GetRoutes(router).ToList();

            state = await beforeBranchingFunction.Invoke(catalogNode, router, state, subroutes, ct);

            List<object> substates = new List<object>();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                var subRoute = subroutes.ElementAt(i);
                var newState = await onBranchingFunction.Invoke(route, subRoute, subroutes, state, ct);
                var substate = await RouteAsync(subRoute, recursivity - 1, router, newState, ct);
                substates.Add(substate);
            }
            return await afterBranchingFunction.Invoke(catalogNode, router, state, substates, ct);
        }


        private Func<ICatalog, IRouter, object, IEnumerable<object>, CancellationToken, Task<object>> afterBranchingFunction = (node, router, state, substates, ct) => { return Task.FromResult(state); };
        public void OnAfterBranching(Func<ICatalog, IRouter, object, IEnumerable<object>, CancellationToken, Task<object>> afterBranchingFunction)
        {
            this.afterBranchingFunction = afterBranchingFunction;
        }

        private Func<ICatalog, IRouter, object, ICollection<IResource>, CancellationToken, Task<object>> beforeBranchingFunction = (node, router, state, subroutes, ct) => { return Task.FromResult(state); };
        public void OnBeforeBranching(Func<ICatalog, IRouter, object, ICollection<IResource>, CancellationToken, Task<object>> beforeBranchingFunction)
        {
            this.beforeBranchingFunction = beforeBranchingFunction;
        }


        private Func<IItem, IRouter, object, CancellationToken, Task<object>> onItemFunction = (node, router, state, ct) => { return Task.FromResult(state); };
        public void OnItem(Func<IItem, IRouter, object, CancellationToken, Task<object>> onItemFunction)
        {
            this.onItemFunction = onItemFunction;
        }

        private Func<ICollection, IRouter, object, CancellationToken, Task<object>> onCollectionFunction = (node, router, state, ct) => { return Task.FromResult(state); };
        public void OnCollection(Func<ICollection, IRouter, object, CancellationToken, Task<object>> onCollectionFunction)
        {
            this.onCollectionFunction = onCollectionFunction;
        }

        private Func<IResource, IRouter, Exception, object, CancellationToken, Task<object>> onRoutingExceptionFunction = (route, router, e, state, ct) => { return Task.FromResult(state); };
        public void OnRoutingException(Func<IResource, IRouter, Exception, object, CancellationToken, Task<object>> onRoutingException)
        {
            onRoutingExceptionFunction = onRoutingException;
        }

        private Func<IResource, IResource, IEnumerable<IResource>, object, CancellationToken, Task<object>> onBranchingFunction = (parentRoute, route, siblings, state, ct) => { return Task.FromResult(state); };
        public void OnBranching(Func<IResource, IResource, IEnumerable<IResource>, object, CancellationToken, Task<object>> onBranchingFunction)
        {
            this.onBranchingFunction = onBranchingFunction;
        }


    }
}
