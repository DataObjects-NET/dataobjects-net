// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Orm.Tests.Storage.SnakesModel;

namespace Xtensive.Orm.Tests.Rse
{
  [TestFixture, Category("Rse")]
  public class ProviderTreeRewriterTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Memory);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void InsertSecondaryIndexesOnlyTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof (Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreateRangeSetsForSecondary(creatureType);
      var providersTree = CreateTreeForInsertSecondaryIndexesOnlyTest(primaryIndex);
      var inserter = new ProviderTreeRewriter(Domain.Model);
      var modifiedTree = inserter.InsertRangeProviders(providersTree, extractedRangeSets);

      Assert.AreEqual(providersTree.GetType(), modifiedTree.GetType());
      var joinProvider = (JoinProvider) ((SelectProvider) ((FilterProvider) ((UnaryProvider) modifiedTree)
        .Source).Source).Source;
      var primaryIndexProvider = (IndexProvider)((UnaryProvider) ((UnaryProvider) providersTree)
        .Source).Source;
      Assert.AreEqual(primaryIndexProvider, joinProvider.Left);
      Assert.IsTrue(((AliasProvider)joinProvider.Right).Source is UnionProvider);
    }

    private static CompilableProvider CreateTreeForInsertSecondaryIndexesOnlyTest(IndexInfo primaryIndex)
    {
      CompilableProvider result = IndexProvider.Get(primaryIndex);
      result = new FilterProvider(result, t => t.GetValueOrDefault<int>(1) > 1
        && t.GetValueOrDefault<int>(1) < 3);
      return new SortProvider(result, new DirectionCollection<int>(0));
    }

    private static Dictionary<IndexInfo, RangeSetInfo> CreateRangeSetsForSecondary(TypeInfo creatureType)
    {
      //var secondaryIndex0 = creatureType.Indexes.Skip(2).First();
      var secondaryIndex0 = creatureType.Indexes.GetIndex("Name");
      var secondaryIndex1 = creatureType.Indexes.GetIndex("Name", "AlsoKnownAs");
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

    [Test]
    public void InsertPrimaryIndexOnlyTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof (Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreateRangeSetsForPrimary(creatureType);
      var providersTree = CreateTreeForInsertSecondaryIndexesOnlyTest(primaryIndex);
      var inserter = new ProviderTreeRewriter(Domain.Model);
      var modifiedTree = inserter.InsertRangeProviders(providersTree, extractedRangeSets);

      Assert.AreEqual(providersTree.GetType(), modifiedTree.GetType());
      var rangeSetProvider = ((FilterProvider) ((SortProvider) modifiedTree)
        .Source).Source;
      CheckPrimaryIndex(primaryIndex, rangeSetProvider);
    }

    private void CheckPrimaryIndex(IndexInfo primaryIndex, Provider rangeSetProvider)
    {
      Assert.AreEqual(typeof (RangeSetProvider), rangeSetProvider.GetType());
      Assert.AreEqual(typeof (IndexProvider), ((RangeSetProvider) rangeSetProvider).Source.GetType());
      Assert.AreSame(primaryIndex,
        ((IndexProvider)((RangeSetProvider) rangeSetProvider).Source).Index.Resolve(Domain.Model));
    }

    private static Dictionary<IndexInfo, RangeSetInfo> CreateRangeSetsForPrimary(TypeInfo creatureType)
    {
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      return new Dictionary<IndexInfo, RangeSetInfo>
        {
          {
            primaryIndex, new RangeSetInfo(
              Expression.Constant(new RangeSet<Entire<Tuple>>(Range<Entire<Tuple>>.Full,
                AdvancedComparer<Entire<Tuple>>.Default)), null, false)
            }
        };
    }

    [Test]
    public void InsertPrimaryIndexAndSecondaryTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof (Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreateRangeSetsForPrimaryAndSecondary(creatureType);
      var providersTree = CreateTreeForInsertSecondaryIndexesOnlyTest(primaryIndex);
      var inserter = new ProviderTreeRewriter(Domain.Model);
      var modifiedTree = inserter.InsertRangeProviders(providersTree, extractedRangeSets);

      Assert.AreEqual(providersTree.GetType(), modifiedTree.GetType());
      Assert.AreEqual(typeof (SelectProvider),
        ((FilterProvider)((SortProvider) modifiedTree).Source).Source.GetType());
      var joinProvider = (JoinProvider)((SelectProvider)((FilterProvider)((SortProvider) modifiedTree).Source)
        .Source).Source;
      CheckPrimaryIndex(primaryIndex, joinProvider.Left);
      Assert.AreEqual(typeof(UnionProvider), ((AliasProvider)joinProvider.Right).Source.GetType());
    }

    private static Dictionary<IndexInfo, RangeSetInfo> CreateRangeSetsForPrimaryAndSecondary(
      TypeInfo creatureType)
    {
      var result = new Dictionary<IndexInfo, RangeSetInfo>(CreateRangeSetsForSecondary(creatureType));
      foreach (var primary in CreateRangeSetsForPrimary(creatureType))
        result.Add(primary.Key, primary.Value);
      return result;
    }

    [Test]
    public void DoNotModifyTest()
    {
      TypeInfo creatureType = Domain.Model.Types[typeof(Creature)];
      var primaryIndex = creatureType.Indexes.PrimaryIndex;
      var extractedRangeSets = CreateRangeSetsForSecondary(creatureType);
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
  }
}