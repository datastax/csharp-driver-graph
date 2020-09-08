# DataStax C# Graph Extension

This package builds on the [DataStax C# Driver for Apache Cassandra][driver], adding a fluent API for interacting with DataStax graph features using [Apache TinkerPop][tinkerpop] [Gremlin.Net][gremlin-dotnet].

This library supports .NET Framework 4.6.1+ and .NET Core 2.1+.

The package should be used with [DataStax Enterprise][dse].

## Installation

[Get it on Nuget][nuget]

```
PM> Install-Package CassandraCSharpDriver.Graph
```

## Documentation

- [Documentation index][doc-index]
- [Getting started guide][getting-started]
- [API docs][doc-api]

## Basic Usage

### Import namespace:

```c#
using Cassandra.DataStax.Graph;
```

### Get the traversal source using a IDseSession instance

```c#
GraphTraversalSource g = DseGraph.Traversal(session);
```

You can reuse your `GraphTraversalSource` instance across your application.

### Get a list of vertices

```c#
IList<Vertex> people = g.V().HasLabel("person").ToList();
```

Visit the [Getting Started Guide][getting-started] for more examples.

## Getting Help

You can use the project [Mailing list][mailing-list] or create a ticket on the [Jira issue tracker][jira].

## License

© DataStax, Inc.

Licensed under the Apache License, Version 2.0 (the “License”); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an “AS IS” BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

[dse]: http://www.datastax.com/products/datastax-enterprise
[driver]: http://docs.datastax.com/en/developer/csharp-driver/latest/
[nuget]: https://www.nuget.org/packages/CassandraCSharpDriver.Graph
[doc-index]: http://docs.datastax.com/en/developer/csharp-dse-graph/latest/
[doc-api]: http://docs.datastax.com/en/drivers/csharp-dse-graph/1.2/
[getting-started]: http://docs.datastax.com/en/developer/csharp-dse-graph/latest/getting-started/
[jira]: https://datastax-oss.atlassian.net/projects/CSHARP/issues
[mailing-list]: https://groups.google.com/a/lists.datastax.com/forum/#!forum/csharp-driver-user
[tinkerpop]: http://tinkerpop.apache.org/
[gremlin-dotnet]: http://tinkerpop.apache.org/docs/3.2.9/reference/#gremlin-DotNet
