// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class RangeSetExtractorCnfTest : BaseRangeSetExtractorTest
  {
    [Test]
    public void SimpleExpressionTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);

      /* Expression<Func<Tuple, bool>> exp =
         (t) => t.GetValue<int?>(cLengthIdx) >= 3 &&
                t.GetValue<int?>(cLengthIdx) < 6 ||
                10 <= t.GetValue<int?>(cLengthIdx); */

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<int?>(cLengthIdx) >= 3)
          .AddBoolean(t => t.GetValue<int?>(cLengthIdx) < 6))
        .AddCnf(AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));
      
      var expectedRanges = CreateExpectedRangesForSimpleTest(indexInfo, LengthField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForSimpleTest(IndexInfo indexInfo,
      string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      Entire<Tuple> expectedFirst = new Entire<Tuple>(
        CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 3));
      Entire<Tuple> expectedSecond = new Entire<Tuple>(
        CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 6), Direction.Negative);
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);

      expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 10));
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    public void DifferentFieldsTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);
      int cNameIdx = GetFieldIndex(rsHeader, NameField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<int?>(cLengthIdx) <= 3)
          .AddBoolean(t => t.GetValue<string>(cNameIdx)=="abc"))
        .AddCnf(AsCnf(t => t.GetValue<int?>(cLengthIdx) > 15));

      var expectedRanges = CreateExpectedRangesForDifferentFieldsTest(indexInfo, LengthField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForDifferentFieldsTest(IndexInfo indexInfo,
      string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      Entire<Tuple> expectedFirst = new Entire<Tuple>(InfinityType.Negative);
      Entire<Tuple> expectedSecond = new Entire<Tuple>(
        CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 3));
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);
      expectedFirst = new Entire<Tuple>(
        CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 15), Direction.Positive);
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    public void DifferentFieldsInSameComparisonTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(NameField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cDescriptionIdx = GetFieldIndex(rsHeader, LengthField);
      int cNameIdx = GetFieldIndex(rsHeader, NameField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<string>(cNameIdx)
          .CompareTo(t.GetValue<string>(cDescriptionIdx)) > 0))
        .AddCnf(AsCnf(t => t.GetValue<string>(cNameIdx) == "abc"));

      var expectedRanges = CreateRangesForDifferentFieldsInSameComparisonTest();
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateRangesForDifferentFieldsInSameComparisonTest()
    {
      return new SetSlim<Range<Entire<Tuple>>> {Range<Entire<Tuple>>.Full};
    }

    [Test]
    public void StandAloneBooleanExpressionsTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);

      int x = 1;
      int y = 2;
      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<int?>(cLengthIdx) <= 3)
          .AddBoolean(t => x + 3 > y))
        .AddCnf(AsCnf(t => t.GetValue<int?>(cLengthIdx) > 15))
        .AddCnf(AsCnf(t => false));

      var expectedRanges = CreateExpectedRangesForStandAloneBooleanExpressionsTest(indexInfo, LengthField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForStandAloneBooleanExpressionsTest(
      IndexInfo indexInfo, string keyFieldName)
    {
      return CreateExpectedRangesForDifferentFieldsTest(indexInfo, keyFieldName);
    }

    [Test]
    public void MultiColumnIndexTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);
      int cDescriptionIdx = GetFieldIndex(rsHeader, DescriptionField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<string>(cDescriptionIdx).CompareTo("abc") < 0)
        .AddBoolean(t => t.GetValue<int?>(cLengthIdx)==6))
        .AddCnf(AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));

      var expectedRanges = CreateExpectedRangesForMultiColumnIndexTest(indexInfo, LengthField, DescriptionField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForMultiColumnIndexTest(
      IndexInfo indexInfo, params string[] keyFieldName)
    {
      var keyFieldIndex0 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[0]);
      var keyFieldIndex1 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[1]);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var secondTuple = CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex0, 6);
      secondTuple.SetValue(keyFieldIndex1, "abc");
      Entire<Tuple> expectedFirst = new Entire<Tuple>(InfinityType.Negative);
      Entire<Tuple> expectedSecond = new Entire<Tuple>(secondTuple,
                                                       Direction.Negative);
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);

      expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex0, 10));
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }
  }
}