// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Index;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class BaseRangeSetExtractorTest : AutoBuildTest
  {
    protected string IdField { get; set; }
    protected string NameField { get; set; }
    protected string LengthField { get; set; }
    protected string FeaturesField { get; set; }
    protected string DescriptionField { get; set; }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      config.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Storage.Tests.ObjectModel.NorthwindDO");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var result = base.BuildDomain(configuration);

      Xtensive.Storage.Model.FieldInfo field;
      field = result.Model.Types[typeof(Creature)].Fields["ID"];
      IdField = field.Column.Name;
      field = result.Model.Types[typeof(Creature)].Fields["Name"];
      NameField = field.Column.Name;
      field = result.Model.Types[typeof(Snake)].Fields["Length"];
      LengthField = field.Column.Name;
      field = result.Model.Types[typeof(Snake)].Fields["Features"];
      FeaturesField = field.Column.Name;
      field = result.Model.Types[typeof(ClearSnake)].Fields["Description"];
      DescriptionField = field.Column.Name;
      return result;
    }

    protected static Conjunction<Expression> AsCnf(Expression<Func<Tuple, bool>> exp)
    {
      var result = new Conjunction<Expression>();
      result.Operands.Add(exp.Body);
      return result;
    }

    protected void TestExpression(DisjunctiveNormalized predicate, IndexInfo indexInfo,
      RecordSetHeader primaryIndexRsHeader, IEnumerable<Range<Entire<Tuple>>> expectedRanges)
    {
      TestExpression(expectedRanges,
        extractor => extractor.Extract(predicate, new[] {indexInfo}, primaryIndexRsHeader));
    }

    protected void TestExpression(Expression predicate, IndexInfo indexInfo,
      RecordSetHeader primaryIndexRsHeader, IEnumerable<Range<Entire<Tuple>>> expectedRanges)
    {
      TestExpression(expectedRanges,
        extractor => extractor.Extract(predicate, new[]{indexInfo}, primaryIndexRsHeader));
    }

    private void TestExpression(IEnumerable<Range<Entire<Tuple>>> expectedRanges,
      Func<RangeSetExtractor, Dictionary<Expression, List<RSExtractionResult>>> extractingFunc)
    {
      var extractor = new RangeSetExtractor(Domain.Model,
        new OptimizationInfoProviderResolver((DomainHandler)Domain.Handler));
      var rangeSetExp = extractingFunc(extractor);
      var result = rangeSetExp.GetRangeSetForSingleIndex();
      CheckRanges(expectedRanges, result);
    }

    protected static void CheckRanges(IEnumerable<Range<Entire<Tuple>>> expected,
      IEnumerable<Range<Entire<Tuple>>> actual)
    {
      Assert.AreEqual(expected.Count(), actual.Count());
      foreach (var range in expected) {
        // ReSharper disable AccessToModifiedClosure
        actual.Single(r => range.CompareTo(r) == 0);
        // ReSharper restore AccessToModifiedClosure
      }
    }

    protected static Tuple CreateTuple(TupleDescriptor descriptor, int fieldIndex, object fieldValue)
    {
      Tuple result = Tuple.Create(descriptor);
      result.SetValue(fieldIndex, fieldValue);
      return result;
    }

    protected static int GetFieldIndex(RecordSetHeader rsHeader, string fieldName)
    {
      return rsHeader.IndexOf(fieldName);
    }
  }
}