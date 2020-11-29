// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0740_EntitySetFieldsRemapOnCastBugModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0740_EntitySetFieldsRemapOnCastBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Product).Assembly, typeof(Product).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        _ = new DerivedProduct() { ProducibleItem = new ProducibleItem() { MeasureType = "A" } };
        _ = new DerivedProduct() { ProducibleItem = new ProducibleItem() { MeasureType = "M" } };
        _ = new DerivedProduct() { ProducibleItem = new ProducibleItem() { MeasureType = "B" } };
        _ = new DerivedProduct() { ProducibleItem = new ProducibleItem() { MeasureType = "M" } };
        _ = new DerivedProduct() { ProducibleItem = new ProducibleItem() { MeasureType = "C" } };

        _ = new IntermediateProduct() { MeasureType = "D" };
        _ = new IntermediateProduct() { MeasureType = "M" };
        _ = new IntermediateProduct() { MeasureType = "E" };
        _ = new IntermediateProduct() { MeasureType = "M" };
        _ = new IntermediateProduct() { MeasureType = "F" };
        _ = new IntermediateProduct() { MeasureType = "M" };
        _ = new IntermediateProduct() { MeasureType = "G" };

        transactionScope.Complete();
      }
    }

    [Test]
    public void CustomerCaseTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var derivedProducts = session.Query.All<DerivedProduct>().Where(p => p.ProducibleItem.MeasureType == "M");
        var intermediateProducts = session.Query.All<IntermediateProduct>().Where(p => p.MeasureType == "M");
        var allProducts = derivedProducts.Union<Product>(intermediateProducts).ToArray();

        Assert.That(allProducts.Length, Is.EqualTo(5));
        Assert.That(allProducts.OfType<DerivedProduct>().Count(), Is.EqualTo(2));
        Assert.That(allProducts.OfType<IntermediateProduct>().Count(), Is.EqualTo(3));
      }
    }

    [Test]
    public void CastToBaseClass01()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var derrivedProduct = session.Query.All<DerivedProduct>()
          .Where(p => p.ProducibleItem.MeasureType == "M")
          .Cast<Product>()
          .ToArray();
      }
    }

    [Test]
    public void CastToBaseClass02()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var derrivedProduct = session.Query.All<IntermediateProduct>()
          .Where(p => p.MeasureType == "M")
          .Cast<Product>()
          .ToArray();
      }
    }

    [Test]
    public void CastToInterface01()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var derrivedProduct = session.Query.All<DerivedProduct>()
          .Where(p => p.ProducibleItem.MeasureType == "M")
          .Cast<IProduct>()
          .ToArray();
      }
    }

    [Test]
    public void CastToInterface02()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var derrivedProduct = session.Query.All<IntermediateProduct>()
          .Where(p => p.MeasureType == "M")
          .Cast<IProduct>()
          .ToArray();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0740_EntitySetFieldsRemapOnCastBugModel
{
  public interface IProduct : IEntity
  {
    [Field]
    DateTime CreationDate { get; set; }

    [Field]
    EntitySet<ProductDestination> ProductDestinations { get; }

    [Field]
    int SequenceOrder { get; set; }

    [Field]
    string Name { get; set; }
  }

  public abstract class MesObject : Entity
  {
    [Field, Key]
    public long ID { get; set; }
  }

  [HierarchyRoot]
  public abstract class Product : MesObject, IProduct
  {
    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public int SequenceOrder { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<ProductDestination> ProductDestinations { get; set; }
  }

  public class DerivedProduct : Product
  {
    [Field]
    public ProducibleItem ProducibleItem { get; set; }
  }

  public class IntermediateProduct : Product
  {
    [Field(DefaultValue = "", Length = 20, Nullable = false)]
    public string MeasureType { get; set; }
  }

  [HierarchyRoot]
  public class ProducibleItem : MesObject
  {
    [Field(DefaultValue = "", Length = 20, Nullable = false)]
    public string MeasureType { get; set; }
  }

  [HierarchyRoot]
  public class ProductDestination : MesObject
  {
    [Field]
    public Product Product { get; set; }
  }
}