// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.22

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Linq;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Rse
{
  #region Implementation of DomainBuilder
  public class TypeModifier : IModule
  {
    public static bool IsEnabled;

    public static readonly TypeModifier Current = new TypeModifier();

    public InheritanceSchema InheritanceSchema { get; set; }

    public void OnBuilt(Domain domain)
    {}

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      Current.BuildReal(context, model);
    }

    public void BuildReal(BuildingContext context, DomainModelDef model)
    {
      model.Hierarchies[typeof (Product)].Schema = InheritanceSchema;
    }
  }
  #endregion
  [TestFixture, Category("Rse")]
  public class IndexOptimizerInheritanceTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      config.Types.Register(typeof(TypeModifier));
      return config;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    public override void TestFixtureSetUp()
    {
      TypeModifier.IsEnabled = true;
    }

    public override void TestFixtureTearDown()
    {
      TypeModifier.IsEnabled = false;
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
        index => CheckVirtualIndex(index, IndexAttributes.View));
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
      Test<ActiveProduct>(index => Assert.IsTrue(index.IsTyped),
        index => Assert.IsTrue(index.IsTyped));
    }

    private void Test<T>(Action<IndexInfo> secondaryIndexValidator, Action<IndexInfo> primaryIndexValidator)
      where T : Product
    {
      CreateDomain();

      Expression<Func<T, bool>> predicate = product => product.UnitPrice > 10 && product.UnitPrice < 70 
        || product.ProductName.GreaterThan("e") && product.ProductName.GreaterThan("t");
      var expected = Session.Demand().Query.All<T>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
      var query = Session.Demand().Query.All<T>().Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      var primaryIndex = Domain.Model.Types[typeof (T)].Indexes.PrimaryIndex;
      primaryIndexValidator(primaryIndex);
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
      Assert.AreEqual(expectedIndexType, index.Attributes & (IndexAttributes.View | IndexAttributes.Union | IndexAttributes.Join | IndexAttributes.Filtered | IndexAttributes.Typed));
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