// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class FilterVisitor
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

    protected virtual SqlExpression Visit(Expression exp)
    {
      if (exp == null)
        return null;

      switch (exp.NodeType) {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.ArrayLength:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
          return VisitUnary((UnaryExpression)exp);
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
        case ExpressionType.Divide:
        case ExpressionType.Modulo:
        case ExpressionType.And:
        case ExpressionType.AndAlso:
        case ExpressionType.Or:
        case ExpressionType.OrElse:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
        case ExpressionType.Coalesce:
        case ExpressionType.ArrayIndex:
        case ExpressionType.RightShift:
        case ExpressionType.LeftShift:
        case ExpressionType.ExclusiveOr:
          return VisitBinary((BinaryExpression)exp);
        case ExpressionType.TypeIs:
          return VisitTypeIs((TypeBinaryExpression)exp);
        case ExpressionType.Conditional:
          return VisitConditional((ConditionalExpression)exp);
        case ExpressionType.Constant:
          return VisitConstant((ConstantExpression)exp);
        case ExpressionType.Parameter:
          return VisitParameter((ParameterExpression)exp);
        case ExpressionType.MemberAccess:
          return VisitMemberAccess((MemberExpression)exp);
        case ExpressionType.Call:
          return VisitMethodCall((MethodCallExpression)exp);
        case ExpressionType.Lambda:
          return VisitLambda((LambdaExpression)exp);
        case ExpressionType.New:
          return VisitNew((NewExpression)exp);
        case ExpressionType.NewArrayInit:
        case ExpressionType.NewArrayBounds:
          return VisitNewArray((NewArrayExpression)exp);
        case ExpressionType.Invoke:
          return VisitInvocation((InvocationExpression)exp);
        case ExpressionType.MemberInit:
          return VisitMemberInit((MemberInitExpression)exp);
        case ExpressionType.ListInit:
          return VisitListInit((ListInitExpression)exp);
        default:
          throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
      }
    }

    private SqlExpression VisitUnary(UnaryExpression expression)
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

    private SqlExpression VisitBinary(BinaryExpression expression)
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

    private SqlExpression VisitTypeIs(TypeBinaryExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitConditional(ConditionalExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitConstant(ConstantExpression expression)
    {
      var constant = expression.Value != null ? 
        SqlFactory.Literal(expression.Value) : 
        (SqlExpression)SqlFactory.Null;
      return constant;
    }

    private SqlExpression VisitParameter(ParameterExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitMemberAccess(MemberExpression expression)
    {
      if (expression.Expression.NodeType == ExpressionType.Constant) {
        var lambda = Expression.Lambda(expression).Compile();
        var binding = new SqlFetchRequestParameter(() => lambda.DynamicInvoke(ArrayUtils<object>.EmptyArray));
        request.Parameters.Add(binding);
        return binding.Parameter;
      }
      if (expression.Expression.NodeType == ExpressionType.MemberAccess && expression.Expression.Type.BaseType == typeof(Core.Parameters.Parameter)) {
        var lambda = Expression.Lambda(expression).Compile();
        var binding = new SqlFetchRequestParameter(() => lambda.DynamicInvoke(ArrayUtils<object>.EmptyArray));
        request.Parameters.Add(binding);
        return binding.Parameter;
      }
      throw new NotSupportedException();
    }

    private SqlExpression VisitMethodCall(MethodCallExpression expression)
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

    private SqlExpression VisitLambda(LambdaExpression expression)
    {
      return Visit(expression.Body);
    }

    private SqlExpression VisitNew(NewExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitInvocation(InvocationExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitMemberInit(MemberInitExpression expression)
    {
      throw new NotSupportedException();
    }

    private SqlExpression VisitListInit(ListInitExpression expression)
    {
      throw new NotSupportedException();
    }


    // Constructor

    public FilterVisitor(SqlFetchRequest request)
    {
      this.request = request;
      query = (SqlSelect)request.Statement;
    }
  }
}