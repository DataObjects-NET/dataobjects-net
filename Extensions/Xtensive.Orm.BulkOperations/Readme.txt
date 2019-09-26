===========================
Xtensive.Orm.BulkOperations
===========================

Summary
-------
The extension provides a set of IQueryable extension methods that are translated 
to server-side UPDATE or DELETE commands.

Prerequisites
-------------
DataObjects.Net Core 0.1 or later (http://dataobjects.net)

Implementation
-------------
1. Add reference to Xtensive.Orm.BulkOperations assembly

Demo
----
1. Update primitive property with a constant value, e.g.:

Query.All<Bar>()
  .Where(a => a.Id == 1)
  .Set(a => a.Count, 2)
  .Update();

2. Updating persistent property with expression, computed on server, e.g.:

Query.All<Bar>()
  .Where(a => a.Id==1)
  .Set(a => a.Count, a => a.Description.Length)
  .Update();

3. Setting a reference to an entity that is already loaded into current Session

// Emulating entity loading
var bar = Query.Single<Bar>(1);

Query.All<Foo>()
  .Where(a => a.Id == 2)
  .Set(a => a.Bar, bar)
  .Update();

4. Setting a reference to an entity that is not loaded into Session, 1st way

Query.All<Foo>()
  .Where(a => a.Id == 1)
  .Set(a => a.Bar, a => Query.Single<Bar>(1))
  .Update();

5. Setting a reference to an entity that is not loaded into Session, 2nd way

Query.All<Foo>()
  .Where(a => a.Id == 1)
  .Set(a => a.Bar, a => Query.All<Bar>().Single(b => b.Name == "test"))
  .Update();

6. Constructing update expressions of the fly

bool condition = CheckCondition();
var query = Query.All()<Bar>
  .Where(a => a.Id == 1)
  .Set(a => a.Count, 2);

if(condition)
  query = query.Set(a => a.Name, a => a.Name + "test");
query.Update();

7. Updating lots of properties at once

Query.All<Bar>()
  .Where(a => a.Id == 1)
  .Update(a => new Bar(null) { Count = 2, Name = a.Name + "test", dozens of other properties... });

8. Deleting entities

Query.All<Foo>()
  .Where(a => a.Id == 1)
  .Delete();

References
----------
http://doextensions.codeplex.com