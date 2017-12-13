# Getting started

The `Dse.Graph` package leverages the features of [Gremlin.Net language variant][glv] and the high-level client driver
features of the [DataStax Enterprise C# Driver][dse-driver].

```c#
using Dse;
using Dse.Graph;
using Gremlin.Net;
```

To start building traversals, you will need a `IDseSession` instance that represents a pool of connections to your
DSE cluster.
 
```c#
IDseCluster cluster = DseCluster.Builder()
                                .AddContactPoint("127.0.0.1")
                                .Build();
IDseSession session = cluster.Connect();
```

`IDseSession` instances of the [DSE driver][dse-driver] are designed to be long-lived and you should normally
reuse it during your application lifetime.

You can use your `IDseSession` instances to obtain `GraphTraversalSource` instances.

```c#
GraphTraversalSource g = DseGraph.Traversal(session);
```

A `GraphTraversalSource` from Gremlin.Net can be used (and reused) to get traversals.

```c#
var traversal = g.V().HasLabel("person");
```

## Traversal Execution

### Explicit execution

Traversals can be executed like regular `GraphStatement` instaces using `IDseSession.ExecuteGraph()` and
`IDseSession.ExecuteGraphAsync()` methods.

The returned types from this execution would be the ones from the DataStax Enterprise C# Driver, in the `Dse.Graph`
namespace.

```c#
var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person"));
GraphResultSet result = session.ExecuteGraph(statement);
```

You can benefit from the extension method on the `Dse.Graph` namespace to call `ExecuteGraph()` using the traversal,
without the need to manually convert it:

```c#
GraphResultSet result = session.ExecuteGraph(g.V().HasLabel("person"));
```

`GraphResultSet` is an `IEnumerable<IGraphNode>` implementation. `IGraphNode` represents a response item returned by the server. Each item can be converted to the expected type, for example: `node.To<IVertex>()`. You can also apply a conversion to the expected type to all the sequence by using `GraphResultSet.To<T>()` method:

```c#
foreach (IVertex vertex in result.To<IVertex>())
{
    Console.WriteLine(vertex.Label);
}
```

### Implicit execution

Traversals can be executed on the server using the methods that represents [Gremlin terminal steps][gremlin-terminal].
In the case of Gremlin.Net variant, those are `ToList()`, `ToSet()`, `Next()`, `NextTraverser()` and `Iterate()`, 
along with `Promise()` for async traversal execution.

The types returned from this type of execution will be `Gremlin.Net` types.

```c#
// An IList<Gremlin.Net.Structure.Vertex> instance
IList<Vertex> people = g.V().HasLabel("person").ToList();
```

## Enums, Static Methods and the Anonymous Traversal

Gremlin has various tokens (ie: `T`, `P`, `Order`, ...) that are represented in Gremlin.Net as
classes and [enums][enum].

```c#
g.V().HasLabel("person").Has("age", P.Gt(36))
```

The statements can be further simplified with the [`using static` directive in C# 6][using-static].

```c#
using static Gremlin.Net.Process.Traversal.P;
```

Then it is possible to represent the above traversal as below.

```c#
// Gt is declared in the P class
g.V().HasLabel("person").Has("age", Gt(36))
```

Finally, the anonymous traversal is exposed in the class `__` that can be statically imported, allowing 
to be expressed as below:

```c#
using static Gremlin.Net.Process.Traversal.__;
```

```c#
// Out is declared in the __ class
g.V().Repeat(Out()).Times(2).Values<string>("name").Fold()
```

## Execution options

[As explained in the C# DSE driver docs][graph-options], the graph options can be defined when initializing the
cluster, making them the defaults for all graph executions.

```c#
var dseCluster = DseCluster.Builder()
    .AddContactPoint("127.0.0.1")
    .WithGraphOptions(new GraphOptions().SetName("demo"))
    .Build();
```

In the previous example, the graph with the name "demo" will be used for all executions.

Additionally, you can define the graph options when obtaining the `GraphTraversalSource`.

```c#
var g = DseGraph.Traversal(session, new GraphOptions().SetName("demo"));
```

That way all traversals created from the `GraphTraversalSource` instance will be using those options.

[glv]: http://tinkerpop.apache.org/docs/3.2.7-SNAPSHOT/reference/#gremlin-DotNet
[gremlin-terminal]: http://tinkerpop.apache.org/docs/current/reference/#terminal-steps
[dse-driver]: https://github.com/datastax/csharp-dse-driver
[enum]: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/enum
[using-static]: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-static
[graph-options]: http://docs.datastax.com/en/developer/csharp-driver-dse/latest/features/graph-support/#graph-options
