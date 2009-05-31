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

    private static IEnumerable<UpgradeHint> Version1To2Hints
    {
      get {
        // renaming types
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.BusinessContact", typeof(Person));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Person", typeof(BusinessContact));
        // useless for upgrade purposes but leads to exception in upgrade process
        //yield return new RenameTypeHint(
        //  "Xtensive.Storage.Tests.Upgrade.Model.Version1.Address", typeof (Address));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", typeof (Employee));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Order", typeof (Order));

        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Product", typeof (Product));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Category", typeof (ProductGroup));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Boy", typeof (Boy));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Girl", typeof (Girl));

        // renaming fields
        yield return new RenameFieldHint(typeof (Product), "Name", "Title");
        yield return new RenameFieldHint(typeof (Product), "Category", "Group");
        yield return new RenameFieldHint(typeof (ProductGroup), "Id", "GroupId");
        yield return new RenameFieldHint(typeof (Boy), "FriendlyGirls", "MeetWith");
        yield return new RenameFieldHint(typeof (Girl), "FriendlyBoys", "MeetWith");
        
        // copying data
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "FirstName", typeof (BusinessContact));
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "LastName", typeof (BusinessContact));
        
      }
    }
  }
}