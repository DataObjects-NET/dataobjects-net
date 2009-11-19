// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.12

using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Manual.Upgrade.Model_3
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
        new RenameTypeHint(typeof (Model_2.Order).FullName, typeof (Order)));
      hintSet.Add(
        new RenameTypeHint(typeof (Model_2.Customer).FullName, typeof (Person)));
      hintSet.Add(
        new RenameFieldHint(typeof (Person), "Name", "FullName"));

    }
  }
}