// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.19

using System;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Manual.Upgrade.Model_4
{
  public class Upgrader : UpgradeHandler
  {
    public override bool IsEnabled
    {
      get { return UpgradeHandlerEnabler.EnabledUpgradeHandler==GetType(); }
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      hints.Add(
        new RenameTypeHint(typeof (Model_3.Order).FullName, typeof (Order)));
      hints.Add(
        new RenameTypeHint(typeof (Model_3.Person).FullName, typeof (Person)));
    }

    public override void OnUpgrade()
    {
      foreach (var order in Query.All<Order>()) {
        var product = Query.All<Product>()
          .SingleOrDefault(p => p.Name==order.ProductName);
        if (product==null)
          product = new Product { Name = order.ProductName };
        order.Product = product;
      }
    }
  }
}