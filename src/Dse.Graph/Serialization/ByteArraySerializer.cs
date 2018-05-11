//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using Newtonsoft.Json.Linq;

namespace Dse.Graph.Serialization
{
    internal class ByteArraySerializer : TypeSerializer<byte[]>
    {
        public ByteArraySerializer() : base("dse", "Blob", Read, Write)
        {
        }

        private static object Write(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        private static byte[] Read(JToken token)
        {
            return Convert.FromBase64String(token.ToString());
        }
    }
}