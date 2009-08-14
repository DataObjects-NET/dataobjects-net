// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Tests.Upgrade.Sample3.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade.Sample3
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

      if (runningVersion=="2")
        Version1To2Hints.Apply(hint=>context.Hints.Add(hint));
    }

    public override void OnUpgrade()
    {
      var deps = Query<Employee>.All.ToList();
      foreach (var employee in deps)
        employee.DepartmentName = employee.RcDepartment;
    }

    private static IEnumerable<UpgradeHint> Version1To2Hints
    {
      get {
        // renaming types
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Sample3.Model.Version1.Person", typeof (Person));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Sample3.Model.Version1.Employee", typeof (Employee));
      }
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }
  }
}