// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class SecondaryIndexProvidersInserterTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void InsertingTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof (Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreatedExtractedRangeSets(creatureType);
      var providersTree = CreateTreeForInsertingTest(primaryIndex);
      var inserter = new SecondaryIndexProvidersInserter(Domain.Model);
      var modifiedTree = inserter.Insert(providersTree, extractedRangeSets);
      Assert.AreEqual(providersTree.GetType(), modifiedTree.GetType());
      Assert.IsTrue(((UnaryProvider)modifiedTree).Source is JoinProvider);
      var primaryIndexProvider = (IndexProvider)((UnaryProvider) ((UnaryProvider) providersTree).Source).Source;
      var joinProvider = (JoinProvider) ((DistinctProvider) modifiedTree).Source;
      Assert.AreEqual(primaryIndexProvider, joinProvider.Sources[1]);
      Assert.IsTrue(joinProvider.Sources[0] is DistinctProvider);
    }

    private static Dictionary<IndexInfo, RangeSetInfo> CreatedExtractedRangeSets(TypeInfo creatureType)
    {
      var secondaryIndex0 = creatureType.Indexes.Skip(2).First();
      var secondaryIndex1 = creatureType.Indexes.GetIndex("Name");
      return new Dictionary<IndexInfo, RangeSetInfo>
        {
          {
            secondaryIndex0, new RangeSetInfo(
              Expression.Constant(new RangeSet<Entire<Tuple>>(Range<Entire<Tuple>>.Full,
                AdvancedComparer<Entire<Tuple>>.Default)), null, false)
            },
          {
            secondaryIndex1, new RangeSetInfo(
              Expression.Constant(new RangeSet<Entire<Tuple>>(Range<Entire<Tuple>>.Full,
                AdvancedComparer<Entire<Tuple>>.Default)), null, false)
            }
        };
    }

    private static CompilableProvider CreateTreeForInsertingTest(IndexInfo primaryIndex)
    {
      CompilableProvider result = IndexProvider.Get(primaryIndex);
      result = new FilterProvider(result, t => t.GetValueOrDefault<int>(1) > 1
        && t.GetValueOrDefault<int>(1) < 3);
      return new SortProvider(result, new DirectionCollection<int>(0));
    }
  }
}