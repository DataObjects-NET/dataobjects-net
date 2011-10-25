// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Sql.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;
using ErrorCases = Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.ErrorCases;
using NonClustered = Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.NonClusteredHierarchy;
using Clustered = Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.ClusteredHierarchy;
using CustomClustered = Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.CustomClusteredHierarchy;

/*
 *  Common layout for all hierarchies:
 *  
 *         Root  -------------\
 *      /   |    \             \
 *     /    |     \             \
 *     |    V      \             \
 *     |   Item     \             \
 *     |             |             |
 *     V             V             V
 *  AbstractItem   ClusteredItem  ClabstractItem
 *     |                  |                    |
 *     V                  V                    V
 *  AbstractItemChild  ClusteredItemChild    ClabstractItemChild
 *
 *  AbstractItem is abstract
 *  ClusteredItem declares clustered secondary index
 *  ClabstractItem is both abstract and clustered
 */

namespace Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.NonClusteredHierarchy
{
  [HierarchyRoot(Clustered = false)]
  public class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Item : Root
  {
  }

  [Index("Value", Clustered = true)]
  public class ClusteredItem : Root
  {
    [Field]
    public int Value { get; set; }
  }

  public class ClusteredItemChild : ClusteredItem
  {
  }

  public abstract class AbstractItem : Root
  {
  }

  public class AbstractItemChild : AbstractItem
  {
  }

  [Index("Value", Clustered = true)]
  public abstract class ClabstractItem : Root
  {
    [Field]
    public int Value { get; set; }
  }

  public class ClabstractItemChild : ClabstractItem
  {
  }
}

namespace Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.ClusteredHierarchy
{
  [HierarchyRoot]
  public class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Item : Root
  {
  }

  [Index("Value", Clustered = true)]
  public class ClusteredItem : Root
  {
    [Field]
    public int Value { get; set; }
  }

  public class ClusteredItemChild : ClusteredItem
  {
  }

  public abstract class AbstractItem : Root
  {
  }

  public class AbstractItemChild : AbstractItem
  {
  }

  [Index("Value", Clustered = true)]
  public abstract class ClabstractItem : Root
  {
    [Field]
    public int Value { get; set; }
  }

  public class ClabstractItemChild : ClabstractItem
  {
  }
}

namespace Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.CustomClusteredHierarchy
{

  [HierarchyRoot, Index("Value", Clustered = true)]
  public class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }
  }

  public class Item : Root
  {
  }

  [Index("AnotherValue", Clustered = true)]
  public class ClusteredItem : Root
  {
    [Field]
    public int AnotherValue { get; set; }
  }

  public class ClusteredItemChild : ClusteredItem
  {
  }

  public abstract class AbstractItem : Root
  {
  }

  public class AbstractItemChild : AbstractItem
  {
  }

  [Index("AnotherValue", Clustered = true)]
  public abstract class ClabstractItem : Root
  {
    [Field]
    public int AnotherValue { get; set; }
  }

  public class ClabstractItemChild : ClabstractItem
  {
  }
}

