//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Dse.Graph.Serialization
{
    internal class TypeSerializer<T> : ITypeSerializer
    {
        private readonly Func<T, object> _writeHandler = DefaultWriteHandler;
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
            _writeHandler = writeHandler ?? DefaultWriteHandler;
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