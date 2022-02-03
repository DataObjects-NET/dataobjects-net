// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using System.Collections.Generic;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Storage.Multimapping.CrossRenameModel.Version2
{
  public class UpgradeHandler : Orm.Upgrade.UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      hints.Add(new RenameTypeHint(
        typeof (Version1.Namespace1.Renamed1).FullName,
        typeof (Version2.Namespace2.Renamed1)));
      hints.Add(new RenameTypeHint(
        typeof (Version1.Namespace2.Renamed2).FullName,
        typeof (Version2.Namespace1.Renamed2)));
    }

    protected override string DetectAssemblyVersion()
    {
      return "2";
    }
  }
}