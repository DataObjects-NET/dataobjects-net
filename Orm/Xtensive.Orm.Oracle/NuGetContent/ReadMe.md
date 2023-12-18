# Oracle provider for DataObjects.Net

The provider is responsible for interactions with Oracle database - information about features and types the storage supports, low level communications, translation of SqlDom queries to native SQL text queries.

For now it supports Oracle 10g, 11g

### Usage

Create a domain configuration configuration with connection url similar to this

```csharp
    var domainConfiguration = new DomainConfiguration(@"oracle://someuser:somepassword@localhost:1521/xe");
```

or, alternatively, use connection string like

```csharp
    var domainConfiguration = new DomainConfiguration("oracle",
	  "DATA SOURCE="(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xe)))";USER ID=someuser;PASSWORD=somepassword");
```

After that, if connection settings are valid, build Domain and use it as usual.