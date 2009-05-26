// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Storage.Tests.Upgrade.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade
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

    protected override void AddUpgradeHints()
    {
      var context = UpgradeContext.Current;
      if (runningVersion == "2")
        foreach (var hint in Version1To2Hints)
          context.Hints.Add(hint);
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

    public override string GetTypeName(Type type)
    {
      string suffix = ".Version" + runningVersion;
      var nameSpace = type.Namespace.TryCutSuffix(suffix);
      return nameSpace + type.Name;
    }

    private IEnumerable<UpgradeHint> Version1To2Hints
    {
      get
      {
        yield return new RenameTypeHint(typeof (Person), 
          "Xtensive.Storage.Tests.Upgrade.ModelBusinessContact");
        yield return new RenameTypeHint(typeof (BusinessContact), 
          "Xtensive.Storage.Tests.Upgrade.ModelPerson");
        yield return new RenameNodeHint("Tables/BusinessContact", "Tables/Person");
        yield return new RenameNodeHint("Tables/Person", "Tables/BusinessContact");
        yield return new CopyDataHint("Tables/Employee/Columns/LastName", "Tables/BusinessContact/Columns/LastName",
          "Tables/Employee/Columns/Id", "Tables/BusinessContact/Columns/Id");
        yield return new CopyDataHint("Tables/Employee/Columns/FirstName", "Tables/BusinessContact/Columns/FirstName",
          "Tables/Employee/Columns/Id", "Tables/BusinessContact/Columns/Id");
      }
    }
  }
}