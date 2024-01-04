# MySQL provider for DataObjects.Net

The provider is responsible for interactions with MySQL database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports MySQL 5.6, 5.7, 8.0.

### Usage

Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"mysql://someuser:somepassword@localhost:3306/tests");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("mysql",
	  "Server=localhost;Port=3306;Database=tests;Uid=someuser;Pwd=somepassword;");
```

After that, if connection settings are valid, build Domain and use it as usual.