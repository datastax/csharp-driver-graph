//
//  Copyright (C) 2017 DataStax, Inc.
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
        private readonly string _prefix;
        private readonly string _typeName;
        private readonly Func<JToken, T> _parser;

        public string FullTypeName => _prefix + ":" + _typeName;

        public Type Type => typeof(T);

        public TypeSerializer(string prefix, string typeName, Func<string, T> parser)
        {
            _prefix = prefix;
            _typeName = typeName;
            _parser = token => parser(token.ToString());
        }
        
        public Dictionary<string, dynamic> Dictify(dynamic objectData, GraphSONWriter writer)
        {
            return GraphSONUtil.ToTypedValue(_typeName, objectData.ToString(), _prefix);
        }

        public dynamic Objectify(JToken graphsonObject, GraphSONReader reader)
        {
            return _parser(graphsonObject);
        }
    }
}