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

DataObjects.Net is available on Nuget. Install main package (already has MS SQL Server provider included)

    dotnet add package Xtensive.Orm

Providers for Oracle, PostgreSQL, Mysql, Firebird and SQLite may be installed optionally if you need them

    dotnet add package Xtensive.Orm.Oracle
    dotnet add package Xtensive.Orm.PostgreSQL
    dotnet add package Xtensive.Orm.MySql
    dotnet add package Xtensive.Orm.Firebird
    dotnet add package Xtensive.Orm

DataObjects.Net extensions are available on Nuget as well (more about extensions [here](https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions.md))

    dotnet add package Xtensive.Orm.BulkOperations
    dotnet add package Xtensive.Orm.Localization
    dotnet add package Xtensive.Orm.Logging.log4net
    dotnet add package Xtensive.Orm.Reprocessing
    dotnet add package Xtensive.Orm.Security
    dotnet add package Xtensive.Orm.Tracking
    dotnet add package Xtensive.Orm.Web

Use the --version option to specify preview version to install

### Usage 

The following  code demonstrates  basic usage of DataObjects.Net. For full tutorial configuring Domain, defining the model and querying data see our [documentation](http://help.dataobjects.net).

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

    // on application shutdown dispose existing domain
    domain.Dispose()



### Getting support

If you have a specific question about using DataObjects.Net you can ask [on our support forum](http://support.x-tensive.com) or send your question to [support@dataobjects.net](mailto:support@dataobjects.net).

### How To Build

Repository contains one solution file - `Orm`

This solution contains `Weaver` project responsible for post-build processing of assemblies containing inheritors of `Xtensive.Orm.Persistent` class. `Weaver` project is first in build sequence. Other projects in the solution include DataObjects.Net itself as well as its extensions and tests.

In order to build project binaries one need to execute the `dotnet build` command in the solution folder. It will build everything in `Debug` configuration. In case `Release` binaries are needed just specify configuration parameter as following

    dotnet build -c Release

By defuault `Debug` configuration build doesn't generate Nuget packages but `Release` configuration build does. It is possible to change this default behavior by specifying `GeneratePackageOnBuild` parameter explicitly.
So in case Nuget packages aren't needed for release build consider to run 

    dotnet build -c Release /p:GeneratePackageOnBuild=false

alternatively the following command will generate packages for `Debug` build

    dotnet build /p:GeneratePackageOnBuild=true

Build results are available in the `_Build` subdirectory of solution folder.


Version.props file declares version is building. `<DoVersion>` tag defiles version in `<Major version>.<Minor version>.<Revision>` format. Do not define Build number, it is defined while building. `<DoVersionSuffix>` should be defined for pre-release versions like Alphas, Betas, and RCs.

### License

DataObjects.Net and its extensions published here are licensed under the [MIT](https://github.com/DataObjects-NET/dataobjects-net/blob/master/License.txt) license.