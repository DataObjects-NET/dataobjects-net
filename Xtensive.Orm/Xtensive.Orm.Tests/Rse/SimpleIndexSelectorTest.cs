// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using Xtensive.Orm.Tests.Storage.SnakesModel;

namespace Xtensive.Orm.Tests.Rse
{
  [TestFixture, Category("Rse")]
  public class SimpleIndexSelectorTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void NonFullRangeSetsTest()
    {
      TestSelector(CreateRangeSet, ValidateResultOfNonFullRangeSetsTest);
    }

    private static RangeSet<Entire<Tuple>> CreateRangeSet(Random rnd)
    {
      return new RangeSet<Entire<Tuple>>(
          new Range<Entire<Tuple>>(
          Tuple.Create(rnd.Next(), rnd.Next()),
          Tuple.Create(rnd.Next(), rnd.Next())),
          AdvancedComparer<Entire<Tuple>>.Default);
    }

    private static void ValidateResultOfNonFullRangeSetsTest(IndexInfo[] indexes, Expression[] exps,
      Dictionary<Expression, List<RsExtractionResult>> inputData,
      Dictionary<IndexInfo, RangeSetInfo> selectedIndexes)
    {
      Assert.AreEqual(2, selectedIndexes.Count);
      Assert.AreEqual(inputData[exps[0]][2].RangeSetInfo, selectedIndexes[indexes[2]]);
      Assert.AreEqual(inputData[exps[1]][1].RangeSetInfo, selectedIndexes[indexes[1]]);
      var uniteMethod = typeof(RangeSet<Entire<Tuple>>).GetMethod("Unite");
      Assert.AreEqual(uniteMethod,
        ((MethodCallExpression)selectedIndexes[indexes[1]].Source).Method);
      Assert.AreEqual(inputData[exps[2]][1].RangeSetInfo.Source,
        ((MethodCallExpression)selectedIndexes[indexes[1]].Source).Arguments[0]);
    }

    [Test]
    public void FullRangeSetsTest()
    {
      TestSelector(CreateFullRangeSet,
        (indexes, exps, inputData, selectedIndexes) => Assert.AreEqual(0, selectedIndexes.Count));
    }

    [Test]
    public void PrimaryIndexIsSelectedTest()
    {
      TestSelector(CreateRangeSet,
        (indexes, exps, inputData, selectedIndexes) => Assert.AreEqual(2, selectedIndexes.Count), true);
    }

    private void TestSelector(Func<Random, RangeSet<Entire<Tuple>>> rangeSetCreator,
      Action<IndexInfo[], Expression[], Dictionary<Expression, List<RsExtractionResult>>,
        Dictionary<IndexInfo, RangeSetInfo>> resultValidator)
    {
      TestSelector(rangeSetCreator, resultValidator, false);
    }

    private void TestSelector(Func<Random, RangeSet<Entire<Tuple>>> rangeSetCreator,
      Action<IndexInfo[], Expression[], Dictionary<Expression, List<RsExtractionResult>>,
        Dictionary<IndexInfo, RangeSetInfo>> resultValidator, bool selectPrimaryForExp0)
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(Creature)];
      var indexes = new[]
        {
          snakeType.Indexes[0], snakeType.Indexes[1], Domain.Model.Types[typeof (ClearSnake)].Indexes[0]
        };
      Expression[] exps = new[] { Expression.Constant(0), Expression.Constant(1), Expression.Constant(2) };
      var inputData = CreateInputData(exps, indexes, rangeSetCreator);
      var costEvaluator = ConfigureCostEvaluator(exps, inputData, selectPrimaryForExp0);
      var selector = new SimpleIndexSelector(costEvaluator);
      var selectedIndexes = selector.Select(inputData);
      resultValidator(indexes, exps, inputData, selectedIndexes);
    }

    private static RangeSet<Entire<Tuple>> CreateFullRangeSet(Random rnd)
    {
      return new RangeSet<Entire<Tuple>>(Range<Entire<Tuple>>.Full, AdvancedComparer<Entire<Tuple>>.Default);
    }

    private Dictionary<Expression, List<RsExtractionResult>> CreateInputData(Expression[] exps,
      IndexInfo[] indexes, Func<Random, RangeSet<Entire<Tuple>>> rangeSetCreator)
    {
      var result = new Dictionary<Expression, List<RsExtractionResult>>();
      var rnd = new Random();
      foreach (var exp in exps) {
        result.Add(exp, new List<RsExtractionResult>());
        foreach (var index in indexes) {
          result[exp].Add(new RsExtractionResult(index,
            new RangeSetInfo(Expression.Constant(rangeSetCreator(rnd)), null, false)));
        }
      }
      return result;
    }

    private static ICostEvaluator ConfigureCostEvaluator(Expression[] exps,
      Dictionary<Expression, List<RsExtractionResult>> inputData, bool selectPrimaryForExp0)
    {
      var mocks = new MockRepository();
      var result = mocks.Stub<ICostEvaluator>();
      int index = 0;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet()))
        .Return(new CostInfo(selectPrimaryForExp0 ? 1 : 100, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(110.0, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(120.0, 0));
      index = 1;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(100.0, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(5.0, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(5.0, 0));
      index = 2;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(5.0, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(110.0, 0));
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(new CostInfo(120.0, 0));
      mocks.ReplayAll();
      return result;
    }
  }
}