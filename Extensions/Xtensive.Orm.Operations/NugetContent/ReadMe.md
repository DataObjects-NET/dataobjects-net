Xtensive.Orm.Operations
=====================

Summary
-------
Provides ability to record chain of operations on persistent types and reply them later

Prerequisites
-------------
DataObjects.Net 7.3.x or later (http://dataobjects.net)

Implementation
--------------

1. Include types from Xtensive.Orm.Operations assembly into the domain:

in xml configuration
```xml
  <Xtensive.Orm>
    <domains>
      <domain ... >
        <types>
          <add assembly="your assembly"/>
          <add assembly="Xtensive.Orm.Operations"/>
        </types>
      </domain>
    </domains>
  </Xtensive.Orm>
```

or in code

```csharp
var domainConfiguration = new DomainConfiguration("sqlserver://dotest:dotest@localhost/DO-Tests?MultipleActiveResultSets=True");

domainConfiguration.Types.Register(typeof(SomeTypeFromYourAssembly).Assembly);
domainConfiguration.Types.Register(typeof(Xtensive.Orm.Operations.Operation).Assembly);

```


2. To start logging changes, instanciate an ```OperationLog``` and declare region of capture by attaching to session by using ```OperationCapturer.Attach(Session, OperationLog)```
3. Retrieve registered operations from the ```OperationLog``` instance you created and passed into ```OperationCapturer```.


Demo
----

First, create a log

```csharp
var operationLog = new Xtensive.Orm.Operations.OperationLog(OperationLogType.SystemOperationLog);
```

Then, open session and attach capturer

```csharp
var operationLog = new Xtensive.Orm.Operations.OperationLog(OperationLogType.SystemOperationLog);

using (var session = Domain.OpenSession()) 
using (var t = session.OpenTransaction())
  using (var capturer = Xtensive.Orm.Operations.OperationCapturer.Attach(session, operationLog)) {
    var author1 = new Author(session, "Unknown", "Author"); //generate key operation + create entity operation
    author1.DateOfBirth = new DateTime(1985, 6, 12); // set entity field operation

    var book11 = new Book(session);
    book11.Title = "100 pages about something";
    book11.NumberOfPages = 100;
    book11.Author = author1; // only set entity field operation, no operation for paired EntitySet

    var book12 = new Book(session);
    book12.Title = "Even more pages abount something";
    book12.NumberOfPages = 200;
    book12.Author = author1;

    var book13 = new Book(session);
    book13.Title = "The biggest book ever";
    book13.NumberOfPages = 300;
    book13.Author = author1;

    var author2 = new Author(session, "Even Less known", "Author");
    author2.DateOfBirth = new DateTime(1985, 6, 12);

    var book21 = new Book(session) { Title = "100 ways to not live", NumberOfPages = 111 };
    _ = author2.Books.Add(book21); // entity set item add operation

    var book22 = new Book(session) { Title = "50 ways of eating pizza", NumberOfPages = 25 };
    _ = author2.Books.Add(book22);
    var book23 = new Book(session) { Title = "Single way to eat banana", NumberOfPages = 3 };
    _ = author2.Books.Add(book23);

    _ = author1.Books.Remove(book12); // entity set item remove operation
    book12.Remove(); entity remove operation 

    var book23 = author2.Books.AsEnumerable().Last();
    _ = author2.Books.Remove(book23);
    t.Complete();
  }
}

// do something with logged operations, if needed
foreach (var op in operationLog) {
  // do something
}

```

OperationLog can also replay collected operations, but since they will replay exactly, conflicts may occur if created by replay Entites already exist in the database.
Based on the previous example, replay example may look like

```csharp
var operationLog = new Xtensive.Orm.Operations.OperationLog(OperationLogType.SystemOperationLog);

using (var session = Domain.OpenSession()) 
using (var t = session.OpenTransaction())
  using (var capturer = Xtensive.Orm.Operations.OperationCapturer.Attach(session, operationLog)) {
    var author1 = new Author(session, "Unknown", "Author"); //generate key operation + create entity operation
    author1.DateOfBirth = new DateTime(1985, 6, 12); // set entity field operation

    var book11 = new Book(session);
    book11.Title = "100 pages about something";
    book11.NumberOfPages = 100;
    book11.Author = author1; // only set entity field operation, no operation for paired EntitySet

    var book12 = new Book(session);
    book12.Title = "Even more pages abount something";
    book12.NumberOfPages = 200;
    book12.Author = author1;

    var book13 = new Book(session);
    book13.Title = "The biggest book ever";
    book13.NumberOfPages = 300;
    book13.Author = author1;

    var author2 = new Author(session, "Even Less known", "Author");
    author2.DateOfBirth = new DateTime(1985, 6, 12);

    var book21 = new Book(session) { Title = "100 ways to not live", NumberOfPages = 111 };
    _ = author2.Books.Add(book21); // entity set item add operation

    var book22 = new Book(session) { Title = "50 ways of eating pizza", NumberOfPages = 25 };
    _ = author2.Books.Add(book22);
    var book23 = new Book(session) { Title = "Single way to eat banana", NumberOfPages = 3 };
    _ = author2.Books.Add(book23);

    _ = author1.Books.Remove(book12); // entity set item remove operation
    book12.Remove(); entity remove operation 

    var book23 = author2.Books.AsEnumerable().Last();
    _ = author2.Books.Remove(book23);

    //t.Complete(); // no complete means rollback
  }
}

using (var sessionForReplay = Domain.OpenSession())
using (var tx = sessionForReplay.OpenTransaction()) {

  // replay
  var keys = operationLog.Replay(sessionForReplay);

  tx.Complete();
}

```

If there is a need to disable registration for a while, it may be done like so

```csharp
using (var session = Domain.OpenSession())
using (var tx = session.OpenTransaction()) {

  using (var disableScope session.Operations.DisableSystemOperationRegistration()) {
    // changes done here will not be registered
  }

  tx.Complete();
}

```