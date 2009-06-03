// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Upgrade.ConvertDomainModel.Model;
using Xtensive.Storage.Upgrade;
using TypeInfo=Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Tests.Upgrade
{
  
  [TestFixture]
  public class ConvertDomainModelTest
  {
    protected StorageInfo Schema { get; set; }

    protected Domain Domain { get; set; }
    
    protected Domain BuildDomain(string protocol)
    {
      var dc = DomainConfigurationFactory.Create(protocol);
      dc.UpgradeMode = DomainUpgradeMode.Recreate;
      dc.ForeignKeyMode = ForeignKeyMode.Reference;
      dc.Types.Register(Assembly.GetExecutingAssembly(), typeof (A).Namespace);
      Domain.Build(dc);
      Domain = Domain.Build(dc);
      return Domain;
    }
    
    [SetUp]
    public virtual void SetUp()
    {
      Schema = BuildDomain("mssql2005").Schema;
    }

    [Test]
    public void BaseTest()
    {
      Assert.IsNotNull(Schema);
      Assert.IsNotNull(Schema.Tables["A"]);
      Assert.IsNotNull(Schema.Tables["A"].PrimaryIndex);
      Assert.AreEqual(1, Schema.Tables["A"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(4, Schema.Tables["A"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, Schema.Tables["A"].SecondaryIndexes.Count);
      Assert.AreEqual(2, Schema.Tables["A"].SecondaryIndexes[0].KeyColumns.Count);
      Assert.IsTrue(Schema.Tables["A"].SecondaryIndexes[0].IsUnique);
      Assert.AreEqual(new TypeInfo(typeof (string), 125),
        Schema.Tables["A"].Columns["Col3"].Type);

      Assert.IsNotNull(Schema.Tables["B"]);
      Assert.IsNotNull(Schema.Tables["B"].PrimaryIndex);
      Assert.AreEqual(1, Schema.Tables["B"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(3, Schema.Tables["B"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, Schema.Tables["B"].SecondaryIndexes.Count);
      Assert.IsFalse(Schema.Tables["B"].SecondaryIndexes[0].IsUnique);
    }

    [Test]
    public virtual void IncludedColumnsTest()
    {
      Assert.AreEqual(2,
        Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count);
    }

    [Test]
    public void ForeignKeyTest()
    {
      Assert.AreEqual(1, Schema.Tables["B"].ForeignKeys.Count);
      Assert.AreEqual(Schema.Tables["A"].PrimaryIndex,
        Schema.Tables["B"].ForeignKeys[0].PrimaryKey);
    }

    [Test]
    public void TimeSpanColumnTest()
    {
      Assert.AreEqual(new TypeInfo(typeof(TimeSpan)),
        Schema.Tables["C"].Columns["Col1"].Type);
    }

    [Test]
    public void GeneratorsTest()
    {
      Assert.AreEqual(1, Schema.Sequences.Count);
    }
  }
}


#region Model

namespace Xtensive.Storage.Tests.Upgrade.ConvertDomainModel.Model
{

  [HierarchyRoot]
  [Index("Col1", "Col2", IsUnique = true, IncludedFields = new[] { "Col3" })]
  public class A : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public Guid Col2 { get; private set; }

    [Field(Length = 125)]
    public string Col3 { get; private set; }
  }

  [HierarchyRoot]
  [Index("ColA", MappingName = "A_IX")]
  public class B : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public A ColA { get; private set; }
  }

  [HierarchyRoot]
  public class C : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public TimeSpan Col1 { get; private set; }
  }

  [HierarchyRoot]
  public class D : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public EntitySet<E> ColE { get; private set; }
  }

  [HierarchyRoot]
  public class E : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public D ColD { get; private set; }
  }
}

#endregion