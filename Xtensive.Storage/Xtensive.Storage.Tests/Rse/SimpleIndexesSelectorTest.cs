// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class SimpleIndexesSelectorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(Creature)];
      var indexes = new[]
        {
          snakeType.Indexes[0], snakeType.Indexes[1], Domain.Model.Types[typeof (ClearSnake)].Indexes[0]
        };
      Expression[] exps = new[] { Expression.Constant(0), Expression.Constant(1), Expression.Constant(2) };
      var inputData = CreateInputData(exps, indexes);
      var costEvaluator = ConfigureCostEvaluator(exps, inputData);
      var selector = new SimpleIndexesSelector(costEvaluator);
      var selectedIndexes = selector.Select(inputData);
      Assert.AreEqual(2, selectedIndexes.Count);
      Assert.AreEqual(inputData[exps[0]][0].RangeSetInfo, selectedIndexes[indexes[0]]);
      Assert.AreEqual(inputData[exps[1]][1].RangeSetInfo, selectedIndexes[indexes[1]]);
      var uniteMethod = typeof (RangeSet<Entire<Tuple>>).GetMethod("Unite");
      Assert.AreEqual(uniteMethod,
        ((MethodCallExpression)selectedIndexes[indexes[1]].Source).Method);
      Assert.AreEqual(inputData[exps[2]][1].RangeSetInfo.Source,
        ((MethodCallExpression)selectedIndexes[indexes[1]].Source).Arguments[0]);
    }

    private RangeSet<Entire<Tuple>> CreateRangeSet()
    {
      return new RangeSet<Entire<Tuple>>(Range<Entire<Tuple>>.Full,
        AdvancedComparer<Entire<Tuple>>.Default);
    }

    private Dictionary<Expression, List<RsExtractionResult>> CreateInputData(Expression[] exps,
      IndexInfo[] indexes)
    {
      var result = new Dictionary<Expression, List<RsExtractionResult>>();
      foreach (var exp in exps) {
        result.Add(exp, new List<RsExtractionResult>());
        foreach (var index in indexes) {
          result[exp].Add(new RsExtractionResult(index,
            new RangeSetInfo(Expression.Constant(CreateRangeSet()), null, false)));
        }
      }
      return result;
    }

    private static ICostEvaluator ConfigureCostEvaluator(Expression[] exps,
      Dictionary<Expression, List<RsExtractionResult>> inputData)
    {
      var mocks = new MockRepository();
      var result = mocks.Stub<ICostEvaluator>();
      int index = 0;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet())).Return(5.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(100.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(110.0);
      index = 1;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet())).Return(100.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(5.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(5.0);
      index = 2;
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[0]][index].RangeSetInfo.GetRangeSet())).Return(100.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[1]][index].RangeSetInfo.GetRangeSet())).Return(110.0);
      SetupResult.For(result.Evaluate(inputData[exps[0]][index].IndexInfo,
        inputData[exps[2]][index].RangeSetInfo.GetRangeSet())).Return(120.0);
      mocks.ReplayAll();
      return result;
    }
  }
}