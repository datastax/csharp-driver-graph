using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dse.Graph.Test.Unit
{
    [TestFixture]
    public class TraversalUnitTest
    {
        [Test]
        public void Json_Tests()
        {
            dynamic result = JObject.Parse("{\"prop1\": \"val\", \"prop2\": 2, \"@type\": \"type1\"}");
            string prop1 = result.prop1;
            int prop2 = result.prop2;
            string type = result["@type"];
            Assert.AreEqual(prop1, "val");
            Assert.AreEqual(prop2, 2);
            Assert.AreEqual(type, "type1");
        }
    }
}
