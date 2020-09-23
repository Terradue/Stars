using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;

namespace Terradue.Stars.Service.Router
{
    public class RoutingService : IStarsService
    {
        public RoutingTaskParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly RoutersManager routersManager;

        public RoutingService(ILogger logger, RoutersManager routersManager)
        {
            this.Parameters = new RoutingTaskParameters();
            this.logger = logger;
            this.routersManager = routersManager;
        }

        public async Task ExecuteAsync(IEnumerable<string> inputs)
        {
            foreach (var input in inputs)
            {
                IRoute initRoute = WebRoute.Create(new Uri(input));
                object state = await onBranchingFunction.Invoke(null, initRoute, null, null);
                await Route(initRoute, Parameters.Recursivity, null, state);
            }
        }

        internal async Task<object> Route(IRoute route, int recursivity, IRouter prevRouter, object state)
        {
            // Stop here if there is no route
            if (route == null) return state;

            INode node = null;
            Exception exception = null;

            // if recursivity threshold is not reached
            if (recursivity > 0)
            {
                // let's keep going -> follow the route to the node
                try
                {
                    node = await route.GoToNode();
                    if (node == null)
                        throw new NullReferenceException("Null");
                }
                catch (AggregateException ae)
                {
                    exception = ae.InnerException;
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            else
            {
                exception = new Exception("Max Depth");
            }

            // Print the route info if node cannot be reached
            if (exception != null)
            {
                return await onRoutingToNodeExceptionFunction.Invoke(route, prevRouter, exception, state);
            }

            IRoutable routableNode = null;
            IRouter router = null;
            // If node is not routable (there is no more native routes fom that node)
            if (!(node is IRoutable))
            {
                // Ask the router manager if there is another router available for this resource
                router = routersManager.GetRouter(node);
                // Definitively no more routes
                if (router == null)
                {
                    return await onLeafNodeFunction.Invoke(node, prevRouter, state);
                }
                else
                {
                    // New route from new router!
                    routableNode = await router.Route(node);
                }
            }
            else
            {
                // If the resource is natively routable
                routableNode = (IRoutable)node;
                router = prevRouter;
            }

            // Let's get sub routes
            IList<IRoute> subroutes = routableNode.GetRoutes();

            // I any, otherwise consider it a leaf node
            if (subroutes.Count() == 0)
                return await onLeafNodeFunction.Invoke(node, prevRouter, state);

            state = await onBranchingNodeFunction.Invoke(routableNode, router, state);

            state = await beforeBranchingFunction.Invoke(routableNode, router, state);


            List<object> substates = new List<object>();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                var newRoute = subroutes.ElementAt(i);
                var newState = await onBranchingFunction.Invoke(route, newRoute, subroutes, state);
                var substate = await Route(newRoute, recursivity - 1, router, newState);
                substates.Add(substate);
            }
            return await afterBranchingFunction.Invoke(routableNode, router, state, substates);
        }


        private Func<IRoutable, IRouter, object, IEnumerable<object>, Task<object>> afterBranchingFunction = (node, router, state, substates) => { return Task.FromResult<object>(state); };
        public void OnAfterBranching(Func<IRoutable, IRouter, object, IEnumerable<object>, Task<object>> afterBranchingFunction)
        {
            this.afterBranchingFunction = afterBranchingFunction;
        }

        private Func<IRoutable, IRouter, object, Task<object>> beforeBranchingFunction = (node, router, state) => { return Task.FromResult<object>(state); };
        public void OnBeforeBranching(Func<IRoutable, IRouter, object, Task<object>> beforeBranchingFunction)
        {
            this.beforeBranchingFunction = beforeBranchingFunction;
        }


        private Func<INode, IRouter, object, Task<object>> onLeafNodeFunction = (node, router, state) => { return Task.FromResult<object>(state); };
        public void OnLeafNode(Func<INode, IRouter, object, Task<object>> onLeafNodeFunction)
        {
            this.onLeafNodeFunction = onLeafNodeFunction;
        }

        private Func<INode, IRouter, object, Task<object>> onBranchingNodeFunction = (node, router, state) => { return Task.FromResult<object>(state); };
        public void OnBranchingNode(Func<INode, IRouter, object, Task<object>> onBranchingNodeFunction)
        {
            this.onBranchingNodeFunction = onBranchingNodeFunction;
        }

        private Func<IRoute, IRouter, Exception, object, Task<object>> onRoutingToNodeExceptionFunction = (route, router, e, state) => { return Task.FromResult<object>(state); };
        public void OnRoutingToNodeException(Func<IRoute, IRouter, Exception, object, Task<object>> onRoutingToNodeException)
        {
            this.onRoutingToNodeExceptionFunction = onRoutingToNodeException;
        }

        private Func<IRoute, IRoute, IList<IRoute>, object, Task<object>> onBranchingFunction = (parentRoute, route, siblings, state) => { return Task.FromResult<object>(state); };
        public void OnBranching(Func<IRoute, IRoute, IList<IRoute>, object, Task<object>> onBranchingFunction)
        {
            this.onBranchingFunction = onBranchingFunction;
        }


    }
}