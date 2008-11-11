// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class Visitor : Visitor<SqlExpression>
  {
    private readonly SqlFetchRequest request;
    private readonly SqlSelect query;

    public SqlFetchRequest Request
    {
      get { return request; }
    }

    public void AppendFilterToRequest(Expression<Func<Tuple,bool>> exp)
    {
      var expression = Visit(exp);
      query.Where &= expression;
    }

    public void AppendCalculationToRequest(Expression<Func<Tuple, object>> exp, string name)
    {
      var expression = Visit(exp);
      query.Columns.Add(expression, name);
    }

    protected override SqlExpression VisitUnary(UnaryExpression expression)
    {
      var operand = Visit(expression.Operand);
      switch (expression.NodeType) {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          return SqlFactory.Negate(operand);
        case ExpressionType.UnaryPlus:
          return operand;

        case ExpressionType.Not:
          if ((expression.Operand.Type!=typeof (bool)) && (expression.Operand.Type!=typeof (bool?)))
            return SqlFactory.BitNot(operand);
          return SqlFactory.Not(operand);

//        case ExpressionType.TypeAs:
//          return operand;
      }
      return operand;
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      var left = Visit(expression.Left);
      var right = Visit(expression.Right);
      switch(expression.NodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return SqlFactory.Add(left,right);
        case ExpressionType.And:
          if ((expression.Left.Type!=typeof (bool)) && (expression.Left.Type!=typeof (bool?)))
            return SqlFactory.BitAnd(left, right);
          return SqlFactory.And(left, right);
        case ExpressionType.AndAlso:
          return SqlFactory.And(left, right);
//        case ExpressionType.Coalesce:
//          break;
        case ExpressionType.Divide:
          return SqlFactory.Divide(left, right);
        case ExpressionType.Equal:
          return SqlFactory.Equals(left, right);
        case ExpressionType.ExclusiveOr:
          return SqlFactory.BitXor(left, right);
        case ExpressionType.GreaterThan:
          return SqlFactory.GreaterThan(left, right);
        case ExpressionType.GreaterThanOrEqual:
          return SqlFactory.GreaterThanOrEquals(left, right);
        case ExpressionType.LessThan:
          return SqlFactory.LessThan(left, right);
        case ExpressionType.LessThanOrEqual:
          return SqlFactory.LessThanOrEquals(left, right);
        case ExpressionType.Modulo:
          return SqlFactory.Modulo(left, right);
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return SqlFactory.Multiply(left, right);
        case ExpressionType.NotEqual:
          return SqlFactory.NotEquals(left, right);
        case ExpressionType.Or:
          if ((expression.Left.Type != typeof(bool)) && (expression.Left.Type != typeof(bool?)))
            return SqlFactory.BitOr(left, right);
          return SqlFactory.Or(left, right);
        case ExpressionType.OrElse:
          return SqlFactory.Or(left, right);
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return SqlFactory.Subtract(left, right);
        default:
          throw new ArgumentOutOfRangeException("expression");
      }
    }

    protected override SqlExpression VisitTypeIs(TypeBinaryExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitConditional(ConditionalExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitConstant(ConstantExpression expression)
    {
      var constant = expression.Value != null ? 
        SqlFactory.Literal(expression.Value) : 
        (SqlExpression)SqlFactory.Null;
      return constant;
    }

    protected override SqlExpression VisitParameter(ParameterExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberAccess(MemberExpression expression)
    {
      if (expression.Expression.NodeType == ExpressionType.Constant) {
        var lambda = Expression.Lambda(expression).Compile();
        var binding = new SqlFetchParameterBinding(() => lambda.DynamicInvoke(ArrayUtils<object>.EmptyArray));
        request.ParameterBindings.Add(binding);
        return binding.SqlParameter;
      }
      if (expression.Expression.NodeType == ExpressionType.MemberAccess && expression.Expression.Type.BaseType == typeof(Core.Parameters.Parameter)) {
        var lambda = Expression.Lambda(expression).Compile();
        var binding = new SqlFetchParameterBinding(() => lambda.DynamicInvoke(ArrayUtils<object>.EmptyArray));
        request.ParameterBindings.Add(binding);
        return binding.SqlParameter;
      }
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMethodCall(MethodCallExpression expression)
    {
      if (expression.Object.Type == typeof(Tuple)) {
        if (expression.Method.Name == "GetValue" || expression.Method.Name == "GetValueOrDefault") {
          var columnArgument = expression.Arguments[0];
          int columnIndex;
          if (columnArgument.NodeType==ExpressionType.Constant)
            columnIndex = (int) ((ConstantExpression) columnArgument).Value;
          else {
            var columnFunc = Expression.Lambda<Func<int>>(columnArgument).Compile();
            columnIndex = columnFunc();
          }
          var sqlSelect = (SqlSelect)request.Statement;
          return sqlSelect[columnIndex];
        }
      }
      var map = MethodMapping.GetMapping(expression.Method);
      var target = Visit(expression.Object);
      var arguments = expression.Arguments.Select(a => Visit(a)).ToArray();
      return map(target, arguments);
    }

    protected override SqlExpression VisitLambda(LambdaExpression expression)
    {
      return Visit(expression.Body);
    }

    protected override SqlExpression VisitNew(NewExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitInvocation(InvocationExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberInit(MemberInitExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitListInit(ListInitExpression expression)
    {
      throw new NotSupportedException();
    }

    
    // Constructor

    public Visitor(SqlFetchRequest request)
    {
      this.request = request;
      query = (SqlSelect)request.Statement;
    }
  }
}