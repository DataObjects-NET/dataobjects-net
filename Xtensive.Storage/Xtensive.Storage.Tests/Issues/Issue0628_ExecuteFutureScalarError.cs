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
using Xtensive.Core.Caching;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0628_ExecuteFutureScalarError_Model;

namespace Xtensive.Storage.Tests.Issues
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
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var item = new Item() {Tag = 10};
        var countSimple = (Query.All<Item>() as IQueryable).Count();
        var countFuture = Query.ExecuteFutureScalar(() => (Query.All<Item>() as IQueryable).Count());
        Assert.AreEqual(1, countSimple);
        Assert.AreEqual(countSimple, countFuture);
        
        t.Complete();
      }  
    }
  }
}