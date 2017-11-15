// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.01

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0441_EntitySetQueryFailsToTranslateModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0441_EntitySetQueryFailsToTranslateModel
  {
    [HierarchyRoot]
    public class Place : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class Company : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field(Nullable = false)]
      public Man CompanyOwner { get; set; }

      [Field]
      public Place Place { get; set; }

      [Field]
      [Association(PairTo = "Owner")]
      public EntitySet<Employee> Employees { get; set; }
    }

    [HierarchyRoot]
    public class Man : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class Employee : Man
    {
      [Field]
      public Company Owner { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0441_EntitySetQueryFailsToTranslate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Employee));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var somePlace = new Place {Name = "SomePlace"};
        var leader1 = new Man {Name = "Company leader 1"};
        var company1 = new Company {Name = "Company 1", CompanyOwner = leader1, Place = somePlace};
        var emp = new Employee {Name = "Slave 1", Owner = company1};
        var emp1 = new Employee {Name = "Slave 2", Owner = company1};
        var emp2 = new Employee {Name = "Slave 3", Owner = company1};
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var company = session.Query.All<Company>().Single(c => c.Name=="Company 1");
        var employees = company.Employees;
        var ordered = employees.OrderBy(e => e.Owner.Name).ThenBy(e => e.Owner.Place.Name).ThenBy(e => e.Name).ToList();
        tx.Complete();
      }
    }
  }
}