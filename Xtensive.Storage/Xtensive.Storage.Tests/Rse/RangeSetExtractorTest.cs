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
      return result;
    }

    [Test]
    public void SimpleExpressionTest()
    {
      int cIDIdx;
      int cNameIdx;
      int cLengthIdx;
      int cFeaturesIdx;
      //using (Domain.OpenSession()) {
      TypeInfo snakeType = Domain.Model.Types[typeof (ClearSnake)];
      IndexInfo indexInfo = snakeType.Indexes.GetIndex("Length");
      RecordSetHeader rsHeader = indexInfo.GetRecordSetHeader();
      cIDIdx = GetFieldIndex(rsHeader, cID);
      cNameIdx = GetFieldIndex(rsHeader, cName);
      cLengthIdx = GetFieldIndex(rsHeader, cLength);
      cFeaturesIdx = GetFieldIndex(rsHeader, cFeatures);
      NormalizedBooleanExpression exp = new NormalizedBooleanExpression(NormalFormType.Disjunctive, true);
      Expression<Func<Tuple, bool>> tupleExp = (t) => t.GetValue<int?>(cLengthIdx) >= 3;
      NormalizedBooleanExpression cnf = new NormalizedBooleanExpression(NormalFormType.Conjunctive, tupleExp.Body);
      tupleExp = (t) => t.GetValue<int?>(cLengthIdx) < 6;
      cnf.AddBooleanExpression(tupleExp.Body);
      exp.AddNormalizedExpression(cnf);
      tupleExp = (t) => t.GetValue<int?>(cLengthIdx) >= 10;
      exp.AddNormalizedExpression(new NormalizedBooleanExpression(NormalFormType.Conjunctive, tupleExp.Body));
      /*Expression<Func<Tuple, bool>> exp =
        (t) => t.GetValue<int?>(cLengthIdx) >= 3 &&
               t.GetValue<int?>(cLengthIdx) < 6 ||
               t.GetValue<int?>(cLengthIdx) >= 10;*/
      RangeSetExtractor extractor = new RangeSetExtractor(Domain.Model);
      var rangeSetExp = extractor.Extract(exp, indexInfo, rsHeader);
      RangeSet<Entire<Tuple>> result = (RangeSet<Entire<Tuple>>)rangeSetExp.GetResult().Compile().DynamicInvoke();
      Assert.AreEqual(2, result.Count());
      Entire<Tuple> expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, cLengthIdx, 3));
      Entire<Tuple> expectedSecond = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, cLengthIdx, 6),
                                                       Direction.Negative);
      var expectedRange0 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Single(r => expectedRange0.CompareTo(r) == 0);
      expectedFirst = new Entire<Tuple>(CreateTuple(indexInfo.KeyTupleDescriptor, cLengthIdx, 10));
      expectedSecond = new Entire<Tuple>(InfinityType.Positive);
      var expectedRange1 = new Range<Entire<Tuple>>(expectedFirst, expectedSecond);
      result.Single(r => expectedRange1.CompareTo(r) == 0);
      //}
    }

    private Tuple CreateTuple(TupleDescriptor descriptor, int fieldIndex, object fieldValue)
    {
      Tuple result = Tuple.Create(descriptor);
      result.SetValue(fieldIndex, fieldValue);
      return result;
    }

    private int GetFieldIndex(RecordSetHeader rsHeader, string fieldName)
    {
      return rsHeader.IndexOf(fieldName);
    }
  }
}