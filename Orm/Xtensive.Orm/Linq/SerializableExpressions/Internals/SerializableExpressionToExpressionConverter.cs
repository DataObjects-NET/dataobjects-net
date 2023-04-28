// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Linq.SerializableExpressions.Internals
{
  internal sealed class SerializableExpressionToExpressionConverter
  {
    private readonly SerializableExpression source;
    private readonly Dictionary<SerializableExpression, Expression> cache;

    public Expression Convert()
    {
      return Visit(source);
    }

    #region Private / internal methods

    private Expression Visit(SerializableExpression e)
    {
      if (e == null)
        return null;

      Expression result;
      if (cache.TryGetValue(e, out result))
        return result;

      switch (e.NodeType)
      {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.ArrayLength:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
          result = VisitUnary((SerializableUnaryExpression)e);
          break;
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
          result = VisitBinary((SerializableBinaryExpression)e);
          break;
        case ExpressionType.TypeIs:
          result = VisitTypeIs((SerializableTypeBinaryExpression)e);
          break;
        case ExpressionType.Conditional:
          result = VisitConditional((SerializableConditionalExpression)e);
          break;
        case ExpressionType.Constant:
          result = VisitConstant((SerializableConstantExpression)e);
          break;
        case ExpressionType.Parameter:
          result = VisitParameter((SerializableParameterExpression)e);
          break;
        case ExpressionType.MemberAccess:
          result = VisitMemberAccess((SerializableMemberExpression)e);
          break;
        case ExpressionType.Call:
          result = VisitMethodCall((SerializableMethodCallExpression)e);
          break;
        case ExpressionType.Lambda:
          result = VisitLambda((SerializableLambdaExpression)e);
          break;
        case ExpressionType.New:
          result = VisitNew((SerializableNewExpression)e);
          break;
        case ExpressionType.NewArrayInit:
        case ExpressionType.NewArrayBounds:
          result = VisitNewArray((SerializableNewArrayExpression)e);
          break;
        case ExpressionType.Invoke:
          result = VisitInvocation((SerializableInvocationExpression)e);
          break;
        case ExpressionType.MemberInit:
          result = VisitMemberInit((SerializableMemberInitExpression)e);
          break;
        case ExpressionType.ListInit:
          result = VisitListInit((SerializableListInitExpression)e);
          break;
        default:
          throw new ArgumentException();
      }

      cache.Add(e, result);
      return result;
    }

    private Expression VisitUnary(SerializableUnaryExpression u)
    {
      return Expression.MakeUnary(u.NodeType, Visit(u.Operand), u.Type, u.Method);
    }

    private Expression VisitBinary(SerializableBinaryExpression b)
    {
      return Expression.MakeBinary(b.NodeType, Visit(b.Left), Visit(b.Right), b.IsLiftedToNull, b.Method);
    }

    private Expression VisitTypeIs(SerializableTypeBinaryExpression tb)
    {
      return Expression.TypeIs(Visit(tb.Expression), tb.TypeOperand);
    }

    private Expression VisitConstant(SerializableConstantExpression c)
    {
      return Expression.Constant(c.Value, c.Type);
    }

    private Expression VisitConditional(SerializableConditionalExpression c)
    {
      return Expression.Condition(Visit(c.Test), Visit(c.IfTrue), Visit(c.IfFalse));
    }

    private Expression VisitParameter(SerializableParameterExpression p)
    {
      return Expression.Parameter(p.Type, p.Name);
    }

    private Expression VisitMemberAccess(SerializableMemberExpression m)
    {
      var target = Visit(m.Expression);
      return m.Member switch {
        FieldInfo field => Expression.Field(target, field),
        PropertyInfo property => Expression.Property(target, property),
        MethodInfo method => Expression.Property(target, method),
        _ => throw new ArgumentException()
      };
    }

    private Expression VisitMethodCall(SerializableMethodCallExpression mc)
    {
      return Expression.Call(Visit(mc.Object), mc.Method, VisitExpressionSequence(mc.Arguments));
    }

    private Expression VisitLambda(SerializableLambdaExpression l)
    {
      return FastExpression.Lambda(l.Type, Visit(l.Body), l.Parameters.Select(p => (ParameterExpression) Visit(p)));
    }

    private Expression VisitNew(SerializableNewExpression n)
    {
      if (n.Constructor == null)
        return Expression.New(n.Type);
      var nMembers = n.Members;
      if (nMembers?.Length > 0)
        return Expression.New(n.Constructor, VisitExpressionSequence(n.Arguments), nMembers);
      return Expression.New(n.Constructor, VisitExpressionSequence(n.Arguments));
    }

    private Expression VisitMemberInit(SerializableMemberInitExpression mi)
    {
      return Expression.MemberInit((NewExpression) Visit(mi.NewExpression), VisitMemberBindingSequence(mi.Bindings));
    }

    private Expression VisitListInit(SerializableListInitExpression li)
    {
      return Expression.ListInit((NewExpression) Visit(li.NewExpression), VisitElementInitSequence(li.Initializers));
    }

    private Expression VisitNewArray(SerializableNewArrayExpression na)
    {
      switch (na.NodeType) {
      case ExpressionType.NewArrayInit:
        return Expression.NewArrayInit(na.Type.GetElementType(), VisitExpressionSequence(na.Expressions));
      case ExpressionType.NewArrayBounds:
        return Expression.NewArrayBounds(na.Type.GetElementType(), VisitExpressionSequence(na.Expressions));
      default:
        throw new ArgumentException();
      }
    }

    private Expression VisitInvocation(SerializableInvocationExpression i)
    {
      return Expression.Invoke(Visit(i.Expression), VisitExpressionSequence(i.Arguments));
    }

    private IEnumerable<MemberBinding> VisitMemberBindingSequence(IEnumerable<SerializableMemberBinding> bindings)
    {
      foreach (var binding in bindings)
        switch (binding.BindingType) {
        case MemberBindingType.Assignment:
          yield return Expression.Bind(binding.Member,
            Visit(((SerializableMemberAssignment) binding).Expression));
          break;
        case MemberBindingType.MemberBinding:
          yield return Expression.MemberBind(binding.Member,
            VisitMemberBindingSequence(((SerializableMemberMemberBinding) binding).Bindings));
          break;
        case MemberBindingType.ListBinding:
          yield return Expression.ListBind(binding.Member, VisitElementInitSequence(((SerializableMemberListBinding) binding).Initializers));
          break;
        default:
          throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerable<ElementInit> VisitElementInitSequence(IEnumerable<SerializableElementInit> initializers)
    {
      return initializers.Select(initializer =>
        Expression.ElementInit(initializer.AddMethod, VisitExpressionSequence(initializer.Arguments)));
    }

    private IEnumerable<Expression> VisitExpressionSequence<T>(IEnumerable<T> expressions)
      where T : SerializableExpression
    {
      return expressions.Select(e => Visit(e));
    }

    #endregion

    public SerializableExpressionToExpressionConverter(SerializableExpression source)
    {
      this.source = source;
      cache = new Dictionary<SerializableExpression, Expression>();
    }
  }
}