namespace Xtensive.Orm.Tests.Storage.ClusteredIndexesTestModels.ErrorCases
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class SingleTableBase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Index("Value", Clustered = true)]
  public class SingleTableChild : SingleTableBase
  {
    [Field]
    int Value { get; set; }
  }

  [Index("Value", Clustered = true)]
  public interface IInterfaceWithClusteredIndex : IEntity
  {
    [Field]
    int Value { get; set; }
  }

  [HierarchyRoot]
  public class BadInterfaceImplementor : Entity, IInterfaceWithClusteredIndex
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }
  }

  [HierarchyRoot, Index("Value1", Clustered = true), Index("Value2", Clustered = true)]
  public class DuplicateClusteredIndex : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value1 { get; set; }

    [Field]
    public int Value2 { get; set; }
  }

  [HierarchyRoot, Index("Value", Filter = "IndexRange", Clustered = true)]
  public class PartialClusteredIndex : Entity
  {
    private static Expression<Func<PartialClusteredIndex, bool>> IndexRange()
    {
      return item => item.Value > 0;
    }

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ClusteredIndexesTest
  {
    private Domain domain;
    private HashSet<Type> domainTypes;
    private InheritanceSchema? inheritanceSchema;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ClusteredIndexes);
    }

    [TearDown]
    public void TearDown()
    {
      CleanDomain();
    }

    private void CleanDomain()
    {
      if (domain==null)
        return;
      try {
        domain.Dispose();
      }
      finally {
        domain = null;
      }
    }

    private IEnumerable<Type> GetAllValidTypes()
    {
      yield return typeof (NonClustered.Root);
      yield return typeof (NonClustered.Item);
      yield return typeof (NonClustered.ClusteredItem);
      yield return typeof (NonClustered.ClusteredItemChild);
      yield return typeof (NonClustered.AbstractItem);
      yield return typeof (NonClustered.AbstractItemChild);
      yield return typeof (NonClustered.ClabstractItem);
      yield return typeof (NonClustered.ClabstractItemChild);

      yield return typeof (Clustered.Root);
      yield return typeof (Clustered.Item);
      yield return typeof (Clustered.ClusteredItem);
      yield return typeof (Clustered.ClusteredItemChild);
      yield return typeof (Clustered.AbstractItem);
      yield return typeof (Clustered.AbstractItemChild);
      yield return typeof (Clustered.ClabstractItem);
      yield return typeof (Clustered.ClabstractItemChild);

      yield return typeof (CustomClustered.Root);
      yield return typeof (CustomClustered.Item);
      yield return typeof (CustomClustered.ClusteredItem);
      yield return typeof (CustomClustered.ClusteredItemChild);
      yield return typeof (CustomClustered.AbstractItem);
      yield return typeof (CustomClustered.AbstractItemChild);
      yield return typeof (CustomClustered.ClabstractItem);
      yield return typeof (CustomClustered.ClabstractItemChild);
    }

    private IEnumerable<Type> GetTypesValidForSingleTable()
    {
      yield return typeof (NonClustered.Root);
      yield return typeof (NonClustered.Item);
      yield return typeof (NonClustered.AbstractItem);
      yield return typeof (NonClustered.AbstractItemChild);

      yield return typeof (Clustered.Root);
      yield return typeof (Clustered.Item);
      yield return typeof (Clustered.AbstractItem);
      yield return typeof (Clustered.AbstractItemChild);

      yield return typeof (CustomClustered.Root);
      yield return typeof (CustomClustered.Item);
      yield return typeof (CustomClustered.AbstractItem);
      yield return typeof (CustomClustered.AbstractItemChild);
    }

    private Table GetTable(Type type)
    {
      var declaringType = domain.Model.Types[type];
      if (declaringType.Hierarchy.InheritanceSchema==InheritanceSchema.SingleTable)
        declaringType = declaringType.Hierarchy.Root;
      return ((Xtensive.Storage.Providers.Sql.DomainHandler) domain.Handler).Schema.Tables[declaringType.MappingName];
    }

    private void CheckType(Type type, bool primaryKeyIsClustered, string clusteredSecondaryIndexColumn)
    {
      var table = GetTable(type);
      var primaryKey = table.TableConstraints.OfType<PrimaryKey>().Single();
      var index = table.Indexes.FirstOrDefault(i => i.IsClustered);
      if (primaryKeyIsClustered)
        Assert.IsTrue(primaryKey.IsClustered);
      else
        Assert.IsFalse(primaryKey.IsClustered);
      if (clusteredSecondaryIndexColumn != null) {
        Assert.IsNotNull(index);
        Assert.AreEqual(1, index.Columns.Count);
        Assert.AreEqual(clusteredSecondaryIndexColumn, index.Columns[0].Name);
      }
      else
        Assert.IsNull(index);
    }

    private void BuildDomain(DomainUpgradeMode upgradeMode)
    {
      CleanDomain();

      var config = DomainConfigurationFactory.Create();

      config.NamingConvention.NamespacePolicy = NamespacePolicy.Synonymize;
      config.NamingConvention.NamespaceSynonyms[typeof (NonClustered.Root).Namespace] = "NC";
      config.NamingConvention.NamespaceSynonyms[typeof (Clustered.Root).Namespace] = "C";
      config.NamingConvention.NamespaceSynonyms[typeof (CustomClustered.Root).Namespace] = "CC";
      config.NamingConvention.NamespaceSynonyms[typeof (ErrorCases.DuplicateClusteredIndex).Namespace] = "E";

      foreach (var type in domainTypes)
        config.Types.Register(type);
      config.Types.Register(typeof (SingleTableSchemaModifier).Assembly, typeof (SingleTableSchemaModifier).Namespace);

      config.UpgradeMode = upgradeMode;
      if (inheritanceSchema!=null)
        InheritanceSchemaModifier.ActivateModifier(inheritanceSchema.Value);
      else
        InheritanceSchemaModifier.DeactivateModifiers();

      domain = Domain.Build(config);
    }

    private void InitializeTest(InheritanceSchema? schema, IEnumerable<Type> types)
    {
      inheritanceSchema = schema;
      domainTypes = types.ToHashSet();
    }

    private void RunFailureTest(Type badType)
    {
      InitializeTest(null, new[] {badType});
      AssertEx.Throws<DomainBuilderException>(() => BuildDomain(DomainUpgradeMode.Recreate));
    }

    [Test]
    public void ClassTableTest()
    {
      InitializeTest(InheritanceSchema.ClassTable, GetAllValidTypes());

      BuildDomain(DomainUpgradeMode.Recreate);

      CheckType(typeof (NonClustered.Root), false, null);
      CheckType(typeof (NonClustered.Item), false, null);
      CheckType(typeof (NonClustered.ClusteredItem), false, "Value");
      CheckType(typeof (NonClustered.ClusteredItemChild), false, null);
      CheckType(typeof (NonClustered.AbstractItem), false, null);
      CheckType(typeof (NonClustered.AbstractItemChild), false, null);
      CheckType(typeof (NonClustered.ClabstractItem), false, "Value");
      CheckType(typeof (NonClustered.ClabstractItemChild), false, null);

      CheckType(typeof (Clustered.Root), true, null);
      CheckType(typeof (Clustered.Item), true, null);
      CheckType(typeof (Clustered.ClusteredItem), false, "Value");
      CheckType(typeof (Clustered.ClusteredItemChild), true, null);
      CheckType(typeof (Clustered.AbstractItem), true, null);
      CheckType(typeof (Clustered.AbstractItemChild), true, null);
      CheckType(typeof (Clustered.ClabstractItem), false, "Value");
      CheckType(typeof (Clustered.ClabstractItemChild), true, null);

      CheckType(typeof (CustomClustered.Root), false, "Value");
      CheckType(typeof (CustomClustered.Item), true, null);
      CheckType(typeof (CustomClustered.ClusteredItem), false, "AnotherValue");
      CheckType(typeof (CustomClustered.ClusteredItemChild), true, null);
      CheckType(typeof (CustomClustered.AbstractItem), true, null);
      CheckType(typeof (CustomClustered.AbstractItemChild), true, null);
      CheckType(typeof (CustomClustered.ClabstractItem), false, "AnotherValue");
      CheckType(typeof (CustomClustered.ClabstractItemChild), true, null);

      BuildDomain(DomainUpgradeMode.Validate);
    }

    [Test]
    public void ConcreteTableTest()
    {
      InitializeTest(InheritanceSchema.ConcreteTable, GetAllValidTypes());

      BuildDomain(DomainUpgradeMode.Recreate);

      CheckType(typeof (NonClustered.Root), false, null);
      CheckType(typeof (NonClustered.Item), false, null);
      CheckType(typeof (NonClustered.ClusteredItem), false, "Value");
      CheckType(typeof (NonClustered.ClusteredItemChild), false, "Value");
      // Table does not exist: NonClustered.AbstractItem
      CheckType(typeof (NonClustered.AbstractItemChild), false, null);
      // Table does not exist: NonClustered.ClabstractItem
      CheckType(typeof (NonClustered.ClabstractItemChild), false, "Value");

      CheckType(typeof (Clustered.Root), true, null);
      CheckType(typeof (Clustered.Item), true, null);
      CheckType(typeof (Clustered.ClusteredItem), false, "Value");
      CheckType(typeof (Clustered.ClusteredItemChild), false, "Value");
      // Table does not exist: Clustered.AbstractItem
      CheckType(typeof (Clustered.AbstractItemChild), true, null);
      // Table does not exist: Clustered.ClabstractItem
      CheckType(typeof (Clustered.ClabstractItemChild), false, "Value");

      CheckType(typeof (CustomClustered.Root), false, "Value");
      CheckType(typeof (CustomClustered.Item), false, "Value");
      CheckType(typeof (CustomClustered.ClusteredItem), false, "AnotherValue");
      CheckType(typeof (CustomClustered.ClusteredItemChild), false, "AnotherValue");
      // Table does not exist: CustomClustered.AbstractItem
      CheckType(typeof (CustomClustered.AbstractItemChild), false, "Value");
      // Table does not exist: CustomClustered.ClabstractItem
      CheckType(typeof (CustomClustered.ClabstractItemChild), false, "AnotherValue");

      BuildDomain(DomainUpgradeMode.Validate);
    }

    [Test]
    public void SingleTableTest()
    {
      InitializeTest(InheritanceSchema.SingleTable, GetTypesValidForSingleTable());

      BuildDomain(DomainUpgradeMode.Recreate);

      CheckType(typeof (NonClustered.Root), false, null);
      CheckType(typeof (NonClustered.Item), false, null);
      CheckType(typeof (NonClustered.AbstractItem), false, null);
      CheckType(typeof (NonClustered.AbstractItemChild), false, null);

      CheckType(typeof (Clustered.Root), true, null);
      CheckType(typeof (Clustered.Item), true, null);
      CheckType(typeof (Clustered.AbstractItem), true, null);
      CheckType(typeof (Clustered.AbstractItemChild), true, null);

      CheckType(typeof (CustomClustered.Root), false, "Value");
      CheckType(typeof (CustomClustered.Item), false, "Value");
      CheckType(typeof (CustomClustered.AbstractItem), false, "Value");
      CheckType(typeof (CustomClustered.AbstractItemChild), false, "Value");

      BuildDomain(DomainUpgradeMode.Validate);
    }

    [Test]
    public void DuplicateClusteredIndexTest()
    {
      RunFailureTest(typeof (ErrorCases.DuplicateClusteredIndex));
    }
    
    [Test]
    public void PartialClusteredIndexTest()
    {
      RunFailureTest(typeof (ErrorCases.PartialClusteredIndex));
    }

    [Test]
    public void ClusteredIndexInInterfaceTest()
    {
      RunFailureTest(typeof (ErrorCases.BadInterfaceImplementor));
    }

    [Test]
    public void ChangeSingleTableClusteringInChildTest()
    {
      RunFailureTest(typeof (ErrorCases.SingleTableChild));
    }
  }
}