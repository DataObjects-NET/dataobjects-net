// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.24

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.Prefetch
{
  #region Model

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
    public Person Manager { get; private set; }

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
          .PrefetchSingle(p => p.Manager, p => p.Manager, // 1-to-1 association
            managers => managers
              .Prefetch(p => p.Photo)
            );
        foreach (var person in prefetchedPersons) {
          Assert.IsTrue(person.GetFieldState("Photo")==PersistentFieldState.Loaded);
          Assert.IsTrue(person.GetFieldState("Manager")==PersistentFieldState.Loaded);
          Assert.IsNotNull(session.Cache[person.ManagerKey]);
          Assert.IsTrue(person.Manager.GetFieldState("Photo")==PersistentFieldState.Loaded);
          // some code here...
        }
        transactionScope.Complete();
      }
    }
  }
}