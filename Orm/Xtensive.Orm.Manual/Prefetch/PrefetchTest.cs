// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.12.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Prefetch
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    public DateTime BirthDay { get; set; }

    [Field(LazyLoad = true, Length = 65536)]
    public byte[] Photo { get; set; }

    [Field]
    public Person Manager { get; set; }

    [Field]
    [Association(PairTo = "Manager")]
    public EntitySet<Person> Employees { get; private set; }

    public Key ManagerKey => GetReferenceKey(TypeInfo.Fields["Manager"]);

    public Person(Session session)
      : base(session)
    { }
  }

  #endregion

  [TestFixture]
  public class PrefetchTest : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return config;
    }

    [TearDown]
    public void ClearContent()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var people = session.Query.All<Person>().ToList();
        foreach (var person in people) {
          person.Manager = null;
        }
        session.SaveChanges();

        foreach (var person in people) {
          person.Remove();
        }
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.All<Person>()
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var personIds = session.Query.All<Person>().Select(p => p.Id);
        var prefetchedPeople = session.Query.Many<Person, int>(personIds)
          .Prefetch(p => new { p.Photo, p.Manager }) // Lazy load field and Referenced entity
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => new { e.Photo, e.Manager })); // and lazy load field and referenced entity of each of its items
        foreach (var person in prefetchedPeople) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.All<Person>()
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        foreach (var person in people) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.All<Person>()
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager) // Referenced entity
          .ExecuteAsync();
        foreach (var person in await people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var personIds = session.Query.All<Person>().Select(p => p.Id);
        var prefetchedPeople = session.Query.Many<Person, int>(personIds)
          .Prefetch(p => new { p.Photo, p.Manager }) // Lazy load field and Referenced entity
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => new { e.Photo, e.Manager })) // and lazy load field and referenced entity of each of its items
          .ExecuteAsync();
        foreach (var person in await prefetchedPeople) {
          // some code here...
        }
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.All<Person>()
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        await foreach (var person in people.AsAsyncEnumerable()) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public void MultipleBatchesTest()
    {
      var count = 1000;

      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var people = new Person[count];
        for (var i = 0; i < count; i++) {
          people[i] = new Person(session) { Name = i.ToString(), Photo = new[] { (byte) (i % 256) } };
        }
        session.SaveChanges();

        var random = new Random(10);
        for (var i = 0; i < count; i++) {
          var person = people[i];
          if (random.Next(5) > 0) {
            person.Manager = people[random.Next(count)];
          }
        }
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var prefetchedPeople = (
          from person in session.Query.All<Person>()
          orderby person.Name
          select person)
          .Take(100)
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in prefetchedPeople) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.IsTrue(DirectStateAccessor.Get(person.Employees).IsFullyLoaded);
          foreach (var employee in person.Employees) {
            Assert.That(DirectStateAccessor.Get(employee).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public async Task MultipleBatchesAsyncTest()
    {
      var count = 1000;

      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var people = new Person[count];
        for (var i = 0; i < count; i++) {
          people[i] = new Person(session) { Name = i.ToString(), Photo = new[] { (byte) (i % 256) } };
        }
        session.SaveChanges();

        var random = new Random(10);
        for (var i = 0; i < count; i++) {
          var person = people[i];
          if (random.Next(5) > 0) {
            person.Manager = people[random.Next(count)];
          }
        }
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var prefetchedPeople = (
          from person in session.Query.All<Person>()
          orderby person.Name
          select person)
          .Take(100)
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        await foreach (var person in prefetchedPeople.AsAsyncEnumerable()) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.IsTrue(DirectStateAccessor.Get(person.Employees).IsFullyLoaded);
          foreach (var employee in person.Employees) {
            Assert.That(DirectStateAccessor.Get(employee).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public void DelayedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        foreach (var person in people) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public async Task DelayedQueryAsyncTest()
    {
      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        await foreach (var person in people.AsAsyncEnumerable()) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    private class QueryCounterSessionHandlerMoq : ChainingSessionHandler
    {
      private volatile int syncCounter;
      private volatile int asyncCounter;

      public int GetSyncCounter() => syncCounter;

      public int GetAsyncCounter() => asyncCounter;

      public QueryCounterSessionHandlerMoq(SessionHandler chainedHandler) : base(chainedHandler)
      {
      }

      public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
      {
        _ = Interlocked.Increment(ref syncCounter);
        base.ExecuteQueryTasks(queryTasks, allowPartialExecution);
      }

      public override Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution, CancellationToken token)
      {
        _ = Interlocked.Increment(ref asyncCounter);
        return base.ExecuteQueryTasksAsync(queryTasks, allowPartialExecution, token);
      }
    }

    [Test]
    public async Task DelayedQueryAsyncShouldMaterializeAsyncTest()
    {
      await using (var session = await Domain.OpenSessionAsync()) {
        using (var transactionScope = session.OpenTransaction()) {
          var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
          var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
          _ = manager.Employees.Add(employee);
          transactionScope.Complete();
        }
      }

      await using (var session = await Domain.OpenSessionAsync()) // no session activation!
      {
        var sessionAccessor = new DirectSessionAccessor(session);
        var prop = typeof(Session).GetProperty("Handler", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
        var handler = prop.GetValue(session) as SessionHandler;
        var moq = new QueryCounterSessionHandlerMoq(handler);
        using (sessionAccessor.ChangeSessionHandler(moq))

        using (var transactionScope = session.OpenTransaction()) {
          var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
            .Prefetch(p => p.Photo) // Lazy load field
            .Prefetch(p => p.Employees // EntitySet Employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
            .Prefetch(p => p.Manager); // Referenced entity
          foreach (var person in people) {
            // some code here...
          }
          Assert.That(moq.GetSyncCounter(), Is.GreaterThan(0));
          Assert.That(moq.GetAsyncCounter(), Is.EqualTo(0));
          transactionScope.Complete();
        }
      }

      await using (var session = await Domain.OpenSessionAsync())// no session activation!
      {
        var sessionAccessor = new DirectSessionAccessor(session);
        var prop = typeof(Session).GetProperty("Handler", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
        var handler = prop.GetValue(session) as SessionHandler;
        var moq = new QueryCounterSessionHandlerMoq(handler);
        using (sessionAccessor.ChangeSessionHandler(moq))

        using (var transactionScope = session.OpenTransaction()) {
          var people = session.Query.CreateDelayedQuery(q => q.All<Person>())
            .Prefetch(p => p.Photo) // Lazy load field
            .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
            .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
          await foreach (var person in people.AsAsyncEnumerable()) {
            var accessor = DirectStateAccessor.Get(person);
            Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
            Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
            if (person.ManagerKey != null) {
              Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
              Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
            }
            // some code here...
          }
          Assert.That(moq.GetSyncCounter(), Is.EqualTo(0));
          Assert.That(moq.GetAsyncCounter(), Is.GreaterThan(0));
          transactionScope.Complete();
        }

      }
    }

    [Test]
    public void CachedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.Execute(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = session.Query.Execute(q => q.All<Person>())
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        foreach (var person in people) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public async Task CachedQueryAsyncTest()
    {
      await using (var session = await Domain.OpenSessionAsync())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person(session) { Name = "Employee", Photo = new byte[] { 8, 0 } };
        var manager = new Person(session) { Name = "Manager", Photo = new byte[] { 8, 0 } };
        _ = manager.Employees.Add(employee);
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = (await session.Query.ExecuteAsync(q => q.All<Person>()))
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees // EntitySet Employees
            .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in people) {
          // some code here...
        }
        transactionScope.Complete();
      }

      await using (var session = await Domain.OpenSessionAsync())// no session activation!
      using (var transactionScope = session.OpenTransaction()) {
        var people = (await session.Query.ExecuteAsync(q => q.All<Person>()))
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees.Prefetch(e => e.Photo)) // EntitySet Employees and lazy load field of each of its items with the limit on number of items to be loaded
          .Prefetch(p => p.Manager.Photo); // Referenced entity and lazy load field for each of them
        await foreach (var person in people.AsAsyncEnumerable()) {
          var accessor = DirectStateAccessor.Get(person);
          Assert.That(accessor.GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          Assert.That(accessor.GetFieldState("Manager"), Is.EqualTo(PersistentFieldState.Loaded));
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.That(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo"), Is.EqualTo(PersistentFieldState.Loaded));
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }
  }
}
