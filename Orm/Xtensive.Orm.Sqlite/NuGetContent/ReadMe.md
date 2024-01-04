# Sqlite provider for DataObjects.Net

The provider is responsible for interactions with Sqlite database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports Sqlite 3

### Usage

Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"sqlite3:///tests.db3");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("sqlite3",
	  "Data Source=tests.db3");
```

After that, if connection settings are valid, build Domain and use it as usual.