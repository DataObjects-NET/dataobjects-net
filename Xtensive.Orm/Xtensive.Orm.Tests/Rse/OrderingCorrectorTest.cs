// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Rse
{
  #region Implementation of DomainBuilder

  class SecondaryIndexRemover : IModule
  {
    public static bool IsEnabled;

    public void OnBuilt(Domain domain)
    {}

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      foreach (var type in model.Types) {
        var indexCache = new NodeCollection<IndexDef>(null, "IndexCache");
        indexCache.AddRange(from index in type.Indexes where index.IsPrimary select index);
        type.Indexes.Clear();
        type.Indexes.AddRange(indexCache);
      }
    }
  }

  #endregion

  [TestFixture]
  public class OrderingCorrectorTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Customer).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      SecondaryIndexRemover.IsEnabled = true;
      base.TestFixtureSetUp();
    }

    public override void TestFixtureTearDown()
    {
      SecondaryIndexRemover.IsEnabled = false;
      base.TestFixtureTearDown();
    }

    [Test]
    public void RemovingOfSortProviderTest()
    {
      var primaryIndex = Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex;
      var indexProvider = IndexProvider.Get(primaryIndex);
      var originalOrder = new DirectionCollection<int> {{10, Direction.Negative}};
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int> {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(GetColumnIndex(primaryIndex, "OrderDate"))!=null)
        .OrderBy(originalOrder)
        .Select(0, 2, 10);
      using (new DefaultEnumerationContext().Activate()) {
        var compiledProvider = new DefaultCompilationService().Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var compiledSort = ((SortProvider) ((SelectProvider) compiledProvider.Origin).Source);
        Assert.IsTrue(originalOrder.SequenceEqual(compiledSort.Order));
      }
    }

    [Test]
    public void RemovingOfSortWhenOrderBreakerIsPresentTest()
    {
      var primaryIndex = Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex;
      var indexProvider = IndexProvider.Get(primaryIndex);
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(GetColumnIndex(primaryIndex, "OrderDate"))!=null)
        .OrderBy(new DirectionCollection<int>{{10, Direction.Negative}})
        .Distinct()
        .Select(0, 2, 10);
      using (new DefaultEnumerationContext().Activate()) {
        var compiledProvider = new DefaultCompilationService().Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var lastSelect = (SelectProvider) compiledProvider.Origin;
        Assert.AreEqual(typeof (SortProvider), lastSelect.Source.GetType());
        var rootProvider = ((DistinctProvider) ((SortProvider) lastSelect.Source).Source).Source;
        Assert.AreEqual(typeof (FilterProvider), rootProvider.GetType());
      }
    }

    [Test]
    public void OrderColumnsRemoverIsPresentTest()
    {
      var primaryIndex = Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex;
      var indexProvider = IndexProvider.Get(primaryIndex);
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(GetColumnIndex(primaryIndex, "OrderDate"))!=null)
        .OrderBy(new DirectionCollection<int>{{10, Direction.Negative}})
        .Aggregate(new[] {2, 3})
        .Select(1);
      using (new DefaultEnumerationContext().Activate()) {
        var compiledProvider = new DefaultCompilationService().Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var root = (SelectProvider) compiledProvider.Origin;
        Assert.AreEqual(typeof (AggregateProvider), root.Source.GetType());
        var rootProvider = ((FilterProvider)((AggregateProvider)root.Source).Source).Source;
        Assert.AreEqual(typeof (SelectProvider), rootProvider.GetType());
      }
    }

    private static int GetColumnIndex(IndexInfo primaryIndex, string fieldName)
    {
      return primaryIndex.Columns
        .Where(c => c.Field.Name == fieldName)
        .Select(c => primaryIndex.Columns.IndexOf(c)).First();
    }
  }
}