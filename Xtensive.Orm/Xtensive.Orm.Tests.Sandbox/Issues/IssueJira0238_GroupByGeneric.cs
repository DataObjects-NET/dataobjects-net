// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.24

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueIssueJira0238_GroupByGenericModel;

namespace Xtensive.Orm.Tests.Issues.IssueIssueJira0238_GroupByGenericModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string City { get; set; }
  }

  public class GroupKey<T1, T2>
  {
    public T1 Key1 { get; set; }
    public T2 Key2 { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0238_GroupByGeneric : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession())
      using (Session.Current.OpenTransaction()) {
        // This works
        var result1 = Query.All<Person>()
          .GroupBy(p => new {Key1 = p.City, Key2 = p.Surname})
          .Select(group => new {group.Key, Count = group.Count()})
          .ToList();
        // This does not work
        var result2 = Query.All<Person>()
          .GroupBy(p => new GroupKey<string, string> {Key1 = p.City, Key2 = p.Surname})
          .Select(group => new {group.Key, Count = group.Count()})
          .ToList();
      }
    }
  }
}