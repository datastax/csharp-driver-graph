# DataStax Enterprise C# Graph Extension

This package builds on the [DataStax Enterprise C# driver][dse-driver], adding functionality for interacting
with DSE graph features using [Apache TinkerPop][tinkerpop] [Gremlin.Net][gremlin-dotnet].

This library supports .NET Framework 4.6.1+ and .NET Core 1+.

The package can be used solely with [DataStax Enterprise][dse]. Please consult [the license](#license).

## Installation

[Get it on Nuget][nuget]

```
PM> Install-Package Dse.Graph
```

## Documentation

- [Documentation index][doc-index]
- [Getting started guide][getting-started]
- [API docs][doc-api]

## Basic Usage

### Import namespace:

```c#
using Dse.Graph;
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
Additionally, you can use the `#datastax-drivers` channel in the [DataStax Academy Slack][slack].

## License

Copyright 2017 DataStax

http://www.datastax.com/terms/datastax-dse-driver-license-terms

[dse]: http://www.datastax.com/products/datastax-enterprise
[dse-driver]: http://docs.datastax.com/en/developer/csharp-driver-dse/latest/
[nuget]: https://www.nuget.org/packages/Dse.Graph
[doc-index]: http://docs.datastax.com/en/developer/csharp-dse-graph/latest/
[doc-api]: http://docs.datastax.com/en/drivers/csharp-dse-graph/1.0/
[getting-started]: http://docs.datastax.com/en/developer/csharp-dse-graph/latest/getting-started/
[jira]: https://datastax-oss.atlassian.net/projects/CSHARP/issues
[mailing-list]: https://groups.google.com/a/lists.datastax.com/forum/#!forum/csharp-driver-user
[slack]: https://academy.datastax.com/slack
[tinkerpop]: http://tinkerpop.apache.org/
[gremlin-dotnet]: http://tinkerpop.apache.org/docs/3.2.7/reference/#gremlin-DotNet
