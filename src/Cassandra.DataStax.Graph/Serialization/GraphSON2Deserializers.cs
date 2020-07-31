//
//       Copyright (C) DataStax Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System.Collections.Generic;
using Cassandra.DataStax.Graph.Internal;
using Cassandra.DataStax.Graph.Serialization.Tinkerpop.Structure.IO.GraphSON;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal static class GraphSON2Deserializers
    {
        /// <summary>
        /// Contains the <see cref="IGraphSONDeserializer" /> instances by their type identifier. 
        /// </summary>
        public static readonly Dictionary<string, IGraphSONDeserializer> CustomDeserializers = new Dictionary
            <string, IGraphSONDeserializer>
            {
                {"g:Traverser", new TraverserReader()},
                {"g:Direction", new DirectionDeserializer()},
                {"g:Vertex", new VertexDeserializer()},
                {"g:Edge", new EdgeDeserializer()},
                {"g:Property", new PropertyDeserializer()},
                {"g:VertexProperty", new VertexPropertyDeserializer()},
                {"g:Path", new PathDeserializer()},
                {"g:T", new TDeserializer()}
            };
    }
}