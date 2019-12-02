# Bulk Operations

The extension provides a set of `IQueriable` extension methods that are translated to server-side `UPDATE` or `DELETE` commands

### Installation

The extension is available on Nuget

    dotnet add package Xtensive.Orm.BulkOperations.Core

### Usage

Update primitive property with a constant value e.g:

    session.Query.All<Bar>()
      .Where(a => a.Id == 1)
      .Set(a => a.Count, 2)
    .Update();

Update persistent property with expression, computed on server, e.g:

    session.Query.All<Bar>()
      .Where(a => a.Id == 1)
      .Set(a => a.Count, a => a.Description.Length)

Set a reference to an entity that are already loaded into current `Session`:

    // Emulating entity loading
    var bar = session.Query.Single<Bar>(1);

    session.Query.All<Foo>()
      .Where(a => a.Id == 2)
      .Set(a => a.Bar, bar)
      .Update();

Set a reference to an entity that are not loaded ingo `Session`, 1st way:

    sessionQuery.All<Foo>()
      .Where(a => a.Id == 2)
      .Set(a => a.Bar, a => session.Query.Single<Bar>(1))
      .Update();

Set a reference to an entity that are not loaded ingo `Session`, 2nd way:

    session.Query.All<Foo>()
      .Where(a => a.Id == 2)
      .Set(a => a.Bar, a => session.Query.All<Bar>().Single(b => b.Name == "test"))
      .Update();

Consturuct update expressions on the fly:

    bool condition = CheckCondition();
    var query = session.Query.All()<Bar>
      .Where(a => a.Id == 1)
      .Set(a => a.Count, 2);

    if(condition)
      query = query.Set(a => a.Name, a => a.Name + "test");
    query.Update();

Update lots of properties at once:

      session.Query.All<Bar>()
        .Where(a => a.Id == 1)
        .Update(a => new Bar(null) {Count = 2, Name = a.Name + "test", dozens of other properties... });

Delete entities:

    Query.All<Foo>()
      .Where(a => a.Id == 1)
      .Delete();

