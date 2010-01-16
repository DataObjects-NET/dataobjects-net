// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Upgrade;
using SimpleVersion2 = Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion2;

namespace Xtensive.Storage.Tests.Upgrade
{
  [Serializable]
  public class SimpleUpgrader : UpgradeHandler
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

    public override bool IsEnabled
    {
      get { return isEnabled; }
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

      if (runningVersion == "SimpleVersion2")
        Hints.Apply(hint => context.Hints.Add(hint));
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      var suffix = "." + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace != originalNamespace
        && base.IsTypeAvailable(type, upgradeStage);
    }

    private static IEnumerable<UpgradeHint> Hints
    {
      get
      {
        // renaming types
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.BusinessContact", typeof(SimpleVersion2.Person));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Person", typeof(SimpleVersion2.BusinessContact));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Address", typeof(SimpleVersion2.Address));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee", typeof(SimpleVersion2.Employee));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Order", typeof(SimpleVersion2.Order));

        // renaming fields
        yield return new RenameFieldHint(typeof(SimpleVersion2.Order), "OrderNumber", "Number");

        // type changes
        yield return new ChangeFieldTypeHint(typeof(SimpleVersion2.Person), "PassportNumber");
        yield return new ChangeFieldTypeHint(typeof(SimpleVersion2.Order), "Number");

        // copying data
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee", "FirstName", typeof(SimpleVersion2.BusinessContact));
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee", "LastName", typeof(SimpleVersion2.BusinessContact));
      }
    }
  }
}