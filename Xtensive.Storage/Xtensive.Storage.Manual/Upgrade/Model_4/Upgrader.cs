// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.19

using System.Linq;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Manual.Upgrade.Model_4
{
  public class Upgrader : UpgradeHandler
  {
    public override bool IsEnabled
    {
      get { return UpgradeHandlerEnabler.EnabledUpgradeHandler==GetType(); }
    }

    protected override void AddUpgradeHints()
    {
      var hintSet = UpgradeContext.Demand().Hints;

      hintSet.Add(
        new RenameTypeHint(typeof (Model_3.Order).FullName, typeof (Order)));
      hintSet.Add(
        new RenameTypeHint(typeof (Model_3.Person).FullName, typeof (Person)));
    }

    public override void OnUpgrade()
    {
      foreach (var order in Query<Order>.All) {
        var product = Query<Product>.All
          .SingleOrDefault(p => p.Name==order.ProductName);
        if (product==null)
          product = new Product { Name = order.ProductName };
        order.Product = product;
      }
    }
  }
}