// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Helpers;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2;
using Xtensive.Orm.Upgrade;
using M1 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.DataUpgrade
{
  [Serializable]
  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static string runningVersion;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(string version)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }
    
    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override void AddUpgradeHints(Xtensive.Collections.ISet<UpgradeHint> hints)
    {
      if (runningVersion=="2")
        Version1To2Hints.ForEach(hint => hints.Add(hint));
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }

    private IEnumerable<UpgradeHint> Version1To2Hints
    {
      get
      {
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version1.A", typeof(A));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version1.C", typeof(C));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version1.D", typeof(D));
      }
    }
  }
}