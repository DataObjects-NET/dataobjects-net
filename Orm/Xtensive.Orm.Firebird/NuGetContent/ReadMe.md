# Firebird provider for DataObjects.Net

The provider is responsible for interactions with Firebird database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports Firebird 2.5, 4.0.

### Usage


Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"firebird://someuser:somepassword@localhost:3050/tests");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("firebird",
	  "User=someuser;Password=somepassword;Database=tests;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0");
```

After that, if connection settings are valid, build Domain and use it as usual.