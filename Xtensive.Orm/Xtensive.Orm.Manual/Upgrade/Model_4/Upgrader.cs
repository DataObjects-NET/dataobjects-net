// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.19

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Manual.Upgrade.Model_4
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
      foreach (var order in Session.Demand().Query.All<Order>()) {
        var product = Session.Demand().Query.All<Product>()
#pragma warning disable 612,618
          .SingleOrDefault(p => p.Name==order.ProductName);
#pragma warning restore 612,618
        if (product==null)
#pragma warning disable 612,618
          product = new Product { Name = order.ProductName };
#pragma warning restore 612,618
        order.Product = product;
      }
    }
  }
}