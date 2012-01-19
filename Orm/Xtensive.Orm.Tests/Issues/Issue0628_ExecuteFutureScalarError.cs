// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0628_ExecuteFutureScalarError_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0628_ExecuteFutureScalarError_Model
  {
    [HierarchyRoot]
    class Item : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Tag { get; set; }
    }
  }

  [Serializable]
  public class Issue0628_ExecuteFutureScalarError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item() {Tag = 10};
        var countSimple = session.Query.All<Item>().Count(i => i.Tag == 10);
        var countFuture1 = session.Query.ExecuteDelayed(qe => (qe.All<Item>() as IQueryable).Count());
        var countFuture2 = session.Query.ExecuteDelayed(qe => (qe.All<Item>() as IQueryable).Count());
        var countFuture3 = session.Query.ExecuteDelayed(qe => (qe.All<Item>() as IQueryable).Count());
        Assert.AreEqual(1, countSimple);
        Assert.AreEqual(countSimple, countFuture1.Value);
        Assert.AreEqual(countSimple, countFuture2.Value);
        Assert.AreEqual(countSimple, countFuture3.Value);
        
        t.Complete();
      }  
    }
  }
}