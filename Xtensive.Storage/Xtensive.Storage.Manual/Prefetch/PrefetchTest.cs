// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.24

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using System.Linq;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Manual.Prefetch
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

    public Key ManagerKey { 
      get { return GetReferenceKey(Type.Fields["Manager"]); } 
    }
  }

  #endregion

  [TestFixture]
  public class PrefetchTest
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      var domain = Domain.Build(config);

      using (Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {

        var employee = new Person {Name = "Employee", Photo = new byte[] {8, 0}};
        var manager  = new Person {Name = "Manager",  Photo = new byte[] {8, 0}};
        manager.Employees.Add(employee);
        transactionScope.Complete();
      }
  
      using (var session = Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        var persons = Query.All<Person>();
        var prefetchedPersons = persons
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees, // EntitySet Employees
            employees => employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in prefetchedPersons) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        var personIds = Query.All<Person>().Select(p => p.Id);
        var prefetchedPersons = personIds.Prefetch<Person, int>(id => Key.Create<Person>(id))
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees, // EntitySet Employees
            employees => employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in prefetchedPersons) {
          // some code here...
        }
        transactionScope.Complete();
      }

      using (var session = Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        var persons = Query.All<Person>();
        var prefetchedPersons = persons
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees, // EntitySet Employees
            employees => employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager) // Referenced entity
          .PrefetchSingle(p => p.Manager, // 1-to-1 association
            managers => managers
              .Prefetch(p => p.Photo)) // And lazy load field of each of its items
          .PrefetchMany(p => p.Employees, // Again, EntitySet
            employees => employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Employees, 40); // EntitySet with the limit on number of items to be loaded
        foreach (var person in prefetchedPersons) {
          Assert.IsTrue(DirectStateAccessor.Get(person).GetFieldState("Photo")==PersistentFieldState.Loaded);
          Assert.IsTrue(DirectStateAccessor.Get(person).GetFieldState("Manager")==PersistentFieldState.Loaded);
          if (person.ManagerKey != null) {
            Assert.IsNotNull(DirectStateAccessor.Get(session)[person.ManagerKey]);
            Assert.IsTrue(DirectStateAccessor.Get(person.Manager).GetFieldState("Photo")==PersistentFieldState.Loaded);
          }
          // some code here...
        }
        transactionScope.Complete();
      }
    }

    [Test]
    public void MultipleBatchesTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      var domain = Domain.Build(config);

      int count = 1000;

      using (Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        var random = new Random(10);
        for (int i = 0; i < count; i++)
          new Person {Name = i.ToString(), Photo = new[] {(byte) (i % 256)}};
        var persons = Query.All<Person>().OrderBy(p => p.Id).ToArray();
        for (int i = 0; i<count; i++) {
          var person = persons[i];
          if (random.Next(5)>0)
            person.Manager = persons[random.Next(count)];
        }
        transactionScope.Complete();
      }

      using (var session = Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        var persons =
          from person in Query.All<Person>()
          orderby person.Name
          select person;
        var prefetchedPersons = persons.Take(100)
          .Prefetch(p => p.Photo) // Lazy load field
          .Prefetch(p => p.Employees, // EntitySet Employees
            employees => employees
              .Prefetch(e => e.Photo)) // and lazy load field of each of its items
          .Prefetch(p => p.Manager); // Referenced entity
        foreach (var person in prefetchedPersons) {
          Assert.IsTrue(DirectStateAccessor.Get(person).GetFieldState("Photo")==PersistentFieldState.Loaded);
          Assert.IsTrue(DirectStateAccessor.Get(person).GetFieldState("Manager")==PersistentFieldState.Loaded);
          Assert.IsTrue(DirectStateAccessor.Get(person.Employees).IsFullyLoaded);
          foreach (var employee in person.Employees)
            Assert.IsTrue(DirectStateAccessor.Get(employee).GetFieldState("Photo")==PersistentFieldState.Loaded);
        }
        transactionScope.Complete();
      }
    }
  }
}