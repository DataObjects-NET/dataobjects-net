// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.09.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.DateTimeToStringTestModel;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Linq
{
  namespace DateTimeToStringTestModel
  {
    [HierarchyRoot]
    public class DateTimeTest : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; private set; }

      [Field]
      public DateTime Birthday { get; private set; }

      public DateTimeTest(string name, DateTime birthday)
      {
        Name = name;
        Birthday = birthday;
      }
    }
  }

  public class DateTimeToStringTest : AutoBuildTest
  {
    private DateTime date = new DateTime(2013, 2, 12, 22, 11, 30);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (DateTimeTest));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new DateTimeTest("Hello", date);
        tx.Complete();
      }
    }

    [Test]
    public void SelectTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<DateTimeTest>()
          .Select(d => d.Birthday.ToString("s"))
          .ToList();
        Assert.That(result[0], Is.EqualTo(date.ToString("s")));
      }
    }

    [Test]
    public void WhereTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<DateTimeTest>()
          .Where(dd => dd.Birthday.ToString("s") == date.ToString("s"))
          .ToList()
          .FirstOrDefault();

        Assert.NotNull(result);
      }
    }
  }
}




