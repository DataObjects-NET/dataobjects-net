// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.02

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.LazyLoading
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

    [Field(LazyLoad = true, Length = 8192)]
    public byte[] Avatar { get; set; }

    [Field(LazyLoad = true)]
    public Address Address { get; set; }

    [Field]
    public Person Manager { get; private set; }

    [Field]
    [Association(PairTo = "Manager")]
    public EntitySet<Person> Employees { get; private set; }
  }

  public class Address : Structure
  {
    [Field(Length = 60)]
    public string Street { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  #endregion

  [TestFixture]
  public class LazyLoadingTest
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
      using (var t = Transaction.Open()) {
        var employee = new Person();
        employee.Name = "Employee";
        employee.Avatar = new byte[] {0,1,2};
        employee.Photo = new byte[] {0,1,2};
        var manager = new Person();
        manager.Name = "Manager";
        manager.Avatar = new byte[] {0,1,2};
        manager.Photo = new byte[] {0,1,2};
        manager.Employees.Add(employee);
        t.Complete();
      }
  
      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        var persons = Query.All<Person>()
          .Prefetch(p => p.Avatar)
          .Prefetch(p => p.Photo)
          .Prefetch(p => p.Employees);
        foreach (var person in persons) {
          // some code here...
        }
        t.Complete();
      }
    }
  }
}