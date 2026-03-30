// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System.Linq;
using NUnit.Framework;
using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.ConvertDomainModel.Model;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class ConvertDomainModelTest
  {
    public class Handler : UpgradeHandler
    {
      public static StorageModel TargetStorageModel;

      public override void OnComplete(Domain domain)
      {
        base.OnComplete(domain);
        TargetStorageModel = UpgradeContext.TargetStorageModel;
      }
    }

    private StorageModel Schema { get; set; }

    private Domain Domain { get; set; }

    private Domain BuildDomain()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.ForeignKeyMode = ForeignKeyMode.Reference;
      configuration.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof (A).Namespace);
      configuration.Types.Register(typeof (Handler));

      Domain.DisposeSafely();
      return Domain.Build(configuration);
    }

    [SetUp]
    public virtual void SetUp()
    {
      Domain = BuildDomain();
      Schema = Handler.TargetStorageModel;
    }

    [TearDown]
    public virtual void TearDown()
    {
      if (Domain != null) {
        Domain.Dispose();
      }
    }

    [Test]
    public void BaseTest()
    {
      int typeIdCount = IncludeTypeIdModifier.IsEnabled ? 1 : 0;

      Assert.That(Schema, Is.Not.Null);
      Assert.That(Schema.Tables["A"], Is.Not.Null);
      Assert.That(Schema.Tables["A"].PrimaryIndex, Is.Not.Null);
      Assert.That(Schema.Tables["A"].PrimaryIndex.KeyColumns.Count, Is.EqualTo(1 + typeIdCount));
      Assert.That(Schema.Tables["A"].PrimaryIndex.ValueColumns.Count, Is.EqualTo(3));
      Assert.That(Schema.Tables["A"].SecondaryIndexes.Count, Is.EqualTo(1));
      Assert.That(Schema.Tables["A"].SecondaryIndexes[0].KeyColumns.Count, Is.EqualTo(2));
      Assert.That(Schema.Tables["A"].SecondaryIndexes[0].IsUnique, Is.True);

      // SQLite does not use lenght constraints for varchar types
      if (Domain.StorageProviderInfo.ProviderName!=WellKnown.Provider.Sqlite)
        Assert.That(Schema.Tables["A"].Columns["Col3"].Type, Is.EqualTo(new StorageTypeInfo(typeof (string), null, 125)));

      Assert.That(Schema.Tables["B"], Is.Not.Null);
      Assert.That(Schema.Tables["B"].PrimaryIndex, Is.Not.Null);
      Assert.That(Schema.Tables["B"].PrimaryIndex.KeyColumns.Count, Is.EqualTo(1 + typeIdCount));
      Assert.That(Schema.Tables["B"].PrimaryIndex.ValueColumns.Count, Is.EqualTo(2));
      Assert.That(Schema.Tables["B"].SecondaryIndexes.Count, Is.EqualTo(1));
      Assert.That(Schema.Tables["B"].SecondaryIndexes[0].IsUnique, Is.False);
    }

    [Test]
    public virtual void IncludedColumnsTest()
    {
      if (Domain.StorageProviderInfo.Supports(ProviderFeatures.IncludedColumns))
        Assert.That(Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count, Is.EqualTo(1));
      else
        Assert.That(Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count, Is.EqualTo(0));
    }

    [Test]
    public void ForeignKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints);

      var isConcreteTable = Domain.Model.Types["B"].Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable;
      if (!isConcreteTable) {
        Assert.That(Schema.Tables["B"].ForeignKeys.Count, Is.EqualTo(1));
        Assert.That(Schema.Tables["B"].ForeignKeys[0].PrimaryKey, Is.EqualTo(Schema.Tables["A"].PrimaryIndex));
      }
      else
        Assert.That(Schema.Tables["B"].ForeignKeys.Count, Is.EqualTo(1));
    }

    [Test]
    public void GeneratorsTest()
    {
      Assert.That(Schema.Sequences.Count, Is.EqualTo(1));
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