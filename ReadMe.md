# DataObjects.Net 

DataObjects.Net is a persistence and object-relational mapping framework for the Microsoft .NET. It allows developers to define persistent objects as well as business logic directly in C#, Visual Basic or F#. The persistent objects can be retrieved by LINQ queries. Persistent data can be stored in SQL Servers. In contrast to many other ORM frameworks the database model is generated and maintained automatically.

Supported databases:
- MS SQL Server 2008 R2, 2012, 2014, 2016, 2017, 2019
- MS Azure SQL Database
- Oracle 10g, 11g
- PostgreSQL 8.3, 8.4, 9.0, 9.1, 9.2, 10, 11
- MySQL 5.5, 5.6
- Firebird 2.5
- Sqlite 3

### Installation

DataObjects.Net is available on Nuget. Install the provider package corresponding to your target database (main package already has MS SQL Server provider included)

    dotnet add package Xtensive.Orm.Core
    dotnet add package Xtensive.Orm.Oracle.Core
    dotnet add package Xtensive.Orm.PostgreSQL.Core
    dotnet add package Xtensive.Orm.MySql.Core
    dotnet add package Xtensive.Orm.Firebird.Core
    dotnet add package Xtensive.Orm.Sqlite


Use the --version option to specify preview version to install

### Usage 

The following  code demonstrates  basic usage of DataObjects.Net. For full tutorial configuring Domain, defining the model and querying data see our [documentation](https://duckduckgo.com).

    // create configuration with connection to Tests database on local instance of MS SQL Server
    var domainConfiguration = new DomainConfiguration(@"sqlserver://localhost/Tests");
    // register types from certain domain
    domainConfiguration.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
    // create database structure from scratch
    domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

    // on application start build Domain
    var domain = Domain.Build(domainConfiguration);

    // open a session to database
    using (var session = domain.OpenSession()) {
      using (var transactionScope = session.OpenTransaction()) {
        // query for existing Anton Chekhov
        Author existingAuthor = session.Query.All<Author>()
          .Where(author => author.FirstName=="Anton" && author.LastName=="Chekhov")
          .FirstOrDefault();

        //if Anton Pavlovish isn't in database yet then and him
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

    // on application shutdown dispose existing domain
    domain.Dispose()



### Getting support

If you have a specific question about using DataObjects.Net you can ask [on our support forum](http://support.x-tensive.com).


### Contributing

If you are intrested in congributing to this project, see [contributing](https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Contributing.md).