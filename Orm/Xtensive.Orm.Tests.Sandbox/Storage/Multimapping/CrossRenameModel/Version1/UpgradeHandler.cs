// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

namespace Xtensive.Orm.Tests.Storage.Multimapping.CrossRenameModel.Version1
{
  public class UpgradeHandler : Upgrade.UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override string DetectAssemblyVersion()
    {
      return "1";
    }
  }
}