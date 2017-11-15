// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;

using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Tests.Upgrade.Model.Version2;
using Xtensive.Orm.Upgrade;
using M2 = Xtensive.Orm.Tests.Upgrade.Model.Version2;
using M3 = Xtensive.Orm.Tests.Upgrade.Model.Version3;

namespace Xtensive.Orm.Tests.Upgrade
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
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.BusinessContact", typeof(Person));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Sync<>", typeof(NewSync<>));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Person", typeof(BusinessContact));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Address", typeof (Address));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", typeof (Employee));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Order", typeof (Order));

        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Product", typeof (Product));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Category", typeof (ProductGroup));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Boy", typeof (Boy));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Girl", typeof (Girl));

        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity1", typeof (Entity1));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity2", typeof (Entity2));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity3", typeof (Entity3));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity4", typeof (Entity4));

        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure1", typeof (Structure1));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure2", typeof (Structure2));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure3", typeof (Structure3));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure4", typeof (Structure4));

        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer1", typeof (StructureContainer1));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer2", typeof (StructureContainer2));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer3", typeof (StructureContainer3));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer4", typeof (StructureContainer4));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.MyStructureOwner", typeof (MyStructureOwner));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.ReferencedEntity", typeof (ReferencedEntity));


        // renaming fields
        yield return new RenameFieldHint(typeof (Order), "OrderNumber", "Number");

        yield return new RenameFieldHint(typeof (NewSync<>), "Root", "NewRoot");

        yield return new RenameFieldHint(typeof (Product), "Name", "Title");
        yield return new RenameFieldHint(typeof (Product), "Category", "Group");
        yield return new RenameFieldHint(typeof (ProductGroup), "Id", "GroupId");
        yield return new RenameFieldHint(typeof (Boy), "FriendlyGirls", "MeetWith");
        yield return new RenameFieldHint(typeof (Girl), "FriendlyBoys", "MeetWith");
        yield return new RenameFieldHint(typeof (Entity1), "Id", "Code");
        yield return new RenameFieldHint(typeof (Entity2), "Id", "Code");
        yield return new RenameFieldHint(typeof (Entity3), "Id", "Code");
        yield return new RenameFieldHint(typeof (Entity4), "Id", "Code");
        
        yield return new RenameFieldHint(typeof (Structure1), "E1", "MyE1");
        yield return new RenameFieldHint(typeof (Structure2), "E2", "MyE2");
        yield return new RenameFieldHint(typeof (Structure3), "E3", "MyE3");
        yield return new RenameFieldHint(typeof (Structure4), "E4", "MyE4");

        // type changes
        yield return new ChangeFieldTypeHint(typeof (Person), "PassportNumber");
        yield return new ChangeFieldTypeHint(typeof (Order), "Number");

        // copying data
        yield return new CopyFieldHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", "FirstName", typeof (BusinessContact));
        yield return new CopyFieldHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", "LastName", typeof (BusinessContact));
        if (!IncludeTypeIdModifier.IsEnabled)
          yield return new CopyFieldHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.MyStructureOwner", "Structure", typeof (MyStructureOwner), "Reference");
        
      }
    }

     private IEnumerable<UpgradeHint> Version1To3Hints
    {
      get
      {
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Order", typeof (Model.Version3.Order));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Person", typeof(Model.Version3.Person));
      }
    }
  }
}