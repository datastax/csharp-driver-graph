//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using Gremlin.Net.Structure.IO.GraphSON;

namespace Dse.Graph.Serialization
{
    internal interface ITypeSerializer: IGraphSONSerializer, IGraphSONDeserializer
    {
        /// <summary>
        /// Gets the full TinkerPop name, ie: g:UUID
        /// </summary>
        string FullTypeName { get; }
        
        /// <summary>
        /// Gets the type that it's handled by this serializer.
        /// </summary>
        Type Type { get; }
    }
}