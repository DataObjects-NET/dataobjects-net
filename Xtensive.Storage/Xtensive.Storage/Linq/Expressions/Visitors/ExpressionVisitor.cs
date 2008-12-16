// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  public abstract class ExpressionVisitor : Visitor<Expression>
  {
    protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
      ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
      if (arguments != initializer.Arguments) {
        return Expression.ElementInit(initializer.AddMethod, arguments);
      }
      return initializer;
    }

    protected virtual ReadOnlyCollection<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
      var results = new List<ElementInit>();
      for (int i = 0, n = original.Count; i < n; i++) {
        ElementInit p = VisitElementInitializer(original[i]);
        results.Add(p);
      }
      return results.AsReadOnly();
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      Expression operand = Visit(u.Operand);
      if (operand!=u.Operand)
        return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
      return u;

    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      Expression left = Visit(b.Left);
      Expression right = Visit(b.Right);
      if ((left==b.Left) && (right==b.Right))
        return b;
      return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);

    }

    protected override Expression VisitTypeIs(TypeBinaryExpression b)
    {
      Expression expression = Visit(b.Expression);
      if (expression!=b.Expression)
        return Expression.TypeIs(expression, b.TypeOperand);
      return b;

    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      return c;
    }

    protected override Expression VisitConditional(ConditionalExpression c)
    {
      Expression test = Visit(c.Test);
      Expression ifTrue = Visit(c.IfTrue);
      Expression ifFalse = Visit(c.IfFalse);
      if (((test==c.Test) && (ifTrue==c.IfTrue)) && (ifFalse==c.IfFalse))
        return c;
      return Expression.Condition(test, ifTrue, ifFalse);

    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      return p;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      Expression expression = Visit(m.Expression);
      if (expression!=m.Expression)
        return Expression.MakeMemberAccess(expression, m.Member);
      return m;

    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      Expression instance = Visit(m.Object);
      IEnumerable<Expression> arguments = VisitExpressionList(m.Arguments);
      if ((instance==m.Object) && (arguments==m.Arguments))
        return m;
      return Expression.Call(instance, m.Method, arguments);
    }


    protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      Expression expression = Visit(assignment.Expression);
      if (expression!=assignment.Expression)
        return Expression.Bind(assignment.Member, expression);
      return assignment;

    }

    #region Member bindings methods

    protected virtual MemberBinding VisitBinding(MemberBinding binding)
    {
      switch (binding.BindingType) {
      case MemberBindingType.Assignment:
        return VisitMemberAssignment((MemberAssignment)binding);
      case MemberBindingType.MemberBinding:
        return VisitMemberMemberBinding((MemberMemberBinding)binding);
      case MemberBindingType.ListBinding:
        return VisitMemberListBinding((MemberListBinding)binding);
      default:
        throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
      }
    }

    protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
      if (bindings != binding.Bindings)
      {
        return Expression.MemberBind(binding.Member, bindings);
      }
      return binding;

    }

    protected virtual ReadOnlyCollection<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
      var results = new List<MemberBinding>();
      for (int i = 0, n = original.Count; i < n; i++) {
        MemberBinding p = VisitBinding(original[i]);
        results.Add(p);
      }
      return results.AsReadOnly();
    }

    protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
      if (initializers!=binding.Initializers)
        return Expression.ListBind(binding.Member, initializers);
      return binding;

    }

    #endregion

    protected override Expression VisitLambda(LambdaExpression lambda)
    {
      Expression body = Visit(lambda.Body);
      if (body!=lambda.Body)
        return Expression.Lambda(lambda.Type, body, lambda.Parameters);
      return lambda;

    }

    protected override Expression VisitNew(NewExpression nex)
    {
      IEnumerable<Expression> arguments = VisitExpressionList(nex.Arguments);
      if (arguments==nex.Arguments)
        return nex;
      if (nex.Members!=null)
        return Expression.New(nex.Constructor, arguments, nex.Members);
      return Expression.New(nex.Constructor, arguments);
    }

    protected override Expression VisitMemberInit(MemberInitExpression init)
    {
      var newExpression = (NewExpression)VisitNew(init.NewExpression);
      IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
      if ((newExpression==init.NewExpression) && (bindings==init.Bindings))
        return init;
      return Expression.MemberInit(newExpression, bindings);

    }

    protected override Expression VisitListInit(ListInitExpression init)
    {
      var newExpression = (NewExpression) VisitNew(init.NewExpression);
      IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
      if ((newExpression==init.NewExpression) && (initializers==init.Initializers))
        return init;
      return Expression.ListInit(newExpression, initializers);

    }

    protected override Expression VisitNewArray(NewArrayExpression na)
    {
      IEnumerable<Expression> initializers = VisitExpressionList(na.Expressions);
      if (initializers==na.Expressions)
        return na;
      if (na.NodeType==ExpressionType.NewArrayInit)
        return Expression.NewArrayInit(na.Type.GetElementType(), initializers);
      return Expression.NewArrayBounds(na.Type.GetElementType(), initializers);
    }

    protected override Expression VisitInvocation(InvocationExpression iv)
    {
      IEnumerable<Expression> arguments = VisitExpressionList(iv.Arguments);
      Expression expression = Visit(iv.Expression);
      if ((arguments==iv.Arguments) && (expression==iv.Expression))
        return iv;
      return Expression.Invoke(expression, arguments);

    }
  }
}