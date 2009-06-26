// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Rse
{
  #region Implementation of DomainBuilder

  class SecondaryIndexRemover : IDomainBuilder
  {
    public void Build(BuildingContext context, DomainModelDef model)
    {
      foreach (var type in model.Types) {
        var indexCache = new NodeCollection<IndexDef>();
        indexCache.AddRange(from index in type.Indexes where index.IsPrimary select index);
        type.Indexes.Clear();
        type.Indexes.AddRange(indexCache);
      }
    }
  }

  #endregion

  [TestFixture, Category("Rse")]
  public class OrderingCorrectorTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Customer).Namespace);
      config.Builders.Add(typeof(SecondaryIndexRemover));
      return config;
    }

    [Test]
    public void RemovingOfSortProviderTest()
    {
      var primaryIndex = Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex;
      var indexProvider = IndexProvider.Get(primaryIndex);
      var originalOrder = new DirectionCollection<int> {{10, Direction.Negative}};
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(GetColumnIndex(primaryIndex, "OrderDate"))!=null)
        .OrderBy(originalOrder)
        .Select(0, 2, 10);
      using (EnumerationScope.Open()) {
        var compiledProvider = CompilationContext.Current.Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var compiledSort = ((SortProvider) ((SelectProvider) compiledProvider.Origin).Source);
        var selectAfterIndex = (SelectProvider) ((FilterProvider) compiledSort.Source).Source;
        var expectedOrder = from pair in originalOrder
        select
          new KeyValuePair<int, Direction>(
            selectAfterIndex.ColumnIndexes.IndexOf(pair.Key), pair.Value);
        Assert.IsTrue(expectedOrder.SequenceEqual(compiledSort.Order));
        Assert.AreEqual(typeof (IndexProvider), selectAfterIndex.Source.GetType());
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
      using (EnumerationScope.Open()) {
        var compiledProvider = CompilationContext.Current.Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var lastSelect = (SelectProvider) compiledProvider.Origin;
        Assert.AreEqual(typeof (DistinctProvider), lastSelect.Source.GetType());
        var selectAfterIndex = (SelectProvider) ((FilterProvider) ((DistinctProvider) lastSelect.Source)
          .Source).Source;
        Assert.AreEqual(typeof (IndexProvider), selectAfterIndex.Source.GetType());
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
      using (EnumerationScope.Open()) {
        var compiledProvider = CompilationContext.Current.Compile(result.Provider);
        Assert.Greater(compiledProvider.Count(), 0);
        var root = (SelectProvider) compiledProvider.Origin;
        Assert.AreEqual(typeof (AggregateProvider), root.Source.GetType());
        var selectAfterIndex = (SelectProvider) ((FilterProvider)((AggregateProvider)root.Source).Source)
          .Source;
        Assert.AreEqual(typeof (IndexProvider), selectAfterIndex.Source.GetType());
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