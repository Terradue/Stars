// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: RoutingException.cs

using System;
using System.Runtime.Serialization;

namespace Stars.Services.Exceptions
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
