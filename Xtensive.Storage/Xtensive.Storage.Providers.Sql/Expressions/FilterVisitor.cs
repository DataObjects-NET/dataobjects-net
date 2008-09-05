// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  public class FilterVisitor
  {
    public virtual FilterExpression Visit(Expression exp)
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

    private FilterExpression VisitUnary(UnaryExpression expression)
    {
      var operand = Visit(expression.Operand);
      switch (expression.NodeType) {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          return new FilterExpression(SqlFactory.Negate(operand.Expression));
        case ExpressionType.UnaryPlus:
          return operand;

        case ExpressionType.Not:
          if ((expression.Operand.Type!=typeof (bool)) && (expression.Operand.Type!=typeof (bool?)))
            return new FilterExpression(SqlFactory.BitNot(operand.Expression));
          return new FilterExpression(SqlFactory.Not(operand.Expression));

//        case ExpressionType.TypeAs:
//          return operand;
      }
      return operand;
    }

    private FilterExpression VisitBinary(BinaryExpression expression)
    {
      var left = Visit(expression.Left).Expression;
      var right = Visit(expression.Right).Expression;
      switch(expression.NodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return new FilterExpression(SqlFactory.Add(left,right));
        case ExpressionType.And:
          if ((expression.Left.Type!=typeof (bool)) && (expression.Left.Type!=typeof (bool?)))
            return new FilterExpression(SqlFactory.BitAnd(left, right));
          return new FilterExpression(SqlFactory.And(left, right));
        case ExpressionType.AndAlso:
          return new FilterExpression(SqlFactory.And(left, right));
//        case ExpressionType.Coalesce:
//          break;
        case ExpressionType.Divide:
          return new FilterExpression(SqlFactory.Divide(left, right));
        case ExpressionType.Equal:
          return new FilterExpression(SqlFactory.Equals(left, right));
        case ExpressionType.ExclusiveOr:
          return new FilterExpression(SqlFactory.BitXor(left, right));
        case ExpressionType.GreaterThan:
          return new FilterExpression(SqlFactory.GreaterThan(left, right));
        case ExpressionType.GreaterThanOrEqual:
          return new FilterExpression(SqlFactory.GreaterThanOrEquals(left, right));
        case ExpressionType.LessThan:
          return new FilterExpression(SqlFactory.LessThan(left, right));
        case ExpressionType.LessThanOrEqual:
          return new FilterExpression(SqlFactory.LessThanOrEquals(left, right));
        case ExpressionType.Modulo:
          return new FilterExpression(SqlFactory.Modulo(left, right));
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return new FilterExpression(SqlFactory.Multiply(left, right));
        case ExpressionType.NotEqual:
          return new FilterExpression(SqlFactory.NotEquals(left, right));
        case ExpressionType.Or:
          if ((expression.Left.Type != typeof(bool)) && (expression.Left.Type != typeof(bool?)))
            return new FilterExpression(SqlFactory.BitOr(left, right));
          return new FilterExpression(SqlFactory.Or(left, right));
        case ExpressionType.OrElse:
          return new FilterExpression(SqlFactory.Or(left, right));
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return new FilterExpression(SqlFactory.Subtract(left, right));
        default:
          throw new ArgumentOutOfRangeException("expression");
      }
    }

    private FilterExpression VisitTypeIs(TypeBinaryExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitConditional(ConditionalExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitConstant(ConstantExpression expression)
    {
      var constant = expression.Value != null ? 
        SqlFactory.Constant(expression.Value.ToString()) : 
        (SqlExpression)SqlFactory.Null;
      return new FilterExpression(constant);
    }

    private FilterExpression VisitParameter(ParameterExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitMemberAccess(MemberExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitMethodCall(MethodCallExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitLambda(LambdaExpression expression)
    {
      return Visit(expression.Body);
    }

    private FilterExpression VisitNew(NewExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitInvocation(InvocationExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitMemberInit(MemberInitExpression expression)
    {
      throw new NotImplementedException();
    }

    private FilterExpression VisitListInit(ListInitExpression expression)
    {
      throw new NotImplementedException();
    }
  }
}