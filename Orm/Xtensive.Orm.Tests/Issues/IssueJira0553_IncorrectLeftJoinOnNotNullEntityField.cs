// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0553_IncorrectLeftJoinOnNotNullEntityFieldModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0553_IncorrectLeftJoinOnNotNullEntityFieldModel
{
  [Serializable]
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  [Serializable]
  public class EmployeeWithCar : Employee
  {
    [Field(Nullable = false)]
    public Car Car { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Car : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  public class BaseClass : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Job : BaseClass
  {
    [Field(Nullable = false)]
    public Location Location { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Invoice : BaseClass
  {
    [Field]
    public Job Job { get; set; }

    [Field(Nullable = true)]
    public Customer Customer { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Location : BaseClass
  {
    [Field]
    public Address Address { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Address : BaseClass
  {
    [Field]
    public string Street { get; set; }
  }

  [HierarchyRoot]
  public class Customer : BaseClass
  {
    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0553_IncorrectLeftJoinOnNotNullEntityField : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Car).Assembly, typeof(Car).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var car = new Car();
        new EmployeeWithCar { Car = car };
        new Employee();
        new Employee();

        var customer = new Customer();
        for (var i = 0; i < 10; i++) {
          if (i % 2==0) {
            var job = new Job() {
                                  Location = new Location() {
                                                              Address = new Address() {
                                                                                        Street = string.Format("{0} street", i + 1.ToString())
                                                                                      }
                                                            }
                                };
            var invoice = new Invoice() {Customer = customer, Job = job};
          }
          else {
            var invoice = new Invoice() { Customer = customer };
          }
        }
        t.Complete();
      }
    }

    [Test]
    public void BadWorkTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var badResult = session.Query.All<Employee>()
          .LeftJoin(
            session.Query.All<EmployeeWithCar>(),
            e => e.Id,
            ewc => ewc.Id,
            (e, ewc) => new {
              e.Id,
              CarObject = ewc.Car
            });

        Assert.AreEqual(3, badResult.Count());
      }
    }

    [Test]
    public void GoodWorkTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var goodResult = session.Query.All<Employee>()
          .LeftJoin(
            session.Query.All<EmployeeWithCar>(),
            e => e.Id,
            ewc => ewc.Id,
            (e, ewc) => new {
              e.Id,
              Car = ewc.Car.Id
            });
        Assert.AreEqual(3, goodResult.Count());
      }
    }

    [Test]
    public void WorkaroundTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var wordaround = session.Query.All<Employee>()
          .LeftJoin(
            session.Query.All<EmployeeWithCar>(),
              e => e.Id,
              ewc => ewc.Id,
              (e, ewc) => new {
                e.Id,
                CarId = ewc.Car.Id
              })
          .LeftJoin(
            session.Query.All<Car>(),
            e => e.CarId,
            c => c.Id,
            (e, c) => new {
              e.Id,
              Car = c
            });
        Assert.AreEqual(3, wordaround.Count());
      }
    }

    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customer = session.Query.All<Customer>().First();
        var result = (from i in session.Query.All<Invoice>()
          from j in session.Query.All<Job>().Where(j => j==i.Job).DefaultIfEmpty()
          where i.Customer.Id==customer.Id
          select new {i.Id, Location = j!=null && j.Location!=null ? j.Location.Address.Street : ""}).ToList();
        Assert.AreEqual(10, result.Count);
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var results = (from i in session.Query.All<Invoice>()
          from j in session.Query.All<Job>().Where(j => j==i.Job).DefaultIfEmpty()
          select new {i.Id, Location = j.Location}).Where(el => el.Location!=null || string.IsNullOrEmpty(el.Location.Address.Street)).ToList();
        Assert.AreEqual(5, results.Count);
      }
    }

    [Test]
    public void Test3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var cusomer = session.Query.All<Customer>().First();
        var result = (from i in session.Query.All<Invoice>()
                      from j in session.Query.All<Job>().Where(j => j==i.Job).DefaultIfEmpty()
                      from l in session.Query.All<Location>().Where(l => l==j.Location).DefaultIfEmpty()
                      where i.Customer.Id==cusomer.Id
                      select new { i.Id, Location = l!=null ? l.Address.Street : "", }).ToList();
        Assert.AreEqual(10, result.Count);
      }
    }
  }
}
