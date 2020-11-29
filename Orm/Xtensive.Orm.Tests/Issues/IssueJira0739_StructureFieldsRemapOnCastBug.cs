// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.28

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0739_StructureFieldsRemapOnCastBugModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0739_StructureFieldsRemapOnCastBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Product).Assembly, typeof(Product).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var rpu = new RecipeProductUsage();
        rpu.Sequence = 1;
        rpu.Quantity.MeasureType = "A";
        rpu.Quantity.SomeStructureField.SomeStructureField1 = "ABC";
        rpu.Quantity.SomeStructureField.SomeStructureField2 = "DEF";
        rpu.Quantity.SomeStructureField.S2.SomeAnotherStructureField1 = "GHI";
        rpu.Quantity.SomeStructureField.S2.SomeAnotherStructureField2 = "JKL";

        rpu = new RecipeProductUsage();
        rpu.Sequence = 2;
        rpu.Quantity.MeasureType = "B";
        rpu.Quantity.SomeStructureField.SomeStructureField1 = "ABCC";
        rpu.Quantity.SomeStructureField.SomeStructureField2 = "DEFF";
        rpu.Quantity.SomeStructureField.S2.SomeAnotherStructureField1 = "GHII";
        rpu.Quantity.SomeStructureField.S2.SomeAnotherStructureField2 = "JKLL";

        transactionScope.Complete();
      }
    }

    [Test]
    public void CastToBaseClassTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<RecipeProductUsage>()
          .Cast<ProductUsage>()
          .Where(pu => pu.Quantity.SomeStructureField.SomeStructureField1.StartsWith("ABC"))
          .ToArray();

        Assert.That(result.Length, Is.EqualTo(2));

        var a = result.First(u => u.Sequence == 1);
        Assert.That(a.Quantity.MeasureType, Is.EqualTo("A"));
        Assert.That(a.Quantity.SomeStructureField.SomeStructureField1, Is.EqualTo("ABC"));
        Assert.That(a.Quantity.SomeStructureField.SomeStructureField2, Is.EqualTo("DEF"));
        Assert.That(a.Quantity.SomeStructureField.S2.SomeAnotherStructureField1, Is.EqualTo("GHI"));
        Assert.That(a.Quantity.SomeStructureField.S2.SomeAnotherStructureField2, Is.EqualTo("JKL"));

        var b = result.First(u => u.Sequence == 2);
        Assert.That(b.Quantity.MeasureType, Is.EqualTo("B"));
        Assert.That(b.Quantity.SomeStructureField.SomeStructureField1, Is.EqualTo("ABCC"));
        Assert.That(b.Quantity.SomeStructureField.SomeStructureField2, Is.EqualTo("DEFF"));
        Assert.That(b.Quantity.SomeStructureField.S2.SomeAnotherStructureField1, Is.EqualTo("GHII"));
        Assert.That(b.Quantity.SomeStructureField.S2.SomeAnotherStructureField2, Is.EqualTo("JKLL"));
      }
    }

    [Test]
    public void CastToInterfaceTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<RecipeProductUsage>()
          .Cast<IProductUsage>()
          .Where(pu => pu.Quantity.SomeStructureField.SomeStructureField1.StartsWith("ABC"))
          .ToArray();

        Assert.That(result.Length, Is.EqualTo(2));

        var a = result.First(u => u.Sequence == 1);
        Assert.That(a.Quantity.MeasureType, Is.EqualTo("A"));
        Assert.That(a.Quantity.SomeStructureField.SomeStructureField1, Is.EqualTo("ABC"));
        Assert.That(a.Quantity.SomeStructureField.SomeStructureField2, Is.EqualTo("DEF"));
        Assert.That(a.Quantity.SomeStructureField.S2.SomeAnotherStructureField1, Is.EqualTo("GHI"));
        Assert.That(a.Quantity.SomeStructureField.S2.SomeAnotherStructureField2, Is.EqualTo("JKL"));

        var b = result.First(u => u.Sequence == 2);
        Assert.That(b.Quantity.MeasureType, Is.EqualTo("B"));
        Assert.That(b.Quantity.SomeStructureField.SomeStructureField1, Is.EqualTo("ABCC"));
        Assert.That(b.Quantity.SomeStructureField.SomeStructureField2, Is.EqualTo("DEFF"));
        Assert.That(b.Quantity.SomeStructureField.S2.SomeAnotherStructureField1, Is.EqualTo("GHII"));
        Assert.That(b.Quantity.SomeStructureField.S2.SomeAnotherStructureField2, Is.EqualTo("JKLL"));
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0739_StructureFieldsRemapOnCastBugModel
{
  public interface IProductUsage : IEntity
  {
    [Field]
    float LossFactor { get; set; }

    [Field]
    int Sequence { get; set; }

    [Field]
    DimensionalField Quantity { get; set; }

    [Field]
    Product Product { get; }
  }

  public abstract class MesObject : Entity
  {
    [Key, Field]
    public long ID { get; set; }
  }

  [HierarchyRoot]
  public class Product : MesObject
  {
  }

  [HierarchyRoot]
  public abstract class ProductUsage : MesObject, IProductUsage
  {
    [Field]
    public Product Product { get; private set; }

    [Field]
    public DimensionalField Quantity { get; set; }

    [Field]
    public float LossFactor { get; set; }

    [Field]
    public int Sequence { get; set; }
  }

  public class RecipeProductUsage : ProductUsage
  {
    [Field]
    public string EngineeringReferenceCode { get; set; }
  }

  public class DimensionalField : Structure
  {
    [Field(DefaultValue = "", Length = 20, Nullable = false)]
    public string MeasureType { get; set; }

    [Field]
    public decimal NormalizedValue { get; set; }

    [Field(DefaultValue = 1.0)]
    public double LastUsedScale { get; set; }

    [Field]
    public SomeStructure SomeStructureField { get; set; }
  }

  public class SomeStructure : Structure
  {
    [Field]
    public SomeAnotherStructure S2 { get; set; }

    [Field]
    public string SomeStructureField1 { get; set; }

    [Field]
    public string SomeStructureField2 { get; set; }
  }

  public class SomeAnotherStructure : Structure
  {
    [Field]
    public string SomeAnotherStructureField1 { get; set; }

    [Field]
    public string SomeAnotherStructureField2 { get; set; }
  }
}
