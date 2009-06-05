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
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Storage.Building;
using M1 = Xtensive.Storage.Tests.Upgrade.Model.Version1;
using M2 = Xtensive.Storage.Tests.Upgrade.Model.Version2;
using M3 = Xtensive.Storage.Tests.Upgrade.Model.Version3;

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

      if (runningVersion=="2")
        Version1To2Hints.Apply(hint=>context.Hints.Add(hint));

      if (runningVersion=="3")
        Version1To3Hints.Apply(hint=>context.Hints.Add(hint));
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
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.BusinessContact", typeof(M2.Person));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Person", typeof(M2.BusinessContact));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Address", typeof (M2.Address));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", typeof (M2.Employee));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Order", typeof (M2.Order));

        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Product", typeof (M2.Product));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Category", typeof (M2.ProductGroup));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Boy", typeof (M2.Boy));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Girl", typeof (M2.Girl));

        // renaming fields
        yield return new RenameFieldHint(typeof (M2.Product), "Name", "Title");
        yield return new RenameFieldHint(typeof (M2.Product), "Category", "Group");
        yield return new RenameFieldHint(typeof (M2.ProductGroup), "Id", "GroupId");
        yield return new RenameFieldHint(typeof (M2.Boy), "FriendlyGirls", "MeetWith");
        yield return new RenameFieldHint(typeof (M2.Girl), "FriendlyBoys", "MeetWith");
        
        // copying data
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "FirstName", typeof (M2.BusinessContact));
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "LastName", typeof (M2.BusinessContact));
        
      }
    }

     private IEnumerable<UpgradeHint> Version1To3Hints
    {
      get
      {
        //yield return new RenameTypeHint(
        //  "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", typeof (Employee));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Order", typeof (M3.Order));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Person", typeof(M3.Person));
        //yield return new RenameTypeHint(
        //  "Xtensive.Storage.Tests.Upgrade.Model.Version1.BusinessContact", typeof(BusinessContact));
      }
    }
  }
}