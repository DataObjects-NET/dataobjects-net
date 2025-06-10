// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.Models.Version2;

namespace Xtensive.Orm.Tests.Upgrade.Models
{
  [Serializable]
  public class Upgrader : UpgradeHandler
  {
    [Flags]
    public enum ModelParts
    {
      Order = 1 << 0,
      Product = 1 << 1,
      BoyGirl = 1 << 2,
      CrazyAssociations = 1 << 3,
      ComplexFieldCopy = 1 << 4,
      Generics = Order | BoyGirl | 1 << 5,
      All = Order | Product | BoyGirl | CrazyAssociations | ComplexFieldCopy | Generics
    }

    private static bool isEnabled = false;
    private static int? runningVersion;
    private static ModelParts modelParts = ModelParts.All;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable EnableForVersion(int version)
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

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable EnableForVersion(int version, ModelParts parts)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      runningVersion = version;
      modelParts = parts;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
        modelParts = ModelParts.All;
      });
    }


    public override bool IsEnabled => isEnabled;

    protected override string DetectAssemblyVersion() => runningVersion.ToString();

    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
    {
      if (runningVersion == 2)
        Version1To2Hints.ForEach(hint => hints.Add(hint));
      if (runningVersion == 3)
        Version1To3Hints.ForEach(hint => hints.Add(hint));
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = $".Version{runningVersion}";
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace != originalNamespace
        && base.IsTypeAvailable(type, upgradeStage);
    }

    private static IEnumerable<UpgradeHint> Version1To2Hints
    {
      get {
        // renaming types
        if (modelParts.HasFlag(ModelParts.Order)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.BusinessContact", typeof(Person));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Person", typeof(BusinessContact));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Address", typeof(Address));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", typeof(Employee));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Order", typeof(Order));
        }

        if (modelParts.HasFlag(ModelParts.Product)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Product", typeof(Product));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Category", typeof(ProductGroup));
        }

        if (modelParts.HasFlag(ModelParts.BoyGirl)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Boy", typeof(Boy));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Girl", typeof(Girl));
        }

        if (modelParts.HasFlag(ModelParts.CrazyAssociations)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity1", typeof(Entity1));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity2", typeof(Entity2));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity3", typeof(Entity3));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Entity4", typeof(Entity4));

          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure1", typeof(Structure1));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure2", typeof(Structure2));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure3", typeof(Structure3));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure4", typeof(Structure4));

          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer1", typeof(StructureContainer1));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer2", typeof(StructureContainer2));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer3", typeof(StructureContainer3));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.StructureContainer4", typeof(StructureContainer4));
        }

        if (modelParts.HasFlag(ModelParts.ComplexFieldCopy)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.MyStructureOwner", typeof(MyStructureOwner));
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.ReferencedEntity", typeof(ReferencedEntity));
        }

        if (modelParts.HasFlag(ModelParts.Generics)) {
          yield return new RenameTypeHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Sync<>", typeof(NewSync<>));
        }

        // renaming fields
        if (modelParts.HasFlag(ModelParts.Order)) {
          yield return new RenameFieldHint(typeof(Order), "OrderNumber", "Number");
        }

        if (modelParts.HasFlag(ModelParts.Product)) {
          yield return new RenameFieldHint(typeof(Product), "Name", "Title");
          yield return new RenameFieldHint(typeof(Product), "Category", "Group");
          yield return new RenameFieldHint(typeof(ProductGroup), "Id", "GroupId");
        }

        if (modelParts.HasFlag(ModelParts.BoyGirl)) {
          yield return new RenameFieldHint(typeof(Boy), "FriendlyGirls", "MeetWith");
          yield return new RenameFieldHint(typeof(Girl), "FriendlyBoys", "MeetWith");
        }

        if (modelParts.HasFlag(ModelParts.CrazyAssociations)) {
          yield return new RenameFieldHint(typeof(Entity1), "Id", "Code");
          yield return new RenameFieldHint(typeof(Entity2), "Id", "Code");
          yield return new RenameFieldHint(typeof(Entity3), "Id", "Code");
          yield return new RenameFieldHint(typeof(Entity4), "Id", "Code");
          yield return new RenameFieldHint(typeof(Structure1), "E1", "MyE1");
          yield return new RenameFieldHint(typeof(Structure2), "E2", "MyE2");
          yield return new RenameFieldHint(typeof(Structure3), "E3", "MyE3");
          yield return new RenameFieldHint(typeof(Structure4), "E4", "MyE4");
        }

        if (modelParts.HasFlag(ModelParts.Generics)) {
          yield return new RenameFieldHint(typeof(NewSync<>), "Root", "NewRoot");
        }

        // type changes
        if (modelParts.HasFlag(ModelParts.Order)) {
          yield return new ChangeFieldTypeHint(typeof(Person), "PassportNumber");
          yield return new ChangeFieldTypeHint(typeof(Order), "Number");
        }

        // copying data
        if (modelParts.HasFlag(ModelParts.Order)) {
          yield return new CopyFieldHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", "FirstName", typeof(BusinessContact));
          yield return new CopyFieldHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee", "LastName", typeof(BusinessContact));
        }
        if (!IncludeTypeIdModifier.IsEnabled && modelParts.HasFlag(ModelParts.ComplexFieldCopy))
          yield return new CopyFieldHint(
            "Xtensive.Orm.Tests.Upgrade.Model.Version1.MyStructureOwner", "Structure", typeof(MyStructureOwner), "Reference");

      }
    }

    private IEnumerable<UpgradeHint> Version1To3Hints
    {
      get {
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Order", typeof(Models.Version3.Order));
        yield return new RenameTypeHint(
          "Xtensive.Orm.Tests.Upgrade.Model.Version1.Person", typeof(Models.Version3.Person));
      }
    }
  }
}