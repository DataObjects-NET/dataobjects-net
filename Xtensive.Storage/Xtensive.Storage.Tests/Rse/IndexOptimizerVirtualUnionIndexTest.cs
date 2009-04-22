// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.22

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Rse
{
  #region Implementation of DomainBuilder
  class TypeModifier : IDomainBuilder
  {
    public void Build(BuildingContext context, DomainModelDef model)
    {
      model.Hierarchies[typeof (Product)].Schema=InheritanceSchema.ConcreteTable;
    }
  }
  #endregion
  [TestFixture]
  public class IndexOptimizerVirtualUnionIndexTest : IndexOptimizerTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfiguration.Load("memory");
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      config.Builders.Add(typeof(TypeModifier));
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      Expression<Func<Product, bool>> predicate = product => product.UnitPrice > 10 && product.UnitPrice < 70 
        || product.ProductName.GreaterThan("e") && product.ProductName.GreaterThan("t");
      var expected = Query<Product>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(o => o.Id);
      var query = Query<Product>.All.Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      var unitPriceIndex = Domain.Model.Types[typeof(Product)].Indexes.GetIndex("UnitPrice");
      CheckIndexIsVirtualUnion(unitPriceIndex);
      var productNameIndex = Domain.Model.Types[typeof(Product)].Indexes.GetIndex("ProductName");
      CheckIndexIsVirtualUnion(productNameIndex);
      ValidateUsedIndex(query, unitPriceIndex, productNameIndex);
      ValidateQueryResult(expected, actual);
    }

    private static void CheckIndexIsVirtualUnion(IndexInfo index)
    {
      Assert.IsTrue(index.IsVirtual);
      Assert.AreNotEqual(0, IndexAttributes.Union & index.Attributes);
    }
  }
}