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
      long ticks = 123456789;

      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        new MyEntity(new TimeSpan(ticks));
        ts.Complete();
      }

      using (Session.Open(Domain))
      using (var ts = Transaction.Open())
        Assert.AreEqual(ticks, Query.All<MyEntity>().First().Interval.Ticks);
    }
  }
}