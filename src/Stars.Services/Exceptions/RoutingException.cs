using System;
using System.Runtime.Serialization;

namespace Terradue.Stars.Services.Exceptions
{
    [Serializable]
    internal class RoutingException : Exception
    {
        public RoutingException()
        {
        }

        public RoutingException(string message) : base(message)
        {
        }

        public RoutingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RoutingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}