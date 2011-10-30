// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.28

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Storage.OverrideMetadataModel;

namespace Xtensive.Storage.Tests.Storage.OverrideMetadataModel
{
  [HierarchyRoot]
  public class RefTarget : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Uber : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 10, Nullable = false)]
    public virtual string StringField { get; set; }

    [Field(Scale = 4, Precision = 10)]
    public virtual decimal DecimalField { get; set; }

    [Field(Indexed = true)]
    public virtual int IndexedField { get; set; }

    [Field]
    public virtual int NotIndexedField { get; set; }

    [Field, Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Clear)]
    public virtual RefTarget MainTarget { get; set; }
  }

  public class Cool : Uber
  {
    [Field(Nullable = true)]
    public override string StringField { get; set; }

    [Field(Precision = 12)]
    public override decimal DecimalField { get; set; }

    [Field(Indexed = false)]
    public override int IndexedField { get; set; }

    [Field(Indexed = true)]
    public override int NotIndexedField { get; set; }

    [Field, Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public override RefTarget MainTarget { get; set; }
  }

  [HierarchyRoot]
  public class UberEntity : Uber
  {
    [Field(Nullable = true)]
    public override string StringField { get; set; }

    [Field(Precision = 8)]
    public override decimal DecimalField { get; set; }

    [Field(Indexed = false)]
    public override int IndexedField { get; set; }

    [Field(Indexed = true)]
    public override int NotIndexedField { get; set; }

    [Field, Association(OnOwnerRemove = OnRemoveAction.Deny)]
    public override RefTarget MainTarget { get; set; }
  }

  [HierarchyRoot]
  public class CoolEntity : Cool
  {
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class OverrideMetadata : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Uber).Assembly, typeof(Uber).Namespace);
      return config;
    }

    [Test]
    public void ValidateUberTest()
    {
      var type = Domain.Model.Types[typeof (UberEntity)];

      Assert.AreEqual(10, type.Fields["StringField"].Column.Length);
      Assert.IsTrue(type.Fields["StringField"].IsNullable);
      Assert.AreEqual(8, type.Fields["DecimalField"].Column.Precision);
      Assert.AreEqual(4, type.Fields["DecimalField"].Column.Scale);

      Assert.IsTrue((type.Fields["IndexedField"].Attributes & FieldAttributes.Indexed)==0);
      Assert.IsTrue((type.Fields["NotIndexedField"].Attributes & FieldAttributes.Indexed)!=0);

      Assert.AreEqual(OnRemoveAction.Deny, type.GetOwnerAssociations().Single().OnOwnerRemove);
    }

    [Test]
    public void ValidateCoolTest()
    {
      var type = Domain.Model.Types[typeof (CoolEntity)];

      Assert.AreEqual(10, type.Fields["StringField"].Column.Length);
      Assert.IsTrue(type.Fields["StringField"].IsNullable);
      Assert.AreEqual(12, type.Fields["DecimalField"].Column.Precision);
      Assert.AreEqual(4, type.Fields["DecimalField"].Column.Scale);

      Assert.IsTrue((type.Fields["IndexedField"].Attributes & FieldAttributes.Indexed) == 0);
      Assert.IsTrue((type.Fields["NotIndexedField"].Attributes & FieldAttributes.Indexed) != 0);

      Assert.AreEqual(OnRemoveAction.Cascade, type.GetOwnerAssociations().Single().OnOwnerRemove);
    }
  }
}