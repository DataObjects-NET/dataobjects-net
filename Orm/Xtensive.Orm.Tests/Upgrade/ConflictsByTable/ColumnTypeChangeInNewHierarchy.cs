// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ColumnTypeChangeInNewHierarchyModel.Before;
using V2 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ColumnTypeChangeInNewHierarchyModel.After;
using TheTestHelper = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ColumnTypeChangeInNewHierarchyModel.TestHelper;

namespace Xtensive.Orm.Tests.Upgrade.ConflictsByTable
{
  [TestFixture]
  public sealed class ColumnTypeChangeInNewHierarchy : TestBase
  {
    protected override void CheckSingleTableComparisonResult(SchemaComparisonResult comparisonResult)
    {
      Assert.That(comparisonResult, Is.Not.Null);
      Assert.That(comparisonResult.HasUnsafeActions, Is.True);

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Sqlite)) {
        // length change is not important for SQLite
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(3));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(1));
        Assert.That(structureActions[0].Path == $"Tables/SingleTableRoot/Columns/FieldToChangeTypeRoot");
      }
      else {
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(4));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(2));
        Assert.That(structureActions
            .All(a => a.Path == $"Tables/SingleTableRoot/Columns/FieldToChangeTypeRoot"
              || a.Path == "Tables/SingleTableRoot/Columns/FieldToChangeTypeLeaf"),
          Is.True);
      }

      var dataActions = comparisonResult.UnsafeActions.OfType<DataAction>().ToList();
      Assert.That(dataActions.Count, Is.EqualTo(2));
      Assert.That(dataActions[0].Path, Is.EqualTo($"Tables/SingleTableRoot"));
      Assert.That(dataActions[1].Path, Is.EqualTo($"Tables/SingleTableRoot"));
    }

    protected override void CheckClassTableComparisonResult(SchemaComparisonResult comparisonResult)
    {
      Assert.That(comparisonResult, Is.Not.Null);
      Assert.That(comparisonResult.HasUnsafeActions, Is.True);
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Sqlite)) {
        // length change is not important for SQLite
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(4));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(1));
        Assert.That(structureActions[0].Path == $"Tables/ClassTableRoot/Columns/FieldToChangeTypeRoot");
      }
      else {
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(5));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(2));
        Assert.That(structureActions
            .All(a => a.Path == $"Tables/ClassTableRoot/Columns/FieldToChangeTypeRoot"
              || a.Path == "Tables/ClassTableLeaf/Columns/FieldToChangeTypeLeaf"),
          Is.True);
      }

      var dataActions = comparisonResult.UnsafeActions.OfType<DataAction>().ToList();
      Assert.That(dataActions.Count, Is.EqualTo(3));
      Assert.That(dataActions[0].Path, Is.EqualTo($"Tables/ClassTableRoot"));
      Assert.That(dataActions[1].Path, Is.EqualTo($"Tables/ClassTableRoot"));
      Assert.That(dataActions[2].Path, Is.EqualTo($"Tables/ClassTableLeaf"));
    }

    protected override void CheckConcreteTableComparisonResult(SchemaComparisonResult comparisonResult)
    {
      Assert.That(comparisonResult, Is.Not.Null);
      Assert.That(comparisonResult.HasUnsafeActions, Is.True);

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Sqlite)) {
        // length change is not important for SQLite
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(4));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(2));
        Assert.That(structureActions
            .All(a => a.Path.Contains($"Columns/FieldToChangeTypeRoot", StringComparison.Ordinal)),
          Is.True);
      }
      else {
        Assert.That(comparisonResult.UnsafeActions.Count, Is.EqualTo(5));

        var structureActions = comparisonResult.UnsafeActions.OfType<PropertyChangeAction>().ToList();
        Assert.That(structureActions.Count, Is.EqualTo(3));
        Assert.That(structureActions
            .All(a => a.Path.Contains($"Columns/FieldToChangeTypeRoot", StringComparison.Ordinal)
              || a.Path.Contains($"Columns/FieldToChangeTypeLeaf", StringComparison.Ordinal)),
          Is.True);
      }

      var dataActions = comparisonResult.UnsafeActions.OfType<DataAction>().ToList();
      Assert.That(dataActions.Count, Is.EqualTo(2));
      Assert.That(dataActions[0].Path, Is.EqualTo($"Tables/ConcreteTableRoot"));
      Assert.That(dataActions[1].Path, Is.EqualTo($"Tables/ConcreteTableLeaf"));
    }

    protected override void ValidateSingleTableDataRemains(Session session)
      => Assert.That(TheTestHelper.ValidateSingleTableDataRemains(session), Is.True);

    protected override void ValidateSingleTableDataExists(Session session)
      => Assert.That(TheTestHelper.ValidateSingleTableDataExists(session), Is.True);

    protected override void ValidateSingleTableDataDisapeared(Session session)
      => Assert.That(TheTestHelper.ValidateSingleTableDataDisapeared(session), Is.True);


    protected override void ValidateClassTableDataRemains(Session session)
      => Assert.That(TheTestHelper.ValidateClassTableDataRemains(session), Is.True);

    protected override void ValidateClassTableDataExists(Session session)
      => Assert.That(TheTestHelper.ValidateClassTableDataExists(session), Is.True);

    protected override void ValidateClassTableDataDisapeared(Session session)
      => Assert.That(TheTestHelper.ValidateClassTableDataDisapeared(session), Is.True);


    protected override void ValidateConcreteTableDataRemains(Session session)
      => Assert.That(TheTestHelper.ValidateConcreteTableDataRemains(session), Is.True);

    protected override void ValidateConcreteTableDataExists(Session session)
      => Assert.That(TheTestHelper.ValidateConcreteTableDataExists(session), Is.True);

    protected override void ValidateConcreteTableDataDisapeared(Session session)
      => Assert.That(TheTestHelper.ValidateConcreteTableDataDisapeared(session), Is.True);

    protected override void PopulateSingleTableDomain(Session session) => TheTestHelper.PopulateSingleTable(session);
    protected override void PopulateClassTableDomain(Session session) => TheTestHelper.PopulateClassTable(session);
    protected override void PopulateConcreteTableDomain(Session session) => TheTestHelper.PopulateConcreteTable(session);

    protected override Type[] GetTypes(InheritanceSchema inheritanceSchema, bool isAfter, bool? withHints = null)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          return isAfter
            ? withHints.HasValue && withHints.Value
              ? new[] { typeof(V2.ClassTableNewRoot), typeof(V2.ClassTableLeaf),
                  typeof(V2.ClassTableBase), typeof(V2.ClassTableUpgradeHandler) }
              : new[] { typeof(V2.ClassTableNewRoot), typeof(V2.ClassTableLeaf), typeof(V2.ClassTableBase) }
            : new[] { typeof(V1.ClassTableRoot), typeof(V1.ClassTableLeaf) };
        }
        case InheritanceSchema.ConcreteTable: {
          return isAfter
            ? withHints.HasValue && withHints.Value
              ? new[] { typeof(V2.ConcreteTableNewRoot), typeof(V2.ConcreteTableLeaf),
                  typeof(V2.ConcreteTableBase), typeof(V2.ConcreteTableUpgradeHandler) }
              : new[] { typeof(V2.ConcreteTableNewRoot), typeof(V2.ConcreteTableLeaf), typeof(V2.ConcreteTableBase) }
            : new[] { typeof(V1.ConcreteTableRoot), typeof(V1.ConcreteTableLeaf) };
        }
        case InheritanceSchema.SingleTable: {
          return isAfter
            ? withHints.HasValue && withHints.Value
              ? new[] { typeof(V2.SingleTableNewRoot), typeof(V2.SingleTableLeaf),
                  typeof(V2.SingleTableBase), typeof(V2.SingleTableUpgradeHandler) }
              : new[] { typeof(V2.SingleTableNewRoot), typeof(V2.SingleTableLeaf), typeof(V2.SingleTableBase) }
            : new[] { typeof(V1.SingleTableRoot), typeof(V1.SingleTableLeaf) };
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ColumnTypeChangeInNewHierarchyModel
{
  public static class TestHelper
  {

    public static void PopulateSingleTable(Session session)
    {
      _ = new Before.SingleTableLeaf(session, "Leaf", "Leaf") { LeafValue = "Leaf" };
      _ = new Before.SingleTableRoot(session, "Root", "Root");
    }

    public static void PopulateConcreteTable(Session session)
    {
      _ = new Before.ConcreteTableLeaf(session, "Leaf", "Leaf") { LeafValue = "Leaf" };
      _ = new Before.ConcreteTableRoot(session, "Root", "Root");
    }

    public static void PopulateClassTable(Session session)
    {
      _ = new Before.ClassTableLeaf(session, "Leaf", "Leaf") { LeafValue = "Leaf" };
      _ = new Before.ClassTableRoot(session, "Root", "Root");
    }

    public static bool ValidateSingleTableDataExists(Session session)
    {
      return session.Query.All<After.SingleTableNewRoot>().Count() == 2
        && session.Query.All<After.SingleTableLeaf>().Count() == 1;
    }

    public static bool ValidateConcreteTableDataExists(Session session)
    {
      return session.Query.All<After.ConcreteTableNewRoot>().Count() == 2
        && session.Query.All<After.ConcreteTableLeaf>().Count() == 1;
    }

    public static bool ValidateClassTableDataExists(Session session)
    {
      return session.Query.All<After.ClassTableNewRoot>().Count() == 2
        && session.Query.All<After.ClassTableLeaf>().Count() == 1;
    }

    public static bool ValidateSingleTableDataDisapeared(Session session)
    {
      return session.Query.All<After.SingleTableNewRoot>().Count() == 0
        && session.Query.All<After.SingleTableLeaf>().Count() == 0;
    }

    public static bool ValidateConcreteTableDataDisapeared(Session session)
    {
      return session.Query.All<After.ConcreteTableNewRoot>().Count() == 0
        && session.Query.All<After.ConcreteTableLeaf>().Count() == 0;
    }

    public static bool ValidateClassTableDataDisapeared(Session session)
    {
      return session.Query.All<After.ClassTableNewRoot>().Count() == 0
        && session.Query.All<After.ClassTableLeaf>().Count() == 0;
    }
    internal static bool ValidateSingleTableDataRemains(Session session)
    {
      return session.Query.All<Before.SingleTableRoot>().Count() == 2
        && session.Query.All<Before.SingleTableLeaf>().Count() == 1;
    }

    internal static bool ValidateClassTableDataRemains(Session session)
    {
      return session.Query.All<Before.ClassTableRoot>().Count() == 2
        && session.Query.All<Before.ClassTableLeaf>().Count() == 1;
    }

    internal static bool ValidateConcreteTableDataRemains(Session session)
    {
      return session.Query.All<Before.ConcreteTableRoot>().Count() == 2
        && session.Query.All<Before.ConcreteTableLeaf>().Count() == 1;
    }
  }

  namespace Before
  {
    [HierarchyRoot(InheritanceSchema.ClassTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class ClassTableRoot : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeRoot { get; set; }

      public ClassTableRoot(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    public class ClassTableLeaf : ClassTableRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeLeaf { get; set; }

      public ClassTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class ConcreteTableRoot : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeRoot { get; set; }

      public ConcreteTableRoot(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    public class ConcreteTableLeaf : ConcreteTableRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeLeaf { get; set; }

      public ConcreteTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class SingleTableRoot : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeRoot { get; set; }

      public SingleTableRoot(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    public class SingleTableLeaf : SingleTableRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 100)]
      public string FieldToChangeTypeLeaf { get; set; }

      public SingleTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }
  }

  namespace After
  {
    [HierarchyRoot(InheritanceSchema.ClassTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    [TableMapping("ClassTableRoot")]
    [Index("Name", Name = "PK_ClassTableRoot", Clustered = true, Unique = true)]
    public class ClassTableNewRoot : ClassTableBase
    {
      public ClassTableNewRoot(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public class ClassTableLeaf : ClassTableNewRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 50)]
      public string FieldToChangeTypeLeaf { get; set; }

      public ClassTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public abstract class ClassTableBase : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 50, Nullable = false)]
      public string FieldToChangeTypeRoot { get; set; }

      public ClassTableBase(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    [TableMapping("ConcreteTableRoot")]
    [Index("Name", Name = "PK_ConcreteTableRoot", Clustered = true, Unique = true)]
    public class ConcreteTableNewRoot : ConcreteTableBase
    {
      public ConcreteTableNewRoot(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public class ConcreteTableLeaf : ConcreteTableNewRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 50)]
      public string FieldToChangeTypeLeaf { get; set; }

      public ConcreteTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public abstract class ConcreteTableBase : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 50, Nullable = false)]
      public string FieldToChangeTypeRoot { get; set; }

      public ConcreteTableBase(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    [KeyGenerator(KeyGeneratorKind.None)]
    [TableMapping("SingleTableRoot")]
    [Index("Name", Name = "PK_SingleTableRoot", Clustered = true, Unique = true)]
    public class SingleTableNewRoot : SingleTableBase
    {
      public SingleTableNewRoot(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public class SingleTableLeaf : SingleTableNewRoot
    {
      [Field]
      public string LeafValue { get; set; }

      [Field(Length = 50)]
      public string FieldToChangeTypeLeaf { get; set; }

      public SingleTableLeaf(Session session, string name, string value = null)
        : base(session, name, value)
      {
      }
    }

    public abstract class SingleTableBase : Entity
    {
      [Key, Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      [Field(Length = int.MaxValue)]
      public string Value { get; set; }

      [Field(Length = 50, Nullable = false)]
      public string FieldToChangeTypeRoot { get; set; }

      public SingleTableBase(Session session, string name, string value = null)
        : base(session, name)
      {
        Value = value;
      }
    }

    public class ClassTableUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion) => true;

      protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
      {
        base.AddUpgradeHints(hints);
        var rootType = typeof(ClassTableNewRoot);
        var leafType = typeof(ClassTableLeaf);

        _ = hints.Add(
          new RenameTypeHint(rootType.Namespace.Replace("After", "Before") + ".ClassTableRoot", rootType));
        _ = hints.Add(new ChangeFieldTypeHint(rootType, "FieldToChangeTypeRoot"));
        _ = hints.Add(new ChangeFieldTypeHint(leafType, "FieldToChangeTypeLeaf"));
      }
    }

    public class ConcreteTableUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion) => true;

      protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
      {
        base.AddUpgradeHints(hints);
        var rootType = typeof(ConcreteTableNewRoot);
        var leafType = typeof(ConcreteTableLeaf);

        _ = hints.Add(
          new RenameTypeHint(rootType.Namespace.Replace("After", "Before") + ".ConcreteTableRoot", rootType));
        _ = hints.Add(new ChangeFieldTypeHint(rootType, "FieldToChangeTypeRoot"));
        _ = hints.Add(new ChangeFieldTypeHint(leafType, "FieldToChangeTypeLeaf"));
      }
    }

    public class SingleTableUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion) => true;

      protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
      {
        base.AddUpgradeHints(hints);
        var rootType = typeof(SingleTableNewRoot);
        var leafType = typeof(SingleTableLeaf);

        _ = hints.Add(
          new RenameTypeHint(rootType.Namespace.Replace("After", "Before") + ".SingleTableRoot", rootType));
        _ = hints.Add(new ChangeFieldTypeHint(rootType, "FieldToChangeTypeRoot"));
        _ = hints.Add(new ChangeFieldTypeHint(leafType, "FieldToChangeTypeLeaf"));
      }
    }
  }
}