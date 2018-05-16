# Batch Support

The `Dse.Graph` package supports batching multiple graph updates into a single transaction. All mutations included
in a batch will be applied if the execution completes successfully or none of them if any of the operations fails.

Use the `DseGraph.Batch()` method to create a `ITraversalBatch` instance.

```c#
ITraversalBatch batch = DseGraph.Batch();
```

You can add traversals to your batch instance using `Add()` method.

```c#
batch
    .Add(g.AddV("person").Property("name", "Matt").Property("age", 12));
    .Add(g.AddV("person").Property("name", "Olivia").Property("age", 8));
```

Once you've added all the mutations to the batch, you can use `ExecuteGraph(ITraversalBatch)` or
`ExecuteGraphAsync(ITraversalBatch)` extension methods of the `IDseSession` defined in this package.

```c#
GraphResultSet result = session.ExecuteGraph(batch);
```

## Batch options

You can specify batch options like consistency level, timeout and other settings when creating the batch instance.

```c#
var options = new GraphOptions().SetWriteConsistencyLevel(ConsistencyLevel.LocalQuorum);
var batch = DseGraph.Batch(options);
```

These options are going to be used when creating a `GraphStatement` internally and executing the batch.

Note that options defined at `GraphTraversalSource` level are going to be ignored for batch executions. 

## Complete code sample

```c#
using Dse;
using Dse.Graph;
using Gremlin.Net;
using Gremlin.Net.Process.Traversal;

namespace Dse.Graph.Samples
{
    public static class SampleBatchExecution
    {
        public static void ExecuteBatchSample(IDseSession session)
        {
            var g = DseGraph.Traversal(session);

            // Create a batch with options
            var batch = DseGraph.Batch(new GraphOptions().SetWriteConsistencyLevel(ConsistencyLevel.LocalQuorum));

            // Create 2 vertices and an edge connecting one to the other
            batch
                .Add(g.AddV("person").Property("name", "Matt").Property("age", 12))
                .Add(g.AddV("person").Property("name", "Olivia").Property("age", 8))
                .Add(g.V().Has("name", "Matt").AddE("knows").To(__.V().Has("name", "Olivia")));

            // Execute the batch using ExecuteGraph(ITraversalBatch) extension method
            session.ExecuteGraph(batch);
        }
    }
}
```