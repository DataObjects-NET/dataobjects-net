// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.04.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJIRA0089_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJIRA0089_Model
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Value { get; set; }
  }

  public class SelectListItem
  {
    public string Name { get; set; }

    public string Value { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJIRA0089 : AutoBuildTest
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
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {

          Expression<Func<MyEntity, string>> x = c => c.Name;
          Expression<Func<MyEntity, SelectListItem>> y = c => new SelectListItem() { Name = c.Name, Value = c.Value};
          var items1 = GetItems(x, y).ToList();
//          var items2 = Query.All<Class1>().OrderBy(x).Select(c => new TextValue{Value = x}).ToList();
          // Rollback
        }
      }
    }

    public IEnumerable<SelectListItem> GetItems (
      Expression<Func<MyEntity, string>> order,
      Expression<Func<MyEntity, SelectListItem>> selector)
    {
      return Session.Current.Query.All<MyEntity>().OrderBy(order).Select(selector);
    }
  }
}