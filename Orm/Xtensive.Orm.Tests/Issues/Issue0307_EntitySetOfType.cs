// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.08.13

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0307_EntitySetOfType_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0307_EntitySetOfType_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class Company : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntitySet<Employee> Employees { get; private set; }
    }

    [Serializable]
    public class Consultant : Employee
    {
      public Consultant(Company company, string firstName, string lastName, Skill mainSkill)
        : base(company, firstName, lastName)
      {
        MainSkill = mainSkill;
      }

      [Field]
      public Skill MainSkill { get; private set; }

      [Field]
      [Association(OnTargetRemove = OnRemoveAction.Deny)]
      public EntitySet<Skill> Skills { get; private set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Employee : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      public Employee(Company company, string firstName, string lastName)
      {
        ArgumentNullException.ThrowIfNull(company);

        Company = company;
        FirstName = firstName;
        LastName = lastName;
      }

      [Field]
      [Association("Employees", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public Company Company { get; private set; }

      [Field]
      public string FirstName { get; set; }

      [Field]
      public string LastName { get; set; }

      public override string ToString()
      {
        return string.Format("{0} {1}", FirstName, LastName);
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Skill : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      public Skill(string name)
      {
        Name = name;
      }

      [Field]
      public string Name { get; private set; }
    }
  }


  [Serializable]
  public class Issue0307_EntitySetOfType : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Skill).Assembly, typeof(Skill).Namespace);
      return config;
    }


    [Test]
    public void Test()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var company = new Company();
        var skill = new Skill("Programmer");
        var employee = new Employee(company, "Joe", "Smith");
        var consultant = new Consultant(company, "George", "Carlson", skill);

        var consultants = from comp in session.Query.All<Company>()
                          from cons in comp.Employees.OfType<Consultant>()
                          select cons;
        foreach (var c in consultants)
          Console.WriteLine(c.ToString());
      }
    }
  }
}