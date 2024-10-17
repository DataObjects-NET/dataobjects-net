Xtensive.Orm.BulkOperations
===========================

Summary
-------
The extension provides a set of IQueryable extension methods that are translated 
to server-side UPDATE or DELETE commands.

Prerequisites
-------------
DataObjects.Net 7.1.x (http://dataobjects.net)


Examples of usage
-----------------

**Example #1**. Update primitive property with a constant value:

```csharp
  session.Query.All<Bar>()
    .Where(a => a.Id == 1)
    .Set(a => a.Count, 2)
    Update();
```

**Example #2** Updating persistent property with expression, computed on server:

```csharp
  session.Query.All<Bar>()
    .Where(a => a.Id==1)
    .Set(a => a.Count, a => a.Description.Length)
    .Update();
```

**Example #3**. Setting a reference to an entity that is already loaded into current Session

```csharp
  // Emulating entity loading
  var bar = session.Query.Single<Bar>(1);

  session.Query.All<Foo>()
    .Where(a => a.Id == 2)
    .Set(a => a.Bar, bar)
    .Update();
```

**Example #4**. Setting a reference to an entity that is not loaded into Session, 1st way

```csharp
  session.Query.All<Foo>()
    .Where(a => a.Id == 1)
    .Set(a => a.Bar, a => Query.Single<Bar>(1))
    .Update();
```

**Example #5**. Setting a reference to an entity that is not loaded into Session, 2nd way

```csharp
  session.Query.All<Foo>()
    .Where(a => a.Id == 1)
    .Set(a => a.Bar, a => Query.All<Bar>().Single(b => b.Name == "test"))
    .Update();
```

**Example #6**. Constructing update expressions of the fly

```csharp
  bool condition = CheckCondition();
  var query = session.Query.All()<Bar>
    .Where(a => a.Id == 1)
    .Set(a => a.Count, 2);

  if(condition)
    query = query.Set(a => a.Name, a => a.Name + "test");
  query.Update();
```

**Example #7**. Updating lots of properties at once

```csharp
  session.Query.All<Bar>()
    .Where(a => a.Id == 1)
    Update(
      a => new Bar(null) { Count = 2, Name = a.Name + "test", /*dozens of other properties...*/ });
```

**Example #8**. Deleting entities

```csharp
  session.Query.All<Foo>()
    .Where(a => a.Id == 1)
    .Delete();
```
