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
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class ProviderTreeRewriterTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void InsertTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof (Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreatedExtractedRangeSets(creatureType);
      var providersTree = CreateTreeForInsertTest(primaryIndex);
      var inserter = new ProviderTreeRewriter(Domain.Model);
      var modifiedTree = inserter.InsertRangeProviders(providersTree, extractedRangeSets);

      Assert.AreEqual(providersTree.GetType(), modifiedTree.GetType());
      var joinProvider = (JoinProvider)((SelectProvider)((FilterProvider) ((UnaryProvider) modifiedTree)
        .Source).Source).Source;
      var primaryIndexProvider = (IndexProvider)((UnaryProvider) ((UnaryProvider) providersTree)
        .Source).Source;
      Assert.AreEqual(primaryIndexProvider, joinProvider.Left);
      Assert.IsTrue(((AliasProvider)joinProvider.Right).Source is UnionProvider);
    }

    private static CompilableProvider CreateTreeForInsertTest(IndexInfo primaryIndex)
    {
      CompilableProvider result = IndexProvider.Get(primaryIndex);
      result = new FilterProvider(result, t => t.GetValueOrDefault<int>(1) > 1
        && t.GetValueOrDefault<int>(1) < 3);
      return new SortProvider(result, new DirectionCollection<int>(0));
    }

    [Test]
    public void DoNotModifyTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof(Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreatedExtractedRangeSets(creatureType);
      var providersTree = CreateTreeForForDoNotModifyTest(primaryIndex);
      var inserter = new ProviderTreeRewriter(Domain.Model);
      var nonModifiedTree = inserter.InsertRangeProviders(providersTree, extractedRangeSets);
      Assert.AreEqual(providersTree, nonModifiedTree);
    }

    private static CompilableProvider CreateTreeForForDoNotModifyTest(IndexInfo primaryIndex)
    {
      CompilableProvider result = IndexProvider.Get(primaryIndex);
      result = new RangeProvider(result, Range<Entire<Tuple>>.Full);
      return new SortProvider(result, new DirectionCollection<int>(0));
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
  }
}