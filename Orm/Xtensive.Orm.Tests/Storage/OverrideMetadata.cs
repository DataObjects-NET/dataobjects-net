// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.28

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.OverrideMetadataModel;

namespace Xtensive.Orm.Tests.Storage.OverrideMetadataModel
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

namespace Xtensive.Orm.Tests.Storage
{
  public class OverrideMetadata : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(Uber).Assembly, typeof(Uber).Namespace);
      return config;
    }

    [Test]
    public void ValidateUberTest()
    {
      var type = Domain.Model.Types[typeof (UberEntity)];

      Assert.That(type.Fields["StringField"].Column.Length, Is.EqualTo(10));
      Assert.That(type.Fields["StringField"].IsNullable, Is.True);
      Assert.That(type.Fields["DecimalField"].Column.Precision, Is.EqualTo(8));
      Assert.That(type.Fields["DecimalField"].Column.Scale, Is.EqualTo(4));

      Assert.That((type.Fields["IndexedField"].Attributes & FieldAttributes.Indexed)==0, Is.True);
      Assert.That((type.Fields["NotIndexedField"].Attributes & FieldAttributes.Indexed)!=0, Is.True);

      Assert.That(type.GetOwnerAssociations().Single().OnOwnerRemove, Is.EqualTo(OnRemoveAction.Deny));
    }

    [Test]
    public void ValidateCoolTest()
    {
      var type = Domain.Model.Types[typeof (CoolEntity)];

      Assert.That(type.Fields["StringField"].Column.Length, Is.EqualTo(10));
      Assert.That(type.Fields["StringField"].IsNullable, Is.True);
      Assert.That(type.Fields["DecimalField"].Column.Precision, Is.EqualTo(12));
      Assert.That(type.Fields["DecimalField"].Column.Scale, Is.EqualTo(4));

      Assert.That((type.Fields["IndexedField"].Attributes & FieldAttributes.Indexed) == 0, Is.True);
      Assert.That((type.Fields["NotIndexedField"].Attributes & FieldAttributes.Indexed) != 0, Is.True);

      Assert.That(type.GetOwnerAssociations().Single().OnOwnerRemove, Is.EqualTo(OnRemoveAction.Cascade));
    }
  }
}