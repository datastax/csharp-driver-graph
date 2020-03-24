# Upgrading from the DSE Driver Graph Extension

This guide is intended for users of the DSE driver that plan to migrate to the [DataStax C# Driver for Apache Cassandra][driver], i.e., `CassandraCSharpDriver`.

Users of the `Dse.Graph` nuget package that are transitioning to the DataStax C# Driver for Apache Cassandra should now change their applications to use the `CassandraCSharpDriver.Graph` nuget package instead.

The main difference between `Dse.Graph` and `CassandraCSharpDriver.Graph` is that the `Dse` nuget package dependency was replaced with a dependency to the [`CassandraCSharpDriver` nuget package][driver-nuget], which is the package of the [DataStax C# Driver for Apache Cassandra][driver].

The API of this package hasn't changed but the namespace was changed from `Dse.Graph` to `Cassandra.DataStax.Graph` and the nuget package name was changed from `Dse.Graph` to `CassandraCSharpDriver.Graph`.

With these changes we bumped the major version so `CassandraCSharpDriver.Graph` will start at version `2.0.0` while `Dse.Graph` will stay at `1.x` and will no longer be maintained.

[driver]: http://docs.datastax.com/en/developer/csharp-driver/latest/
[driver-nuget]: https://www.nuget.org/packages/CassandraCSharpDriver
