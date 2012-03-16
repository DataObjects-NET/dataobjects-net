// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using NUnit.Framework;
using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.ConvertDomainModel.Model;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class ConvertDomainModelTest
  {
    protected StorageModel Schema { get; set; }

    protected Domain Domain { get; set; }
    
    protected Domain BuildDomain()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.ForeignKeyMode = ForeignKeyMode.Reference;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), typeof (A).Namespace);

      Domain.DisposeSafely();
      Domain.Build(configuration);
      Domain = Domain.Build(configuration);
      return Domain;
    }

    [SetUp]
    public virtual void SetUp()
    {
      Schema = BuildDomain().Schema;
    }

    [Test]
    public void BaseTest()
    {
      int typeIdCount = IncludeTypeIdModifier.IsEnabled ? 1 : 0;
      Assert.IsNotNull(Schema);
      Assert.IsNotNull(Schema.Tables["A"]);
      Assert.IsNotNull(Schema.Tables["A"].PrimaryIndex);
      Assert.AreEqual(1 + typeIdCount, Schema.Tables["A"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(3, Schema.Tables["A"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, Schema.Tables["A"].SecondaryIndexes.Count);
      Assert.AreEqual(2, Schema.Tables["A"].SecondaryIndexes[0].KeyColumns.Count);
      Assert.IsTrue(Schema.Tables["A"].SecondaryIndexes[0].IsUnique);
      Assert.AreEqual(new StorageTypeInfo(typeof (string), 125, null),
        Schema.Tables["A"].Columns["Col3"].Type);

      Assert.IsNotNull(Schema.Tables["B"]);
      Assert.IsNotNull(Schema.Tables["B"].PrimaryIndex);
      Assert.AreEqual(1 + typeIdCount, Schema.Tables["B"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(2, Schema.Tables["B"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, Schema.Tables["B"].SecondaryIndexes.Count);
      Assert.IsFalse(Schema.Tables["B"].SecondaryIndexes[0].IsUnique);
    }

    [Test]
    public virtual void IncludedColumnsTest()
    {
      if (Domain.StorageProviderInfo.Supports(ProviderFeatures.IncludedColumns))
        Assert.AreEqual(1,
          Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count);
      else
        Assert.AreEqual(0,
          Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count);
    }

    [Test]
    public void ForeignKeyTest()
    {
      var isConcreteTable = Domain.Model.Types["B"].Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable;
      if (!isConcreteTable) {
        Assert.AreEqual(1, Schema.Tables["B"].ForeignKeys.Count);
        Assert.AreEqual(Schema.Tables["A"].PrimaryIndex,
          Schema.Tables["B"].ForeignKeys[0].PrimaryKey);
      }
      else
        Assert.AreEqual(1, Schema.Tables["B"].ForeignKeys.Count);
    }

    [Test]
    public void GeneratorsTest()
    {
      Assert.AreEqual(1, Schema.Sequences.Count);
    }
  }
}


#region Model

namespace Xtensive.Orm.Tests.Upgrade.ConvertDomainModel.Model
{
  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  [Index("Col1", "Col2", Unique = true, IncludedFields = new[] { "Col3" })]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public Guid Col2 { get; private set; }

    [Field(Length = 125)]
    public string Col3 { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  [Index("ColA", Name = "A_IX")]
  public class B : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public A ColA { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class C : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TimeSpan Col1 { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class D : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<E> ColE { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class E : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public D ColD { get; private set; }
  }
}

#endregion