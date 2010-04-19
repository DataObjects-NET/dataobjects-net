// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Upgrade;
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

    protected override void AddUpgradeHints(Core.Collections.ISet<UpgradeHint> hints)
    {
      if (runningVersion=="2")
        Version1To2Hints.ForEach(hint => hints.Add(hint));
      if (runningVersion=="3")
        Version1To3Hints.ForEach(hint => hints.Add(hint));
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
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Sync<>", typeof(M2.NewSync<>));
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

        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Entity1", typeof (M2.Entity1));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Entity2", typeof (M2.Entity2));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Entity3", typeof (M2.Entity3));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Entity4", typeof (M2.Entity4));

        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Structure1", typeof (M2.Structure1));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Structure2", typeof (M2.Structure2));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Structure3", typeof (M2.Structure3));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Structure4", typeof (M2.Structure4));

        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.StructureContainer1", typeof (M2.StructureContainer1));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.StructureContainer2", typeof (M2.StructureContainer2));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.StructureContainer3", typeof (M2.StructureContainer3));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.StructureContainer4", typeof (M2.StructureContainer4));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.MyStructureOwner", typeof (M2.MyStructureOwner));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.ReferencedEntity", typeof (M2.ReferencedEntity));


        // renaming fields
        yield return new RenameFieldHint(typeof (M2.Order), "OrderNumber", "Number");

        yield return new RenameFieldHint(typeof (M2.NewSync<>), "Root", "NewRoot");

        yield return new RenameFieldHint(typeof (M2.Product), "Name", "Title");
        yield return new RenameFieldHint(typeof (M2.Product), "Category", "Group");
        yield return new RenameFieldHint(typeof (M2.ProductGroup), "Id", "GroupId");
        yield return new RenameFieldHint(typeof (M2.Boy), "FriendlyGirls", "MeetWith");
        yield return new RenameFieldHint(typeof (M2.Girl), "FriendlyBoys", "MeetWith");
        yield return new RenameFieldHint(typeof (M2.Entity1), "Id", "Code");
        yield return new RenameFieldHint(typeof (M2.Entity2), "Id", "Code");
        yield return new RenameFieldHint(typeof (M2.Entity3), "Id", "Code");
        yield return new RenameFieldHint(typeof (M2.Entity4), "Id", "Code");
        
        yield return new RenameFieldHint(typeof (M2.Structure1), "E1", "MyE1");
        yield return new RenameFieldHint(typeof (M2.Structure2), "E2", "MyE2");
        yield return new RenameFieldHint(typeof (M2.Structure3), "E3", "MyE3");
        yield return new RenameFieldHint(typeof (M2.Structure4), "E4", "MyE4");

        // type changes
        yield return new ChangeFieldTypeHint(typeof (M2.Person), "PassportNumber");
        yield return new ChangeFieldTypeHint(typeof (M2.Order), "Number");

        // copying data
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "FirstName", typeof (M2.BusinessContact));
        yield return new CopyFieldHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Employee", "LastName", typeof (M2.BusinessContact));
        if (!IncludeTypeIdModifier.IsEnabled)
          yield return new CopyFieldHint(
            "Xtensive.Storage.Tests.Upgrade.Model.Version1.MyStructureOwner", "Structure", typeof (M2.MyStructureOwner), "Reference");
        
      }
    }

     private IEnumerable<UpgradeHint> Version1To3Hints
    {
      get
      {
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Order", typeof (M3.Order));
        yield return new RenameTypeHint(
          "Xtensive.Storage.Tests.Upgrade.Model.Version1.Person", typeof(M3.Person));
      }
    }
  }
}