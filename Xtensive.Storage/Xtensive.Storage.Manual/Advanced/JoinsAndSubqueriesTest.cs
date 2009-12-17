// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.24

using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Manual.Advanced.JoinsAndSubqueriesTestModel;
using System.Linq;

namespace Xtensive.Storage.Manual.Advanced.JoinsAndSubqueriesTestModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field][Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  public class Employee : Person
  {
    [Field]
    public decimal Salary { get; set; }
  }
}

namespace Xtensive.Storage.Manual.Advanced
{
  [TestFixture]
  public class JoinsAndSubqueriesTest
  {
    [Test]
    public void QueryInheritedEntityTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          var employees = Query.All<Employee>();
          employees.ToList();
        }
      }
    }

    [Test]
    public void SubQueryInheritedEntityTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          var person1 = new Person {Name = "John"};
          var person2 = new Person {Name = "Susan"};
          session.Persist();
          var query = Query.All<Person>().Select(employee=> new {employee,  Namesakes = Query.All<Person>().Where(person=>person.Name == employee.Name)});
          // Enumerate query
          foreach (var employeeData in query) {
            // Enumerate each subquery element
            foreach (Person namesake in employeeData.Namesakes) {
              // Do something with employee, namesake
            }
          }
        }
      }
    }

  }
}