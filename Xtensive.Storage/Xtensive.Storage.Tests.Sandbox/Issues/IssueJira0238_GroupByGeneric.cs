// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.24

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueIssueJira0238_GroupByGenericModel;

namespace Xtensive.Storage.Tests.Issues.IssueIssueJira0238_GroupByGenericModel
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

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        new Person {City = "A", Surname = "1"};
        new Person {City = "A", Surname = "1"};
        new Person {City = "A", Surname = "2"};
        new Person {City = "B", Surname = "3"};

        // This works
        var result1 = Query.All<Person>()
          .GroupBy(p => new {Key1 = p.City, Key2 = p.Surname})
          .Select(group => new {group.Key, Count = group.Count()})
          .OrderBy(item => item.Key)
          .ToList();

        Assert.That(result1.Count, Is.EqualTo(3));
        Assert.That(result1[0].Key.Key1, Is.EqualTo("A"));
        Assert.That(result1[0].Key.Key2, Is.EqualTo("1"));
        Assert.That(result1[0].Count, Is.EqualTo(2));
        Assert.That(result1[1].Key.Key1, Is.EqualTo("A"));
        Assert.That(result1[1].Key.Key2, Is.EqualTo("2"));
        Assert.That(result1[1].Count, Is.EqualTo(1));
        Assert.That(result1[2].Key.Key1, Is.EqualTo("B"));
        Assert.That(result1[2].Key.Key2, Is.EqualTo("3"));
        Assert.That(result1[2].Count, Is.EqualTo(1));

        // This does not work
        var result2 = Query.All<Person>()
          .GroupBy(p => new GroupKey<string, string> {Key1 = p.City, Key2 = p.Surname})
          .Select(group => new {group.Key, Count = group.Count()})
          .OrderBy(item => item.Key)
          .ToList();

        Assert.That(result2.Count, Is.EqualTo(3));
        Assert.That(result2[0].Key.Key1, Is.EqualTo("A"));
        Assert.That(result2[0].Key.Key2, Is.EqualTo("1"));
        Assert.That(result2[0].Count, Is.EqualTo(2));
        Assert.That(result2[1].Key.Key1, Is.EqualTo("A"));
        Assert.That(result2[1].Key.Key2, Is.EqualTo("2"));
        Assert.That(result2[1].Count, Is.EqualTo(1));
        Assert.That(result2[2].Key.Key1, Is.EqualTo("B"));
        Assert.That(result2[2].Key.Key2, Is.EqualTo("3"));
        Assert.That(result2[2].Count, Is.EqualTo(1));
      }
    }
  }
}