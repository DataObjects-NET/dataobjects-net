// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Extracts a <see cref="Expression"/> returning a <see cref="RangeSet{T}"/>
  /// from a boolean expression in a conjunctive normal form./>
  /// </summary>
  internal class ExtractingVisitor : ExpressionVisitor
  {
    private IndexInfo indexInfo;
    private RecordSetHeader recordSetHeader;
    private readonly DomainModel domainModel;

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
      var tupleExp = exp.Object as ExpressionOnTupleField;
      if (tupleExp != null) {
        tupleExp.EnqueueOperation(exp.Method.Name, exp.Arguments, exp);
        return tupleExp;
      }
      if(IsTuppleFieldValueGetterCall(exp)) {
        var constantExp = exp.Arguments[0] as ConstantExpression;
        var memberAccessExp = exp.Arguments[0] as MemberExpression;
        if (constantExp == null && memberAccessExp == null)
          //TODO: Add string to resources.
          throw new NotSupportedException(
            "The argument passed to call of method \"GetValue\" must be ConstantExpression or MemberExpression");
        int fieldIndex = constantExp != null ? (int)constantExp.Value : (int)Expression.Lambda(memberAccessExp).Compile().DynamicInvoke();
        return new ExpressionOnTupleField(fieldIndex, exp);
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
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      int keyFieldIndex;
      if (leftIsTuple) {
        if (!GetIndexOfKeyFieldFromIndex(leftAsTuple.FieldIndex, out keyFieldIndex))
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      }
      else {
        if (!GetIndexOfKeyFieldFromIndex(rightAsTuple.FieldIndex, out keyFieldIndex))
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      }
      ExpressionOnTupleField tupleFiledAccessingExp = leftIsTuple ? leftAsTuple : rightAsTuple;
      Expression keyValueExp = !rightIsTuple ? right : left;
      ExpressionType comparisonType = nodeType;
      if (rightIsTuple) {
        ReverseOperation(ref comparisonType);
      }
      if (tupleFiledAccessingExp.HasOperations)
        return ProcessTupleExpressionWithOperations(tupleFiledAccessingExp, keyValueExp, keyFieldIndex,
                                                    comparisonType);
      return RangeSetExpressionsBuilder.BuildConstructor(keyValueExp, keyFieldIndex, comparisonType,
                                                         indexInfo);
    }

    private Expression ProcessTupleExpressionWithOperations(ExpressionOnTupleField tupleExp,
      Expression keyValueExp, int keyFieldIndex, ExpressionType comparisonType)
    {
      //We analyse only one operation.
      if (tupleExp.OperationsCount > 1)
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      OperationInfo operation = tupleExp.DequeueOperation();
      if (String.CompareOrdinal(operation.Name, OperationInfo.WellKnownNames.CompareTo) == 0)
        return ParseCompareToOperation(operation, keyValueExp, keyFieldIndex, comparisonType);
      return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
    }

    private Expression ParseCompareToOperation(OperationInfo operation, Expression keyValueExp,
      int keyFieldIndex, ExpressionType comparisonType)
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
      return RangeSetExpressionsBuilder.BuildConstructor(realKey, keyFieldIndex, realComparison, indexInfo);
    }

    private static void ReverseOperation(ref ExpressionType comparisonType)
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

    private static bool IsTuppleFieldValueGetterCall(Expression exp)
    {
      if (exp.NodeType != ExpressionType.Call)
        return false;
      var methodCall = (MethodCallExpression)exp;
      return methodCall.Object.Type == typeof(Tuple) && methodCall.Method.Name.StartsWith("GetValue");
    }

    private bool GetIndexOfKeyFieldFromIndex(int tupleFieldIndex, out int index)
    {
      index = -1;
      var mappedColumn = recordSetHeader.Columns[tupleFieldIndex] as MappedColumn;
      if (mappedColumn == null)
        return false;

      var mappedColumnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      var columnsCount = indexInfo.KeyColumns.Count;
      for (int i = 0; i < columnsCount; i++) {
        if (mappedColumnInfo.Equals(indexInfo.KeyColumns[i].Key)) {
          index = i;
          return true;
        }
      }
      return false;
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
      RangeSetExpression result = null;
      foreach (var exp in normalized) {
        var intermediate = Visit(exp);
        var tupleExp = intermediate as ExpressionOnTupleField;
        if (tupleExp != null)
          intermediate = VisitStandAloneTupleExpression(tupleExp);
        if (intermediate.Type == typeof(bool))
          intermediate = RangeSetExpressionsBuilder.BuildFullOrEmpty(intermediate);
        if (result == null)
          result = (RangeSetExpression)intermediate;
        else
          result = RangeSetExpressionsBuilder.BuildIntersect((RangeSetExpression)intermediate, result);
      }
      return result;
    }

    private Expression VisitStandAloneTupleExpression(ExpressionOnTupleField tupleExp)
    {
      //We analyse only one operation.
      if (tupleExp.OperationsCount > 1)
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      int keyFieldindex;
      if(GetIndexOfKeyFieldFromIndex(tupleExp.FieldIndex, out keyFieldindex)) {
        if (tupleExp.OperationsCount > 0) {
          var operation = tupleExp.DequeueOperation();
          if (String.CompareOrdinal(operation.Name, OperationInfo.WellKnownNames.Invert) == 0)
            return RangeSetExpressionsBuilder.BuildConstructor(Expression.Constant(true), keyFieldindex,
                                                               ExpressionType.NotEqual, indexInfo);
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
        }
        return RangeSetExpressionsBuilder.BuildConstructor(Expression.Constant(true), keyFieldindex,
                                                               ExpressionType.Equal, indexInfo);
      }
      return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
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