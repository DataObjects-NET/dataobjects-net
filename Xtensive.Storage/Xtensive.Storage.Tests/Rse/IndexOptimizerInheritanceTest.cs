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
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Rse
{
  #region Implementation of DomainBuilder
  class TypeModifier : IDomainBuilder
  {
    public static readonly TypeModifier Current = new TypeModifier();

    public InheritanceSchema InheritanceSchema { get; set; }

    public void Build(BuildingContext context, DomainModelDef model)
    {
      Current.BuildReal(context, model);
    }

    public void BuildReal(BuildingContext context, DomainModelDef model)
    {
      model.Hierarchies[typeof (Product)].Schema = InheritanceSchema;
    }
  }
  #endregion
  [TestFixture]
  public class IndexOptimizerInheritanceTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfiguration.Load("memory");
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      config.Builders.Add(typeof(TypeModifier));
      return config;
    }

    public override void TestFixtureSetUp()
    {
    }

    public override void TestFixtureTearDown()
    {
    }

    public override void SetUp()
    {
    }

    public override void TearDown()
    {
    }

    [Test]
    public void SingleTableSchemaTest()
    {
      TypeModifier.Current.InheritanceSchema = InheritanceSchema.SingleTable;
      Test<Product>(index => Assert.IsFalse(index.IsVirtual),
        index => Assert.IsFalse(index.IsVirtual));
      Test<ActiveProduct>(index => CheckVirtualIndex(index, IndexAttributes.Filtered),
        index => CheckVirtualIndex(index, IndexAttributes.Filtered));
    }

    [Test]
    public void ClassTableSchemaTest()
    {
      TypeModifier.Current.InheritanceSchema = InheritanceSchema.ClassTable;
      Test<Product>(index => Assert.IsFalse(index.IsVirtual),
        index => Assert.IsFalse(index.IsVirtual));
      Test<ActiveProduct>(index => CheckVirtualIndex(index, IndexAttributes.Filtered),
        index => CheckVirtualIndex(index, IndexAttributes.Join));
    }

    [Test]
    public void ConcreteTableSchemaTest()
    {
      TypeModifier.Current.InheritanceSchema = InheritanceSchema.ConcreteTable;
      Test<Product>(index => CheckVirtualIndex(index, IndexAttributes.Union),
        index => CheckVirtualIndex(index, IndexAttributes.Union));
      Test<ActiveProduct>(index => Assert.IsFalse(index.IsVirtual),
        index => Assert.IsFalse(index.IsVirtual));
    }

    private void Test<T>(Action<IndexInfo> secondaryIndexValidator, Action<IndexInfo> primaryIndexValidator)
      where T : Product
    {
      CreateDomain();

      Expression<Func<T, bool>> predicate = product => product.UnitPrice > 10 && product.UnitPrice < 70 
        || product.ProductName.GreaterThan("e") && product.ProductName.GreaterThan("t");
      var expected = Query<T>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(o => o.Id);
      var query = Query<T>.All.Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      primaryIndexValidator(Domain.Model.Types[typeof (T)].Indexes.GetIndexesContainingAllData()
        .Single(index => index.IsPrimary));
      var unitPriceIndex = Domain.Model.Types[typeof(T)].Indexes.GetIndex("UnitPrice");
      secondaryIndexValidator(unitPriceIndex);
      var productNameIndex = Domain.Model.Types[typeof(T)].Indexes.GetIndex("ProductName");
      secondaryIndexValidator(productNameIndex);
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model, unitPriceIndex, productNameIndex);
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);

      DisposeDomain();
    }

    private static void CheckVirtualIndex(IndexInfo index, IndexAttributes expectedIndexType)
    {
      Assert.IsTrue(index.IsVirtual);
      Assert.AreEqual(expectedIndexType, expectedIndexType & index.Attributes);
    }

    private void CreateDomain()
    {
      base.TestFixtureSetUp();
      base.SetUp();
    }

    private void DisposeDomain()
    {
      base.TearDown();
      base.TestFixtureTearDown();
    }
  }
}