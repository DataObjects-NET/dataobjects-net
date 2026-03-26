# MS SQL Server provider for DataObjects.Net

The provider is responsible for interactions with MS SQL Server database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports MS SQL Server 2008 R2, 2012, 2014, 2016, 2017, 2019, 2022

### Usage

Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"sqlserver://someuser:somepassword@localhost/Tests?MultipleActiveResultSets=True");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("sqlserver",
	  "Data Source=localhost;Initial Catalog=Tests;User Id=someuser;Password=somepassword;MultipleActiveResultSets=True");
```

After that, if connection settings are valid, build Domain and use it as usual.