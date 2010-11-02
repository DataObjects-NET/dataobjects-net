// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Helpers;
using Xtensive.Collections;

namespace Xtensive.Orm.Manual.FutureQueries
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

    [Field]
    public Person Manager { get; set; }

    [Field]
    [Association(PairTo = "Manager")]
    public EntitySet<Person> Employees { get; private set; }

    public override string ToString()
    {
      return Name;
    }
  }

  #endregion

  [TestFixture]
  public class FutureQueriesTest
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var employee = new Person {Name = "Employee"};
        var manager  = new Person {Name = "Manager"};
        manager.Employees.Add(employee);

        var simpleCompiledQuery = session.Query.Execute(qe =>
          from person in qe.All<Person>()
          orderby person.Name
          select person
          );
        var managedPersonCount = session.Query.ExecuteDelayed(qe => (
          from person in qe.All<Person>()
          where person.Manager!=null
          select person
          ).Count());
        var personsWithEmployees = session.Query.ExecuteDelayed(qe =>
          from person in session.Query.All<Person>()
          where person.Employees.Count!=0
          select person
          );

        Console.WriteLine("All persons: {0}", 
          simpleCompiledQuery.ToCommaDelimitedString());
        Console.WriteLine("Managed person count: {0}", 
          managedPersonCount.Value);
        Console.WriteLine("Person with employees: {0}", 
          personsWithEmployees.ToCommaDelimitedString());

        transactionScope.Complete();
      }
    }
  }
}