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

using System;
using System.Collections.Generic;
using Cassandra.DataStax.Graph.Internal;
using Cassandra.DataStax.Graph.Serialization.Tinkerpop.Structure.IO.GraphSON;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal static class GraphSON2Serializers
    {
        /// <summary>
        /// Contains the information of serializers by type.
        /// </summary>
        public static readonly IReadOnlyDictionary<Type, IGraphSONSerializer> CustomSerializers = new Dictionary
            <Type, IGraphSONSerializer>
            {
                {typeof(Gremlin.Net.Process.Traversal.ITraversal), new TraversalSerializer()},
                {typeof(Gremlin.Net.Process.Traversal.Bytecode), new BytecodeSerializer()},
                {typeof(Gremlin.Net.Process.Traversal.Binding), new BindingSerializer()},
                {typeof(Gremlin.Net.Driver.Messages.RequestMessage), new RequestMessageSerializer()},
                {typeof(Gremlin.Net.Process.Traversal.EnumWrapper), new EnumSerializer()},
                {typeof(Gremlin.Net.Process.Traversal.P), new PSerializer()},
                {typeof(Gremlin.Net.Process.Traversal.TextP), new TextPSerializer()},
                {typeof(Gremlin.Net.Structure.Vertex), new VertexSerializer()},
                {typeof(Gremlin.Net.Structure.Edge), new EdgeSerializer()},
                {typeof(Gremlin.Net.Structure.Property), new PropertySerializer()},
                {typeof(Gremlin.Net.Structure.VertexProperty), new VertexPropertySerializer()},
                {typeof(Gremlin.Net.Process.Traversal.Strategy.AbstractTraversalStrategy), new TraversalStrategySerializer()},
                {typeof(Gremlin.Net.Process.Traversal.ILambda), new LambdaSerializer()}
            };
    }
}