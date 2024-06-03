// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ListOperationState.cs

namespace Terradue.Stars.Console.Operations
{
    internal class ListOperationState
    {
        public ListOperationState(string prefix, int depth)
        {
            Prefix = prefix;
            Depth = depth;
        }

        public string Prefix { get; internal set; }
        public int Depth { get; internal set; }
    }
}
