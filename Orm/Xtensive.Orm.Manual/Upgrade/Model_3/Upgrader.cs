// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.12

using System.Collections.Generic;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Manual.Upgrade.Model_3
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
        new RenameTypeHint(typeof (Model_2.Order).FullName, typeof (Order)));
      hints.Add(
        new RenameTypeHint(typeof (Model_2.Customer).FullName, typeof (Person)));
      hints.Add(
        new RenameFieldHint(typeof (Person), "Name", "FullName"));
    }
  }
}