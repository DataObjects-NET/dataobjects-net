// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Optimisation;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Rse
{

  [TestFixture]
  public class RangeSetExtractorTest : AutoBuildTest
  {
    private string cID;
    private string cName;
    private string cLength;
    private string cFeatures;
    private string cDescription;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);

      Xtensive.Storage.Model.FieldInfo field;
      field = result.Model.Types[typeof(Creature)].Fields["ID"];
      cID = field.Column.Name;
      field = result.Model.Types[typeof(Creature)].Fields["Name"];
      cName = field.Column.Name;
      field = result.Model.Types[typeof(Snake)].Fields["Length"];
      cLength = field.Column.Name;
      field = result.Model.Types[typeof(Snake)].Fields["Features"];
      cFeatures = field.Column.Name;
      field = result.Model.Types[typeof (ClearSnake)].Fields["Description"];
      cDescription = field.Column.Name;
      return result;
    }

    [Test]
    public void SimpleExpressionTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(cLength);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, cLength);

      /*Expression<Func<Tuple, bool>> exp =
        (t) => t.GetValue<int?>(cLengthIdx) >= 3 &&
               t.GetValue<int?>(cLengthIdx) < 6 ||
               10 <= t.GetValue<int?>(cLengthIdx);*/

      var predicate = GetRootDnf();
      AddCnf(predicate, AddBoolean(AsCnf(t => t.GetValue<int?>(cLengthIdx) >= 3),
                                   t => t.GetValue<int?>(cLengthIdx) < 6));
      AddCnf(predicate, AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));
      
      var expectedRanges = CreateExpectedRangesForSimpleTest(indexInfo, cLength);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForSimpleTest(IndexInfo indexInfo,
      string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      Entire<Tuple> expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 3));
      Entire<Tuple> expectedSecond = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 6),
                                                       Direction.Negative);
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
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(cLength);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, cLength);
      int cNameIdx = GetFieldIndex(rsHeader, cName);

      var predicate = GetRootDnf();
      AddCnf(predicate, AddBoolean(AsCnf(t => t.GetValue<int?>(cLengthIdx) <= 3),
                                   t => t.GetValue<string>(cNameIdx) == "abc"));
      AddCnf(predicate, AsCnf(t => t.GetValue<int?>(cLengthIdx) > 15));

      var expectedRanges = CreateExpectedRangesForDifferentFieldsTest(indexInfo, cLength);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForDifferentFieldsTest(IndexInfo indexInfo,
      string keyFieldName)
    {
      var keyFieldIndex = indexInfo.GetRecordSetHeader().IndexOf(keyFieldName);
      var result = new SetSlim<Range<Entire<Tuple>>>();
      Entire<Tuple> expectedFirst = new Entire<Tuple>(InfinityType.Negative);
      Entire<Tuple> expectedSecond = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 3));
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange0);
      expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, keyFieldIndex, 15),
                                                       Direction.Positive);
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Add(expectedRange1);
      return result;
    }

    [Test]
    [Ignore]
    public void StandAloneBooleanExpressionsTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(cLength);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, cLength);

      int x = 1;
      int y = 2;
      var predicate = GetRootDnf();
      
      AddCnf(predicate, AddBoolean(AsCnf(t => t.GetValue<int?>(cLengthIdx) <= 3),
                                   (t) => x + 3 > y));
      AddCnf(predicate, AsCnf(t => t.GetValue<int?>(cLengthIdx) > 15));
      AddCnf(predicate, AsCnf((t) => false));

      var expectedRanges = CreateExpectedRangesForStandAloneBooleanExpressionsTest(indexInfo, cLength);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForStandAloneBooleanExpressionsTest(
      IndexInfo indexInfo, string keyFieldName)
    {
      return CreateExpectedRangesForDifferentFieldsTest(indexInfo, keyFieldName);
    }

    [Test]
    public void MultiColumnIndexTest()
    {
      TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex(cLength);
      RecordSetHeader rsHeader = snakeType.Indexes.PrimaryIndex.GetRecordSetHeader();
      int cLengthIdx = GetFieldIndex(rsHeader, cLength);
      int cDescriptionIdx = GetFieldIndex(rsHeader, cDescription);

      var predicate = GetRootDnf();
      AddCnf(predicate, AddBoolean(AsCnf(t => t.GetValue<string>(cDescriptionIdx).CompareTo("abc") < 0),
                                   t => t.GetValue<int?>(cLengthIdx) == 6));
      AddCnf(predicate, AsCnf(t => 10 <= t.GetValue<int?>(cLengthIdx)));

      var expectedRanges = CreateExpectedRangesForMultiColumnIndexTest(indexInfo, cLength, cDescription);
      TestExpression(predicate, indexInfo, rsHeader, expectedRanges);
    }

    private IEnumerable<Range<Entire<Tuple>>> CreateExpectedRangesForMultiColumnIndexTest(IndexInfo indexInfo,
      params string[] keyFieldName)
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

    private NormalizedBooleanExpression GetRootDnf()
    {
      return new NormalizedBooleanExpression(NormalFormType.Disjunctive, true);
    }

    private void AddCnf(NormalizedBooleanExpression root,
     NormalizedBooleanExpression exp)
    {
      root.AddNormalizedExpression(exp);
    }

    private NormalizedBooleanExpression AsCnf(Expression<Func<Tuple, bool>> exp)
    {
      return new NormalizedBooleanExpression(NormalFormType.Conjunctive, exp.Body);
    }

    private NormalizedBooleanExpression AddBoolean(NormalizedBooleanExpression parent,
      Expression<Func<Tuple, bool>> exp)
    {
      parent.AddBooleanExpression(exp.Body);
      return parent;
    }

    private void TestExpression(NormalizedBooleanExpression predicate, IndexInfo indexInfo,
      RecordSetHeader primaryIndexRsHeader, IEnumerable<Range<Entire<Tuple>>> expectedRanges)
    {
      RangeSetExtractor extractor = new RangeSetExtractor(Domain.Model);
      var rangeSetExp = extractor.Extract(predicate, indexInfo, primaryIndexRsHeader);
      var result = (RangeSet<Entire<Tuple>>)rangeSetExp.GetResult().Compile().DynamicInvoke();
      CheckRanges(expectedRanges, result);
    }

    private static void CheckRanges(IEnumerable<Range<Entire<Tuple>>> expected,
      IEnumerable<Range<Entire<Tuple>>> actual)
    {
      Assert.AreEqual(expected.Count(), actual.Count());
      foreach (var range in expected) {
// ReSharper disable AccessToModifiedClosure
        actual.Single(r => range.CompareTo(r) == 0);
// ReSharper restore AccessToModifiedClosure
      }
    }

    private static Tuple CreateTuple(TupleDescriptor descriptor, int fieldIndex, object fieldValue)
    {
      Tuple result = Tuple.Create(descriptor);
      result.SetValue(fieldIndex, fieldValue);
      return result;
    }

    private static int GetFieldIndex(RecordSetHeader rsHeader, string fieldName)
    {
      return rsHeader.IndexOf(fieldName);
    }
  }
}