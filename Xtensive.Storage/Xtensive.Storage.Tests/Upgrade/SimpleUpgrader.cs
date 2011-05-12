// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Modelling.Comparison.Hints;
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

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      if (runningVersion != "SimpleVersion2") return;
      // Renaming types
      hints.Add(new RenameTypeHint(
                  "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.BusinessContact",
                  typeof (SimpleVersion2.Person)));
      hints.Add(new RenameTypeHint(
                  "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Person",
                  typeof (SimpleVersion2.BusinessContact)));
      hints.Add(new RenameTypeHint(
                  "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Address",
                  typeof (SimpleVersion2.Address)));
      hints.Add(new RenameTypeHint(
                  "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee",
                  typeof (SimpleVersion2.Employee)));
      hints.Add(new RenameTypeHint(
                  "Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Order",
                  typeof (SimpleVersion2.Order)));
      // Renaming fields
      hints.Add(new RenameFieldHint(typeof(SimpleVersion2.BusinessContact), "Address", "BusinessAddress"));
      hints.Add(new RenameFieldHint(typeof(SimpleVersion2.Employee), "HomeAddress", "Address"));
      hints.Add(new RenameFieldHint(typeof(SimpleVersion2.Order), "OrderNumber", "Number"));

      // Removing fields
      hints.Add(new RemoveFieldHint("Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Order", "ProcessingTime"));
      hints.Add(new RemoveFieldHint("Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Order", "ShippingAddress"));
      hints.Add(new RemoveFieldHint("Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Order", "Data.Value2"));

      // Type changes
      hints.Add(new ChangeFieldTypeHint(typeof(SimpleVersion2.Person), "PassportNumber"));
      hints.Add(new ChangeFieldTypeHint(typeof(SimpleVersion2.Order), "Number"));

      // Moving fields
      hints.Add(new MoveFieldHint("Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee", "FirstName", typeof(SimpleVersion2.BusinessContact)));
      hints.Add(new MoveFieldHint("Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1.Employee", "LastName", typeof(SimpleVersion2.BusinessContact)));
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      var suffix = "." + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace != originalNamespace
        && base.IsTypeAvailable(type, upgradeStage);
    }
  }
}