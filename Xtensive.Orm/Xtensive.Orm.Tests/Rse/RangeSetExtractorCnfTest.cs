// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Linq.Normalization;
using Xtensive.Helpers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Orm.Tests.Storage.SnakesModel;

namespace Xtensive.Orm.Tests.Rse
{
  [TestFixture, Category("Rse")]
  public class RangeSetExtractorCnfTest : BaseRangeSetExtractorTest
  {
    [Test]
    public void SimpleExpressionTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
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
      var tupleDescriptor = indexInfo.KeyTupleDescriptor.TrimFields(1);
      Entire<Tuple> expectedFirst = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, 3));
      Entire<Tuple> expectedSecond = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, 6), Direction.Negative);
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);

      expectedFirst = new Entire<Tuple>(CreateTuple(tupleDescriptor, keyFieldIndex, 10));
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    public void DifferentFieldsTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
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

    private static IEnumerable<Range<Entire<Tuple>>>
      CreateExpectedRangesForDifferentFieldsTest(IndexInfo indexInfo,
        string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var tupleDescriptor = indexInfo.KeyTupleDescriptor.TrimFields(1);
      Entire<Tuple> expectedFirst = new Entire<Tuple>(InfinityType.Negative);
      Entire<Tuple> expectedSecond = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, 3));
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);
      expectedFirst = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, 15), Direction.Positive);
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    public void DifferentFieldsInSameComparisonTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(NameField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cDescriptionIdx = GetFieldIndex(rsHeader, LengthField);
      int cNameIdx = GetFieldIndex(rsHeader, NameField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<string>(cNameIdx)
          .CompareTo(t.GetValue<string>(cDescriptionIdx)) > 0))
        .AddCnf(AsCnf(t => t.GetValue<string>(cNameIdx)=="abc"));

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
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
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
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);
      int cDescriptionIdx = GetFieldIndex(rsHeader, DescriptionField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<string>(cDescriptionIdx).CompareTo("abc") < 0)
          .AddBoolean(t => t.GetValue<int?>(cLengthIdx)==6))
        .AddCnf(AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));

      var expectedRanges = CreateExpectedRangesForMultiColumnIndexTest(indexInfo,
        LengthField, DescriptionField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForMultiColumnIndexTest(
      IndexInfo indexInfo, params string[] keyFieldName)
    {
      var keyFieldIndex0 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[0]);
      var keyFieldIndex1 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[1]);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var trimmedTupleDesc = indexInfo.KeyTupleDescriptor.TrimFields(1);
      var firstTuple = CreateTuple(trimmedTupleDesc, keyFieldIndex0, 6);
      var secondTuple = CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex0,
        firstTuple.GetValue(keyFieldIndex0));
      secondTuple.SetValue(keyFieldIndex1, "abc");
      Entire<Tuple> expectedFirst = new Entire<Tuple>(firstTuple, Direction.Negative);
      Entire<Tuple> expectedSecond = new Entire<Tuple>(secondTuple,
        Direction.Negative);
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);

      expectedFirst = new Entire<Tuple>(CreateTuple(trimmedTupleDesc, keyFieldIndex0, 10));
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    public void MultiColumnIndexWithFieldsInRandomOrderTest()
    {
      const string hireDateField = "HireDate";
      const string lastNameField = "LastName";
      const string titleField = "Title";
      TypeInfo emplType = Domain.Model.Types[typeof (Employee)];
      IndexInfo indexInfo = emplType.Indexes.GetIndex(hireDateField);
      RecordSetHeader rsHeader = emplType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int hireDateIdx = GetFieldIndex(rsHeader, hireDateField);
      int lastNameIdx = GetFieldIndex(rsHeader, lastNameField);
      int titleIdx = GetFieldIndex(rsHeader, titleField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<DateTime?>(hireDateIdx)==new DateTime(1990, 1, 1))
          .AddBoolean(t => t.GetValue<string>(titleIdx).GreaterThan("Sales Manager"))
          .AddBoolean(t => t.GetValue<string>(lastNameIdx)=="John"));

      var expectedRanges = CreateExpectedRangesForMultiColumnIndexWithFieldsInRandomOrderTest(indexInfo,
        hireDateField, lastNameField, titleField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>>
      CreateExpectedRangesForMultiColumnIndexWithFieldsInRandomOrderTest(
      IndexInfo indexInfo, params string[] keyFieldName)
    {
      var keyFieldIndex0 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[0]);
      var keyFieldIndex1 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[1]);
      var keyFieldIndex2 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[2]);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex0,
        new DateTime(1990, 1, 1)), Direction.Positive);
      expectedFirst.Value.SetValue(keyFieldIndex1, "John");
      expectedFirst.Value.SetValue(keyFieldIndex2, "Sales Manager");
      var expectedSecond = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor.TrimFields(2),
        keyFieldIndex0, expectedFirst.Value.GetValue(keyFieldIndex0)), Direction.Positive);
      expectedSecond.Value.SetValue(keyFieldIndex1, expectedFirst.Value.GetValue(keyFieldIndex1));
      var expectedRange = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange);
      return result;
    }

    [Test]
    public void MultiColumnIndexWithSeveralValuesOfNotLastFieldTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(LengthField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, LengthField);
      int cDescriptionIdx = GetFieldIndex(rsHeader, DescriptionField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<string>(cDescriptionIdx).CompareTo("a") > 0)
          .AddBoolean(t => t.GetValue<int?>(cLengthIdx)==6)
          .AddBoolean(t => t.GetValue<int?>(cLengthIdx)==8))
        .AddCnf(AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));

      var expectedRanges =
        CreateExpectedRangesForMultiColumnIndexWithSeveralValuesOfNotLastFieldTest(indexInfo, LengthField,
          DescriptionField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>>
      CreateExpectedRangesForMultiColumnIndexWithSeveralValuesOfNotLastFieldTest(
      IndexInfo indexInfo, params string[] keyFieldName)
    {
      var keyFieldIndex0 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[0]);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var tupleDescriptor = indexInfo.KeyTupleDescriptor.TrimFields(1);
      var expectedFirst = new Entire<Tuple>(CreateTuple(tupleDescriptor, keyFieldIndex0, 10));
      var expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange);
      return result;
    }

    [Test]
    public void MultiColumnIndexWithSeveralValuesOfLastFieldTest()
    {
      const string hireDateField = "HireDate";
      const string lastNameField = "LastName";
      const string titleField = "Title";
      TypeInfo emplType = Domain.Model.Types[typeof (Employee)];
      IndexInfo indexInfo = emplType.Indexes.GetIndex(hireDateField);
      RecordSetHeader rsHeader = emplType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int hireDateIdx = GetFieldIndex(rsHeader, hireDateField);
      int lastNameIdx = GetFieldIndex(rsHeader, lastNameField);
      int titleIdx = GetFieldIndex(rsHeader, titleField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => t.GetValue<DateTime?>(hireDateIdx)==new DateTime(1990, 1, 1))
          .AddBoolean(t => t.GetValue<string>(titleIdx).GreaterThan("Sales Manager"))
          .AddBoolean(t => t.GetValue<string>(lastNameIdx)=="John")
          .AddBoolean(t => t.GetValue<string>(titleIdx).LessThan("Y"))
          .AddBoolean(t => t.GetValue<string>(titleIdx).LessThanOrEqual("X")));


      var expectedRanges =
        CreateExpectedRangesForMultiColumnIndexWithSeveralValuesOfLastFieldTest(indexInfo, hireDateField,
          lastNameField, titleField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>>
      CreateExpectedRangesForMultiColumnIndexWithSeveralValuesOfLastFieldTest(IndexInfo indexInfo,
        params string[] keyFieldName)
    {
      var keyFieldIndex0 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[0]);
      var keyFieldIndex1 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[1]);
      var keyFieldIndex2 = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName[2]);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex0,
        new DateTime(1990, 1, 1)), Direction.Positive);
      expectedFirst.Value.SetValue(keyFieldIndex1, "John");
      expectedFirst.Value.SetValue(keyFieldIndex2, "Sales Manager");
      var expectedSecond = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor,
        keyFieldIndex0, expectedFirst.Value.GetValue(keyFieldIndex0)));
      expectedSecond.Value.SetValue(keyFieldIndex1, expectedFirst.Value.GetValue(keyFieldIndex1));
      expectedSecond.Value.SetValue(keyFieldIndex2, "X");
      var expectedRange = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange);
      return result;
    }

    [Test]
    public void LikeStartsWithTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(NameField);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int nameIdx = GetFieldIndex(rsHeader, NameField);

      var predicate = new DisjunctiveNormalized()
        .AddCnf(AsCnf(t => !t.GetValue<string>(nameIdx).StartsWith("abc")));

      var expectedRanges = CreateExpectedRangesForLikeStartsWithTestTest(indexInfo, NameField);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private static IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForLikeStartsWithTestTest(
      IndexInfo indexInfo, string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      var tupleDescriptor = indexInfo.KeyTupleDescriptor.TrimFields(1);
      const string keyValue = "abc";
      var expectedFirst = new Entire<Tuple>(InfinityType.Negative);
      var expectedSecond = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, keyValue), Direction.Negative);
      result.Add(new Range<Entire<Tuple>>(expectedFirst, expectedSecond));

      expectedFirst = new Entire<Tuple>(
        CreateTuple(tupleDescriptor, keyFieldIndex, keyValue
          + (indexInfo.Columns[keyFieldIndex].CultureInfo==null
            ? Xtensive.Comparison.WellKnown.OrdinalMaxChar
            : Xtensive.Comparison.WellKnown.CultureSensitiveMaxChar)),
        Direction.Positive);
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      result.Add(new Range<Entire<Tuple>>(expectedFirst, expectedSecond));
      return result;
    }
  }
}