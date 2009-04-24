// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.24

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SubQueryTest : NorthwindDOModelTest
  {
    [Test]
    public void QueriableSimpleTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Where(c => c==p.Category));
//      QueryDumper.Dump(result);
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void ScalarSimpleTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Count());
      QueryDumper.Dump(result);
    }


    [Test]
    public void WhereEntitySetTest()
    {
      var result = Query<Category>.All
        .Where(c => c.Products.Count > 0);
      QueryDumper.Dump(result);
    }
  }
}