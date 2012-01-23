// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.21

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0149_LinqTranslationError_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0149_LinqTranslationError_Model
{
  [HierarchyRoot]
  public class ProductInstance : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Product Product { get; set; }

    [Field]
    public bool IsWaybilled { get; set; }

    [Field]
    public bool IsChecked { get; set; }

    public static Expression<Func<ProductInstance, bool>> IsAvialableForSale()
    {
      return pi => !pi.IsChecked && !pi.IsWaybilled;
    }
  }

  [HierarchyRoot]
  public class Product : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public ProductModel ProductModel { get; set; }

    [Field]
    [Association(PairTo = "Product", OnOwnerRemove = OnRemoveAction.Deny, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<ProductInstance> InstanceSet { get; set; }
  }

  [HierarchyRoot]
  public class ProductModel : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "ProductModel", OnOwnerRemove = OnRemoveAction.Deny, OnTargetRemove = OnRemoveAction.Clear
      )]
    public EntitySet<Product> Products { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0149_LinqTranslationError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (ProductModel).Assembly, typeof (ProductModel).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (TransactionScope t = session.OpenTransaction()) {

          new ProductModel();
          session.SaveChanges();

          var model = session.Query.All<ProductModel>().FirstOrDefault();
//works
          session.Query.All<ProductInstance>()
            .Where(pi => pi.Product.ProductModel == model)
            .Where(ProductInstance.IsAvialableForSale())
            .Count();

//Works
          model.Products
            .SelectMany(p => p.InstanceSet.Where(pi => !pi.IsChecked && !pi.IsWaybilled))
            .Count();
//don't works
          model.Products
            .SelectMany(p => p.InstanceSet.Where(ProductInstance.IsAvialableForSale()))
            .Count();
        }
      }
    }
  }
}