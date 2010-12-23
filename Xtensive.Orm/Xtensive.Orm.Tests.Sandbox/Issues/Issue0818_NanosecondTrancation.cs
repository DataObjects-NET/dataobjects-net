// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.14

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0818_NanosecondTrancation_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0818_NanosecondTrancation_Model
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TimeSpan Interval { get; set; }

    public MyEntity(TimeSpan interval)
    {
      Interval = interval;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0818_NanosecondTrancation : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql); // PostgreSql stores intervals with microseconds only
      
      long ticks = 123456789;

      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var entity = new MyEntity(new TimeSpan(ticks));
        ts.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction())
        Assert.AreEqual(ticks, session.Query.All<MyEntity>().First().Interval.Ticks);
    }
  }
}