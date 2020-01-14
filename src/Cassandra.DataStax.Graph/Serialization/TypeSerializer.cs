//
//      Copyright (C) DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Collections.Generic;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal class TypeSerializer<T> : ITypeSerializer
    {
        private readonly Func<T, object> _writeHandler = TypeSerializer<T>.DefaultWriteHandler;
        private readonly string _prefix;
        private readonly string _typeName;
        private readonly Func<JToken, T> _readHandler;

        public string FullTypeName => _prefix + ":" + _typeName;

        public Type Type => typeof(T);

        private static object DefaultWriteHandler(T value)
        {
            return value.ToString();
        }

        public TypeSerializer(string prefix, string typeName, Func<string, T> readHandler) : this(prefix, typeName)
        {
            _readHandler = token => readHandler(token.ToString());
        }

        protected TypeSerializer(string prefix, string typeName, Func<JToken, T> readHandler,
                                 Func<T, object> writeHandler) : this(prefix, typeName)
        {
            _readHandler = readHandler;
            _writeHandler = writeHandler ?? TypeSerializer<T>.DefaultWriteHandler;
        }

        private TypeSerializer(string prefix, string typeName)
        {
            _prefix = prefix;
            _typeName = typeName;
        }
        
        public Dictionary<string, dynamic> Dictify(dynamic objectData, GraphSONWriter writer)
        {
            return GraphSONUtil.ToTypedValue(_typeName, _writeHandler(objectData), _prefix);
        }

        public dynamic Objectify(JToken graphsonObject, GraphSONReader reader)
        {
            return _readHandler(graphsonObject);
        }
    }
}