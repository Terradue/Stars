using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        private readonly RoutersManager routersManager;
        private readonly ICredentials credentialsManager;

        public RouterService(ILogger logger, RoutersManager routersManager, ICredentials credentialsManager)
        {
            this.Parameters = new RouterServiceParameters();
            this.logger = logger;
            this.routersManager = routersManager;
            this.credentialsManager = credentialsManager;
        }

        public async Task ExecuteAsync(IEnumerable<string> inputs)
        {
            foreach (var input in inputs)
            {
                WebRoute initRoute = WebRoute.Create(new Uri(input), credentials: credentialsManager);
                object state = await onBranchingFunction.Invoke(null, initRoute, null, null);
                await Route(initRoute, Parameters.Recursivity, null, state);
            }
        }

        internal async Task<object> Route(IRoute route, int recursivity, IRouter prevRouter, object state)
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
                return await onRoutingExceptionFunction.Invoke(route, prevRouter, exception, state);
            }

            ICatalog catalogNode = route as ICatalog;
            IRouter router = null;
            // If route is not a catalog (there is no more routes fom that node)
            if (catalogNode == null)
            {
                IItem itemNode = route as IItem;
                // If route is an Item
                if (itemNode != null)
                {
                    // Execute the function for the item and return;
                    return await onItemFunction.Invoke(itemNode, prevRouter, state);
                }
                // Ask the router manager if there is another router available for this route
                router = routersManager.GetRouter(route);
                // Definitively impossible to Route
                if (router == null)
                {
                    exception = new NotSupportedException(string.Format("Route is unknown : {0}", route.Uri));
                    return await onRoutingExceptionFunction.Invoke(route, prevRouter, exception, state);
                }
                else
                {
                    // New route from new router!
                    var newRoute = await router.Route(route);
                    if (newRoute is IItem){
                        return await onItemFunction.Invoke(newRoute as IItem, prevRouter, state);
                    }

                    catalogNode = newRoute as ICatalog;

                    if (catalogNode == null)
                    {
                        exception = new NotSupportedException(string.Format("Route is unknown : {0}", route.Uri));
                        return await onRoutingExceptionFunction.Invoke(route, router, exception, state);
                    }
                }
            }
            else
            {
                // If the resource is a catalog, we keep the previous router
                router = prevRouter;
            }

            // Let's get sub routes
            IList<IRoute> subroutes = catalogNode.GetRoutes();

            state = await beforeBranchingFunction.Invoke(catalogNode, router, state);

            List<object> substates = new List<object>();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                var newRoute = subroutes.ElementAt(i);
                var newState = await onBranchingFunction.Invoke(route, newRoute, subroutes, state);
                var substate = await Route(newRoute, recursivity - 1, router, newState);
                substates.Add(substate);
            }
            return await afterBranchingFunction.Invoke(catalogNode, router, state, substates);
        }


        private Func<ICatalog, IRouter, object, IEnumerable<object>, Task<object>> afterBranchingFunction = (node, router, state, substates) => { return Task.FromResult<object>(state); };
        public void OnAfterBranching(Func<ICatalog, IRouter, object, IEnumerable<object>, Task<object>> afterBranchingFunction)
        {
            this.afterBranchingFunction = afterBranchingFunction;
        }

        private Func<ICatalog, IRouter, object, Task<object>> beforeBranchingFunction = (node, router, state) => { return Task.FromResult<object>(state); };
        public void OnBeforeBranching(Func<ICatalog, IRouter, object, Task<object>> beforeBranchingFunction)
        {
            this.beforeBranchingFunction = beforeBranchingFunction;
        }


        private Func<IItem, IRouter, object, Task<object>> onItemFunction = (node, router, state) => { return Task.FromResult<object>(state); };
        public void OnItem(Func<IItem, IRouter, object, Task<object>> onItemFunction)
        {
            this.onItemFunction = onItemFunction;
        }

        private Func<IRoute, IRouter, Exception, object, Task<object>> onRoutingExceptionFunction = (route, router, e, state) => { return Task.FromResult<object>(state); };
        public void OnRoutingException(Func<IRoute, IRouter, Exception, object, Task<object>> onRoutingException)
        {
            this.onRoutingExceptionFunction = onRoutingException;
        }

        private Func<IRoute, IRoute, IList<IRoute>, object, Task<object>> onBranchingFunction = (parentRoute, route, siblings, state) => { return Task.FromResult<object>(state); };
        public void OnBranching(Func<IRoute, IRoute, IList<IRoute>, object, Task<object>> onBranchingFunction)
        {
            this.onBranchingFunction = onBranchingFunction;
        }


    }
}