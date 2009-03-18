// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  internal class RangeSetExtractor : ExpressionVisitor
  {
    private IndexInfo indexInfo;
    private Type[] indexedFiledTypes;
    private RecordSetHeader recordSetHeader;
    private readonly DomainModel domainModel;
    
    public LambdaExpression Extract(Expression<Func<Tuple, bool>> predicate, IndexInfo info, RecordSetHeader header)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      indexInfo = info;
      indexedFiledTypes = indexInfo.KeyColumns.Select(pair => pair.Key.Field.ValueType).ToArray();
      recordSetHeader = header;
      var visited = Visit(predicate.Body);
      if (visited.Type == typeof(RangeSet<Entire<Tuple>>))
        return Expression.Lambda(visited);
      if (visited.Type == typeof(bool))
        return Expression.Lambda(VisitBooleanExpression(predicate));
      //TODO: Create the specific Exception type.
      throw new Exception("Parser internal error.");
    }

    protected override Expression VisitBinary(BinaryExpression exp)
    {
      if(exp.Type != typeof(bool))
        return base.VisitBinary(exp);
      if(exp.NodeType == ExpressionType.GreaterThan || exp.NodeType == ExpressionType.GreaterThanOrEqual ||
        exp.NodeType == ExpressionType.LessThan || exp.NodeType == ExpressionType.LessThanOrEqual ||
        exp.NodeType == ExpressionType.Equal)
          return VisitComparisonOperation(exp);

      var visited = VisitLogicalOperation(exp);
      if (visited.Type == typeof(bool)) {
        return VisitBooleanExpression(visited);
      }
      return visited;
    }

    protected override Expression VisitUnary(UnaryExpression exp)
    {
      if (exp.Type != typeof(bool))
        return base.VisitUnary(exp);
      Expression visited = base.VisitUnary(exp);
      if (visited.Type == typeof(bool))
        return VisitBooleanExpression(visited);
      if (visited.Type == typeof(RangeSet<Entire<Tuple>>))
        if (visited.NodeType == ExpressionType.Not)
          return RangeSetExpressionsBuilder.BuildInvert(visited);
        else
          throw new Exception(String.
            Format("Can't parse the expression performing operation {0} on RangeSet", visited.NodeType));
      return visited;
    }

    private Expression VisitComparisonOperation(BinaryExpression exp)
    {
      var leftIsFieldValue = IsTuppleFieldValueGetterCall(exp.Left);
      var rightIsFieldValue = IsTuppleFieldValueGetterCall(exp.Right);
      if(!(leftIsFieldValue ^ rightIsFieldValue))
        return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      int keyFieldIndex;
      if(leftIsFieldValue) {
        if (!GetIndexOfKeyFieldFromIndex((MethodCallExpression) exp.Left, out keyFieldIndex))
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      }
      else {
        if (!GetIndexOfKeyFieldFromIndex((MethodCallExpression) exp.Right, out keyFieldIndex))
          return RangeSetExpressionsBuilder.BuildFullRangeSetConstructor();
      }
      Expression tupleFiledAccessingExp = exp.Left;
      Expression keyValueExp = exp.Right;
      ExpressionType comparisonType = exp.NodeType;
      if (rightIsFieldValue) {
        ReverseOperandsOrder(ref tupleFiledAccessingExp, ref keyValueExp, ref comparisonType);
      }
      return RangeSetExpressionsBuilder.BuildConstructor(keyValueExp, keyFieldIndex, comparisonType,
                                                         indexedFiledTypes, indexInfo);
    }

    private static void ReverseOperandsOrder(ref Expression left, ref Expression right,
      ref ExpressionType comparisonType)
    {
      var t = left;
      left = right;
      right = t;
      if(comparisonType == ExpressionType.Equal)
        return;
      if (comparisonType == ExpressionType.GreaterThan)
        comparisonType = ExpressionType.LessThan;
      else if (comparisonType == ExpressionType.GreaterThanOrEqual)
        comparisonType = ExpressionType.LessThanOrEqual;
      else if (comparisonType == ExpressionType.LessThan)
        comparisonType = ExpressionType.GreaterThan;
      else if (comparisonType == ExpressionType.LessThanOrEqual)
        comparisonType = ExpressionType.GreaterThanOrEqual;
    }

    private static bool IsTuppleFieldValueGetterCall(Expression exp)
    {
      if (exp.NodeType != ExpressionType.Call)
        return false;
      var methodCall = (MethodCallExpression) exp;
      return methodCall.Object.Type == typeof (Tuple) && methodCall.Method.Name.StartsWith("GetValue");
    }

    private bool GetIndexOfKeyFieldFromIndex(MethodCallExpression fieldGetterCall, out int index)
    {
      index = -1;
      var constantExp = fieldGetterCall.Arguments[0] as ConstantExpression;
      var memberAccessExp = fieldGetterCall.Arguments[0] as MemberExpression;
      if (constantExp == null && memberAccessExp == null)
        //TODO: Add string to resources.
        throw new NotSupportedException(
          "The argument passed to call of method \"GetValue\" must be ConstantExpression or MemberExpression");
      int fieldNum = constantExp != null ? (int) constantExp.Value : (int) Expression.Lambda(memberAccessExp).Compile().DynamicInvoke();
      var mappedColumn = recordSetHeader.Columns[fieldNum] as MappedColumn;
      if (mappedColumn == null)
        return false;

      var mappedColumnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      var columnsCount = indexInfo.KeyColumns.Count;
      for (int i = 0; i < columnsCount; i++ ) {
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
      if(leftIsRangeSet ^ rightIsRangeSet)
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

    public RangeSetExtractor(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}
