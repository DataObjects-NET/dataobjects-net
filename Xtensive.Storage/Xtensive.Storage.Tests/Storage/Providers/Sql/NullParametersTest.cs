// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  public class NullParametersTest : NorthwindDOModelTest
  {
    [Test]
    public void CompareWithNullParameterTest()
    {
      string region = null;
      var result = Query<Order>.All.Where(o => o.ShippingAddress.Region == region).ToList();
      Assert.AreNotEqual(0, result.Count);
    }

    [Test]
    public void SelectNullParameter()
    {
      string name = null;
      var result = Query<Order>.All.Select(o => new {o.Id, Name = name}).ToList();
      foreach (var i in result)
        Assert.IsNull(i.Name);
    }
  }
}
