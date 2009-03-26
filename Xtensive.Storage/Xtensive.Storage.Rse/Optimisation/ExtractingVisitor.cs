// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Extracter of <see cref="RangeSet{T}"/> from a boolean expression in a conjunctive normal form.
  /// </summary>
  internal class ExtractingVisitor : ExpressionVisitor
  {
    private IndexInfo indexInfo;
    private RecordSetHeader recordSetHeader;
    private readonly DomainModel domainModel;
    private const int defaultExpressionsListSize = 10;
    private readonly List<RangeSetExpression> extractedExpressions =
      new List<RangeSetExpression>(defaultExpressionsListSize);
    private readonly Dictionary<int, Expression> indexKeyValues =
      new Dictionary<int, Expression>(defaultExpressionsListSize);

    private readonly Comparison<RangeSetExpression> cashedComparison =
      (r1, r2) =>
      {
        if (r1.Origin != null && r2.Origin != null) {
          return r1.Origin.TupleField.CompareTo(r2.Origin.TupleField);
        }
        else {
          if (r1.Origin == null && r2.Origin == null)
            return 0;
          if (r2.Origin == null)
            return -1;
          return 1;
        }
      };

    public static void ReverseOperation(ref ExpressionType comparisonType)
    {
      switch (comparisonType) {
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
          return;
        case ExpressionType.GreaterThan:
          comparisonType = ExpressionType.LessThan;
          return;
        case ExpressionType.GreaterThanOrEqual:
          comparisonType = ExpressionType.LessThanOrEqual;
          return;
        case ExpressionType.LessThan:
          comparisonType = ExpressionType.GreaterThan;
          return;
        case ExpressionType.LessThanOrEqual:
          comparisonType = ExpressionType.GreaterThanOrEqual;
          return;
      }
    }

    public RangeSetExpression Extract(NormalizedBooleanExpression normalized, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ValidateNormalized(normalized);
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      indexInfo = info;
      recordSetHeader = primaryIdxRecordSetHeader;
      return ParseCnfExpression(normalized);
    }

    protected override Expression VisitBinary(BinaryExpression exp)
    {
      var left = Visit(exp.Left);
      var right = Visit(exp.Right);
      var leftAsTuple = left as ExpressionOnTupleField;
      var rightAsTuple = right as ExpressionOnTupleField;
      bool leftIsTuple = leftAsTuple != null;
      bool rightIsTuple = rightAsTuple != null;
      if (leftIsTuple || rightIsTuple) {
        if (IsComparison(exp.NodeType))
          return VisitComparisonOperation(exp.NodeType, left, right, leftIsTuple, rightIsTuple);
        if (leftIsTuple)
          return VisitBinaryWithTuple(leftAsTuple, true, exp);
        return VisitBinaryWithTuple(rightAsTuple, false, exp);
      }
      if (left.Type == typeof(RangeSet<Entire<Tuple>>) || right.Type == typeof(RangeSet<Entire<Tuple>>))
        throw new Exception(String.
            Format("Can't parse the expression performing operation {0} on RangeSet", exp.NodeType));
      return base.VisitBinary(exp);
    }

    private static bool IsComparison(ExpressionType nodeType)
    {
      return nodeType == ExpressionType.GreaterThan || nodeType == ExpressionType.GreaterThanOrEqual ||
             nodeType == ExpressionType.LessThan || nodeType == ExpressionType.LessThanOrEqual ||
             nodeType == ExpressionType.Equal || nodeType == ExpressionType.NotEqual;
    }

    private static ExpressionOnTupleField VisitBinaryWithTuple(ExpressionOnTupleField tupleExp, bool isLeft,
      BinaryExpression binaryExp)
    {
      string operationName = GetOperationName(binaryExp.NodeType);
      tupleExp.EnqueueOperation(operationName, new[] {isLeft ? binaryExp.Left : binaryExp.Right}, binaryExp);
      return tupleExp;
    }

    protected override Expression VisitMethodCall(MethodCallExpression exp)
    {
      if (IsTuppleFieldValueGetterCall(exp)) {
        var constantExp = exp.Arguments[0] as ConstantExpression;
        var memberAccessExp = exp.Arguments[0] as MemberExpression;
        if (constantExp == null && memberAccessExp == null)
          //TODO: Add string to resources.
          throw new NotSupportedException(
            "The argument passed to call of method \"GetValue\" must be ConstantExpression or MemberExpression");
        int fieldIndex = constantExp != null ? (int)constantExp.Value : (int)Expression.Lambda(memberAccessExp).Compile().DynamicInvoke();
        return new ExpressionOnTupleField(fieldIndex, exp);
      }

      var visitedExp = Visit(exp.Object);
      var visitedArguments = VisitExpressionList(exp.Arguments);
      var tupleExp = visitedExp as ExpressionOnTupleField;
      if (tupleExp != null) {
        tupleExp.EnqueueOperation(exp.Method.Name, visitedArguments, exp);
        return tupleExp;
      }
      return base.VisitMethodCall(exp);
    }

    protected override Expression VisitUnary(UnaryExpression exp)
    {
      if (exp.Type != typeof(bool))
        return base.VisitUnary(exp);
      Expression visited = base.Visit(exp.Operand);
      var rsExp = visited as RangeSetExpression;
      if (rsExp != null)
        if (exp.NodeType == ExpressionType.Not)
          return RangeSetExpressionsBuilder.BuildInvert(rsExp);
        else
          throw new Exception(String.
            Format("Can't parse the expression performing operation {0} on RangeSet", rsExp.NodeType));
      var tupleExp = visited as ExpressionOnTupleField;
      if (tupleExp != null) {
        var operationName = GetOperationName(exp.NodeType);
        tupleExp.EnqueueOperation(operationName, null, exp);
      }
      return visited;
    }

    private Expression VisitComparisonOperation(ExpressionType nodeType, Expression left, Expression right,
      bool leftIsTuple, bool rightIsTuple)
    {
      ExpressionOnTupleField leftAsTuple = leftIsTuple ? (ExpressionOnTupleField) left : null;
      ExpressionOnTupleField rightAsTuple = rightIsTuple ? (ExpressionOnTupleField) right : null;
      if (!(leftIsTuple ^ rightIsTuple))
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(null);

      ExpressionOnTupleField tupleFiledAccessingExp = leftIsTuple ? leftAsTuple : rightAsTuple;
      Expression keyValueExp = !rightIsTuple ? right : left;
      ExpressionType comparisonType = nodeType;

      if (rightIsTuple) {
        ReverseOperation(ref comparisonType);
      }
      if (tupleFiledAccessingExp.HasOperations)
        return ProcessTupleExpressionWithOperations(tupleFiledAccessingExp,
                                                    keyValueExp,
                                                    tupleFiledAccessingExp.FieldIndex,
                                                    comparisonType);

      if (!IndexHasKeyAtZeroPoisition(tupleFiledAccessingExp.FieldIndex))
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(
                                                    new RangeSetOriginInfo(comparisonType,
                                                                           tupleFiledAccessingExp.FieldIndex,
                                                                           keyValueExp));


      return RangeSetExpressionsBuilder.BuildConstructor(keyValueExp,
                                                         tupleFiledAccessingExp.FieldIndex,
                                                         comparisonType,
                                                         indexInfo);
    }

    private Expression ProcessTupleExpressionWithOperations(ExpressionOnTupleField tupleExp,
      Expression keyValueExp, int tupleField, ExpressionType comparisonType)
    {
      //We analyse only one operation.
      if (tupleExp.OperationsCount > 1)
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(null);
      bool isFieldOfIndex = IndexHasKeyAtZeroPoisition(tupleExp.FieldIndex);
      OperationInfo operation = tupleExp.DequeueOperation();
      if (String.CompareOrdinal(operation.Name, OperationInfo.WellKnownNames.CompareTo) == 0)
        return ParseCompareToOperation(operation, keyValueExp, tupleField, isFieldOfIndex, comparisonType);
      return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(null);
    }

    private Expression ParseCompareToOperation(OperationInfo operation, Expression keyValueExp,
      int tupleField, bool isFieldOfIndex, ExpressionType comparisonType)
    {
      var comparisonResult = (int)Expression.Lambda(keyValueExp).Compile().DynamicInvoke();
      ExpressionType realComparison;
      Expression realKey = operation.GetArguments()[0];
      if (comparisonResult < 0) {
        if (comparisonType == ExpressionType.LessThan || comparisonType == ExpressionType.LessThanOrEqual ||
            comparisonType == ExpressionType.Equal)
          realComparison = ExpressionType.LessThan;
        else
          realComparison = ExpressionType.GreaterThanOrEqual;
      }
      else if (comparisonResult == 0) {
        realComparison = comparisonType;
      }
      else {
        if (comparisonType == ExpressionType.LessThan || comparisonType == ExpressionType.LessThanOrEqual ||
            comparisonType == ExpressionType.NotEqual)
          realComparison = ExpressionType.LessThanOrEqual;
        else
          realComparison = ExpressionType.GreaterThan;
      }
      if (!isFieldOfIndex)
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(
                                              new RangeSetOriginInfo(realComparison,
                                                                     tupleField, realKey));
      return RangeSetExpressionsBuilder.BuildConstructor(realKey, tupleField, realComparison, indexInfo);
    }

    private static bool IsTuppleFieldValueGetterCall(Expression exp)
    {
      if (exp.NodeType != ExpressionType.Call)
        return false;
      var methodCall = (MethodCallExpression)exp;
      return methodCall.Object.Type == typeof(Tuple) && methodCall.Method.Name.StartsWith("GetValue");
    }

    private bool IndexHasKeyAtZeroPoisition(int tupleFieldIndex)
    {
      return IndexHasKeyAtSpecifiedPoisition(tupleFieldIndex, 0);
    }

    private bool IndexHasKeyAtSpecifiedPoisition(int tupleFieldPosition, int indexFieldPosition)
    {
      var mappedColumn = recordSetHeader.Columns[tupleFieldPosition] as MappedColumn;
      if (mappedColumn == null)
        return false;

      var mappedColumnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      return indexInfo.KeyColumns.Count > indexFieldPosition &&
             mappedColumnInfo.Equals(indexInfo.KeyColumns[indexFieldPosition].Key);
    }

    private static void ValidateNormalized(NormalizedBooleanExpression normalized)
    {
      ArgumentValidator.EnsureArgumentNotNull(normalized, "normalized");
      if (normalized.NormalForm != NormalFormType.Conjunctive)
        throw new ArgumentException(String.Format(Resources.Strings.ExNormalizedExpressionMustHaveXForm,
                                    NormalFormType.Conjunctive), "normalized");
      if (normalized.IsRoot)
        throw new ArgumentException(Resources.Strings.ExNormalizedExpressionMustNotBeRoot, "normalized");
    }

    private RangeSetExpression ParseCnfExpression(NormalizedBooleanExpression normalized)
    {
      ParseTerms(normalized);
      return MakeResultExpression();
    }

    private void ParseTerms(NormalizedBooleanExpression normalized)
    {
      extractedExpressions.Clear();
      foreach (var exp in normalized) {
        var intermediate = Visit(exp);
        var tupleExp = intermediate as ExpressionOnTupleField;
        if (tupleExp != null)
          intermediate = VisitStandAloneTupleExpression(tupleExp);
        if (intermediate.Type == typeof(bool))
          intermediate = RangeSetExpressionsBuilder.BuildFullOrEmpty(intermediate);
        extractedExpressions.Add((RangeSetExpression)intermediate);
      }
    }

    private Expression VisitStandAloneTupleExpression(ExpressionOnTupleField tupleExp)
    {
      //We analyse only one operation.
      if (tupleExp.OperationsCount > 1)
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(null);
      if(IndexHasKeyAtZeroPoisition(tupleExp.FieldIndex)) {
        if (tupleExp.OperationsCount > 0) {
          var operation = tupleExp.DequeueOperation();
          if (String.CompareOrdinal(operation.Name, OperationInfo.WellKnownNames.Not) == 0)
            return RangeSetExpressionsBuilder.BuildConstructor(Expression.Constant(true), tupleExp.FieldIndex,
                                                               ExpressionType.NotEqual, indexInfo);
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(null);
        }
        return RangeSetExpressionsBuilder.BuildConstructor(Expression.Constant(true), tupleExp.FieldIndex,
                                                               ExpressionType.Equal, indexInfo);
      }
      return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor(
        new RangeSetOriginInfo(ExpressionType.Equal,
                               tupleExp.FieldIndex,
                               Expression.Constant(true)));
    }

    private RangeSetExpression MakeResultExpression()
    {
      RangeSetExpression result = TryUseMultiFieldIndex();
      if (result != null)
        return result;
      foreach (var rangeSet in extractedExpressions) {
        string test = ExpressionWriter.Write(rangeSet.Source);
        if (result == null)
          result = rangeSet;
        else
          result = RangeSetExpressionsBuilder.BuildIntersect(rangeSet, result);
      }
      return result;
    }

    private RangeSetExpression TryUseMultiFieldIndex()
    {
      extractedExpressions.Sort(cashedComparison);
      int lastFieldPosition = -1;
      foreach (var rangeSet in extractedExpressions) {
        if(rangeSet.Origin == null)
          break;
        bool presentInIndex = IndexHasKeyAtSpecifiedPoisition(rangeSet.Origin.TupleField,
                                                              lastFieldPosition + 1);
        if(!presentInIndex)
          break;

        if (rangeSet.Origin.Comparison == ExpressionType.Equal) {
          lastFieldPosition++;
        }
        else {
          lastFieldPosition++;
          break;
        }
      }

      if (lastFieldPosition <= 0)
        return null;

      indexKeyValues.Clear();
      for (int i = 0; i <= lastFieldPosition; i++) {
        indexKeyValues.Add(i, extractedExpressions[i].Origin.KeyValue);
      }
      return RangeSetExpressionsBuilder.BuildConstructor(indexKeyValues,
                                             extractedExpressions[lastFieldPosition].Origin.Comparison,
                                             indexInfo);
    }

    private static string GetOperationName(ExpressionType nodeType)
    {
      switch (nodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return OperationInfo.WellKnownNames.Add;
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return OperationInfo.WellKnownNames.Substract;
        case ExpressionType.Divide:
          return OperationInfo.WellKnownNames.Divide;
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return OperationInfo.WellKnownNames.Multiply;
        case ExpressionType.Not:
          return OperationInfo.WellKnownNames.Not;
        default:
          return OperationInfo.WellKnownNames.Unknown;
      }
    }

    public ExtractingVisitor(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}