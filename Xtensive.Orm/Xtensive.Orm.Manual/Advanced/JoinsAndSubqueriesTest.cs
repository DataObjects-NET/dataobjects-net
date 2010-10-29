// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.24

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Manual.Advanced.JoinsAndSubqueriesTest
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field][Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  public class Employee : Person
  {
    [Field]
    public decimal Salary { get; set; }
  }

  #endregion

  [TestFixture]
  public class JoinsAndSubqueriesTest
  {
    private Domain existingDomain;

    [Test]
    public void QueryForInheritedEntityTest()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var employees = session.Query.All<Employee>();
          employees.ToList();
        }
      }
    }

    [Test]
    public void SubQueryForInheritedEntityTest()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var query = session.Query.All<Person>().Select(employee => 
            new {
              employee, 
              Namesakes = session.Query.All<Person>()
                .Where(person => person.Name == employee.Name)
            });

          // Enumerate query
          foreach (var employeeData in query) {
            // Enumerate each subquery element
            foreach (var namesake in employeeData.Namesakes) {
              // Do something with employee, namesake
            }
          }
        }
      }
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
          UpgradeMode = DomainUpgradeMode.Recreate
        };
        config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
        var domain = Domain.Build(config);

        using (var session = domain.OpenSession()) {
          using (var transactionScope = session.OpenTransaction()) {
            // Creating initial content
            new Person {Name = "John"};
            new Person {Name = "Susan"};

            transactionScope.Complete();
          }
        }
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}