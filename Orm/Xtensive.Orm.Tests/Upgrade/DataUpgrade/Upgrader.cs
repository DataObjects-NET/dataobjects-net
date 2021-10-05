// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;

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
      if (isEnabled) {
        throw new InvalidOperationException();
      }
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled => isEnabled;

    protected override string DetectAssemblyVersion() => runningVersion;

    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      if (runningVersion == "2") {
        Version1To2Hints.ForEach(hint => hints.Add(hint));
      }
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace != originalNamespace
        && base.IsTypeAvailable(type, upgradeStage);
    }

    private IEnumerable<UpgradeHint> Version1To2Hints
    {
      get
      {
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1.A", typeof(A));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1.C", typeof(C));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1.D", typeof(D));
      }
    }
  }
}