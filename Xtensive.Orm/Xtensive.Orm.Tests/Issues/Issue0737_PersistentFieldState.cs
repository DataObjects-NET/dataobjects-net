// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.12

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0733_UseINNER_JOIN_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0733_UseINNER_JOIN_Model
  {
    [HierarchyRoot]
    public class Person : Entity
    {
      [Key,Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field(Nullable = false)]
      public City City { get; set; }
    }

    [HierarchyRoot]
    public class City : Entity
    {
      [Key,Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  [Serializable]
  public class Issue0733_UseINNER_JOIN : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var msk = new City() {Name = "Moscow"};
        var ekb = new City() {Name = "Yekaterinburg"};
        for (int i = 0; i < 100; i++) {
          new Person() {Name = "Alex " + i, City = msk};
          new Person() {Name = "Ivan " + i, City = ekb};
        }

        var list = session.Query.All<Person>()
          .OrderBy(p => p.City.Name)
          .Take(10)
          .ToList();
      }
    }
  }
}