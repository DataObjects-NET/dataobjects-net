// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class OrderingCorrectorMemoryTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfiguration.Load("mssql2005");
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Customer).Namespace);
      return config;
    }

    [Test]
    public void RemovingOfSortProviderTest()
    {
      var indexProvider = IndexProvider.Get(Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex);
      var originalOrder = new DirectionCollection<int> {{10, Direction.Negative}};
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(7)!=null)
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
      var indexProvider = IndexProvider.Get(Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex);
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(6)!=null)
        .OrderBy(new DirectionCollection<int>{{10, Direction.Negative}})
        .Distinct()
        .Select(0, 2, 10);
      var compiledProvider = CompilationContext.Current.Compile(result.Provider);
      Assert.Greater(compiledProvider.Count(), 0);
      var lastSelect = (SelectProvider) compiledProvider.Origin;
      Assert.AreEqual(typeof(DistinctProvider), lastSelect.Source.GetType());
      var selectAfterIndex = (SelectProvider) ((FilterProvider) ((DistinctProvider) lastSelect.Source)
        .Source).Source;
      Assert.AreEqual(typeof (IndexProvider), selectAfterIndex.Source.GetType());
    }

    [Test]
    public void OrderColumnsRemoverIsPresentTest()
    {
      var indexProvider = IndexProvider.Get(Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex);
      var result = indexProvider.Result
        .OrderBy(new DirectionCollection<int>() {{2, Direction.Positive}})
        .Filter(t => t.GetValueOrDefault<DateTime?>(6)!=null)
        .OrderBy(new DirectionCollection<int>{{10, Direction.Negative}})
        .Aggregate(new[] {3});
      var compiledProvider = CompilationContext.Current.Compile(result.Provider);
      Assert.Greater(compiledProvider.Count(), 0);
      var root = (AggregateProvider) compiledProvider.Origin;
      Assert.AreEqual(typeof(FilterProvider), root.Source.GetType());
      var selectAfterIndex = (SelectProvider) ((FilterProvider)root.Source)
        .Source;
      Assert.AreEqual(typeof (IndexProvider), selectAfterIndex.Source.GetType());
    }
  }
}