// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
    private Type[] indexedFiledTypes;
    private RecordSetHeader recordSetHeader;
    private readonly DomainModel domainModel;

    public Expression Extract(NormalizedBooleanExpression normalized, IndexInfo info, RecordSetHeader header)
    {
      ValidateNormalized(normalized);
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      indexInfo = info;
      indexedFiledTypes = indexInfo.KeyColumns.Select(pair => pair.Key.Field.ValueType).ToArray();
      recordSetHeader = header;
      return ProcessCnfExpression(normalized);
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
        if (exp.NodeType == ExpressionType.GreaterThan || exp.NodeType == ExpressionType.GreaterThanOrEqual ||
            exp.NodeType == ExpressionType.LessThan || exp.NodeType == ExpressionType.LessThanOrEqual ||
            exp.NodeType == ExpressionType.Equal || exp.NodeType == ExpressionType.NotEqual)
          return VisitComparisonOperation(exp.NodeType, left, right, leftIsTuple, rightIsTuple);
        if (leftIsTuple)
          return VisitBinaryWithTuple(leftAsTuple, true, exp);
        return VisitBinaryWithTuple(rightAsTuple, false, exp);
      }
      return base.VisitBinary(exp);
    }

    private static ExpressionOnTupleField VisitBinaryWithTuple(ExpressionOnTupleField tupleExp, bool isLeft,
      BinaryExpression binaryExp)
    {
      string operationName;
      switch (binaryExp.NodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          operationName = OperationInfo.WellKnownNames.Add;
          break;
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          operationName = OperationInfo.WellKnownNames.Substract;
          break;
        case ExpressionType.Divide:
          operationName = OperationInfo.WellKnownNames.Divide;
          break;
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          operationName = OperationInfo.WellKnownNames.Multiply;
          break;
        default:
          operationName = OperationInfo.WellKnownNames.Unknown;
          break;
      }
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
      Expression visited = base.VisitUnary(exp);
      if (visited.Type == typeof(RangeSet<Entire<Tuple>>))
        if (visited.NodeType == ExpressionType.Not)
          return RangeSetExpressionsBuilder.BuildInvert(visited);
        else
          throw new Exception(String.
            Format("Can't parse the expression performing operation {0} on RangeSet", visited.NodeType));
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
                                                         indexedFiledTypes, indexInfo);
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
      if (String.CompareOrdinal(operation.Name, OperationInfo.WellKnownNames.Invert) == 0)
        return ParseUnaryNotExpression(keyValueExp, keyFieldIndex, comparisonType);
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
      return RangeSetExpressionsBuilder.BuildConstructor(realKey, keyFieldIndex, realComparison,
                                                         indexedFiledTypes, indexInfo);
    }

    private Expression ParseUnaryNotExpression(Expression keyValueExp, int keyFieldIndex,
      ExpressionType comparisonType)
    {
      ExpressionType realComparison = comparisonType;
      if(comparisonType == ExpressionType.Equal)
        realComparison = ExpressionType.NotEqual;
      else if (comparisonType == ExpressionType.NotEqual)
        realComparison = ExpressionType.Equal;
      else
        ReverseOperation(ref realComparison);
      return RangeSetExpressionsBuilder.BuildConstructor(keyValueExp, keyFieldIndex, realComparison,
                                                         indexedFiledTypes, indexInfo);
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

    private Expression VisitLogicalOperation(BinaryExpression exp)
    {
      Expression left = Visit(exp.Left);
      Expression right = Visit(exp.Right);
      var leftIsRangeSet = left.Type == typeof(RangeSet<Entire<Tuple>>);
      var rightIsRangeSet = right.Type == typeof(RangeSet<Entire<Tuple>>);
      if (leftIsRangeSet ^ rightIsRangeSet)
        //TODO: Create the specific Exception type.
        throw new Exception("Can't parse binary expression where only one operand is RecordSet.");

      //Both of arguments of this operation are not RangeSets.
      if (!leftIsRangeSet)
        return exp;

      switch (exp.NodeType) {
        case ExpressionType.OrElse:
          return RangeSetExpressionsBuilder.BuildUnite(left, right);

        case ExpressionType.AndAlso:
          return RangeSetExpressionsBuilder.BuildIntersect(left, right);

        default:
          //TODO: Create the specific Exception type.
          throw new Exception(String.
            Format("Can't parse the expression perfoming operation {0} on RangeSets.", exp.NodeType));
      }
    }

    private static Expression VisitBooleanExpression(Expression exp)
    {
      //TODO: Impelement the recursive search for expressions containing tuples.
      return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
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

    private Expression ProcessCnfExpression(NormalizedBooleanExpression normalized)
    {
      Expression result = null;
      foreach (var exp in normalized) {
        if (result == null)
          result = Visit(exp);
        else
          result = RangeSetExpressionsBuilder.BuildIntersect(result, Visit(exp));
      }
      return result;
    }

    public ExtractingVisitor(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}