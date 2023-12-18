# PostgreSQL provider for DataObjects.Net

The provider is responsible for interactions with PostgreSQL database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports PostgreSQL 8.3, 8.4, 9.x, 10, 11, 12, 13, 14, 15

### Usage

Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"postgresql://someuser:somepassword@localhost:5432/tests");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("postgresql",
	  "HOST=DODB;PORT=5432;DATABASE=tests;USER ID=someuser;PASSWORD=somepassword");
```

After that, if connection settings are valid, build Domain and use it as usual.