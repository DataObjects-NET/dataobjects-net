// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion2;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade
{
  [Serializable]
  public class SimpleUpgrader : UpgradeHandler
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
      if (runningVersion != "SimpleVersion2") {
        return;
      }

      // Renaming types
      _ = hints.Add(new RenameTypeHint(
                  "Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.BusinessContact",
                  typeof (Person)));
      _ = hints.Add(new RenameTypeHint(
                  "Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Person",
                  typeof (BusinessContact)));
      _ = hints.Add(new RenameTypeHint(
                  "Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Address",
                  typeof (Address)));
      _ = hints.Add(new RenameTypeHint(
                  "Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Employee",
                  typeof (Employee)));
      _ = hints.Add(new RenameTypeHint(
                  "Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Order",
                  typeof (Order)));
      // Renaming fields
      _ = hints.Add(new RenameFieldHint(typeof(BusinessContact), "Address", "BusinessAddress"));
      _ = hints.Add(new RenameFieldHint(typeof(Employee), "HomeAddress", "Address"));
      _ = hints.Add(new RenameFieldHint(typeof(Order), "OrderNumber", "Number"));

      // Removing fields
      _ = hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Order", "ProcessingTime"));
      _ = hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Order", "ShippingAddress"));
      _ = hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Order", "Data.Value2"));

      // Type changes
      _ = hints.Add(new ChangeFieldTypeHint(typeof(Person), "PassportNumber"));
      _ = hints.Add(new ChangeFieldTypeHint(typeof(Order), "Number"));

      // Moving fields
      _ = hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Employee", "FirstName", typeof(BusinessContact)));
      _ = hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1.Employee", "LastName", typeof(BusinessContact)));
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