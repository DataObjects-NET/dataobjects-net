# DataObjects.Net

DataObjects.Net is a persistence and object-relational mapping framework for the Microsoft .NET. It allows developers to define persistent objects as well as business logic directly in C#, Visual Basic or F#. The persistent objects can be retrieved by LINQ queries. Persistent data can be stored in SQL Servers. In contrast to many other ORM frameworks the database model is generated and maintained automatically.

Supported databases:
- MS SQL Server 2008 R2, 2012, 2014, 2016, 2017, 2019, 2022
- MS Azure SQL Database
- Oracle 10g, 11g
- PostgreSQL 8.3, 8.4, 9.x, 10, 11, 12, 13, 14, 15
- MySQL 5.6, 5.7, 8.0
- Firebird 2.5, 4.0
- Sqlite 3

Providers for the databases are available as separate packages and may be installed following way

```csharp
    dotnet add package Xtensive.Orm.SqlServer
    dotnet add package Xtensive.Orm.Oracle
    dotnet add package Xtensive.Orm.PostgreSQL
    dotnet add package Xtensive.Orm.MySql
    dotnet add package Xtensive.Orm.Firebird
    dotnet add package Xtensive.Orm.Sqlite
```

### Usage

The following  code demonstrates basic usage of DataObjects.Net. For full tutorial configuring Domain, defining the model and querying data see our [documentation](http://help.dataobjects.net).

Create a domain configuration configuration to connect to certain database

```csharp
    // create configuration with connection to Tests database on local instance of MS SQL Server
    var domainConfiguration = new DomainConfiguration(@"sqlserver://localhost/Tests");
	
    // register types form certain namespace which contains domain model persistent types -
	// the types derrived from Xtensive.Orm.Entity and/or Xtensive.Orm.Structure
    domainConfiguration.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
	
    // create database structure from scratch
    domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
```
	
Build domain by the configuration created before. Usually, domain is built when the application starts and disposed just before the application shuts down.

```csharp
    var domain = Domain.Build(domainConfiguration);
```

Query data from database, modify results and/or create new entites

```csharp
    // open a session to database
    using (var session = domain.OpenSession()) {
	  // and transaction
      using (var transactionScope = session.OpenTransaction()) {
        // query for existing Anton Chekhov
        Author existingAuthor = session.Query.All<Author>()
          .Where(author => author.FirstName=="Anton" && author.LastName=="Chekhov")
          .FirstOrDefault();

        //if Anton Pavlovich isn't in database yet then and him
        if (existingAuthor==null) {
          existingAuthor = new new Author(session) {
            FirstName = "Anton",
            LastName = "Chekhov";
          }
        }

        // add new book and assign it with Anton Chekhov
        existingAuthor.Books.Add(new Book(session) {Title = "The Cherry Orchard"});

        // commit opened transaction to save changes made within it
        transactionScope.Complete();
      }
    }
```

Dispose domain on application shut down

```csharp
    domain.Dispose();
```