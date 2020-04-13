// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  /// <summary>
  /// An abstract base implementation of <see cref="ExpressionVisitor{TResult}"/>
  /// returning <see cref="Expression"/> as its visit result.
  /// </summary>
  public abstract class ExpressionVisitor : ExpressionVisitor<Expression>
  {
    protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
    {
      bool isChanged = false;
      var results = new List<Expression>(expressions.Count);
      for (int i = 0, n = expressions.Count; i < n; i++) {
        var expression = expressions[i];
        var p = Visit(expression);
        results.Add(p);
        isChanged |= !ReferenceEquals(expression, p);
      }
      return isChanged ? results.AsReadOnly() : expressions;
    }

    /// <summary>
    /// Visits the element initializer expression.
    /// </summary>
    /// <param name="initializer">The initializer.</param>
    /// <returns>Visit result.</returns>
    protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
      ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
      if (arguments!=initializer.Arguments) {
        return Expression.ElementInit(initializer.AddMethod, arguments);
      }
      return initializer;
    }

    /// <summary>
    /// Visits the element initializer list.
    /// </summary>
    /// <param name="original">The original element initializer list.</param>
    /// <returns>Visit result.</returns>
    protected virtual ReadOnlyCollection<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
      var results = new List<ElementInit>();
      bool isChanged = false;
      for (int i = 0, n = original.Count; i < n; i++) {
        var originalIntializer = original[i];
        ElementInit p = VisitElementInitializer(originalIntializer);
        results.Add(p);
        isChanged |= !ReferenceEquals(originalIntializer, p);
      }
      return isChanged ? results.AsReadOnly() : original;
    }

    /// <inheritdoc/>
    protected override Expression VisitUnary(UnaryExpression u)
    {
      Expression operand = Visit(u.Operand);
      if (operand!=u.Operand)
        return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
      return u;
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression b)
    {
      Expression left = Visit(b.Left);
      Expression right = Visit(b.Right);
      if ((left==b.Left) && (right==b.Right))
        return b;
      return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
    }

    /// <inheritdoc/>
    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      Expression expression = Visit(tb.Expression);
      if (expression!=tb.Expression)
        return Expression.TypeIs(expression, tb.TypeOperand);
      return tb;
    }

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression c)
    {
      return c;
    }

    /// <inheritdoc/>
    protected override Expression VisitConditional(ConditionalExpression c)
    {
      Expression test = Visit(c.Test);
      Expression ifTrue = Visit(c.IfTrue);
      Expression ifFalse = Visit(c.IfFalse);
      if (((test==c.Test) && (ifTrue==c.IfTrue)) && (ifFalse==c.IfFalse))
        return c;
      return Expression.Condition(test, ifTrue, ifFalse);
    }

    /// <inheritdoc/>
    protected override Expression VisitParameter(ParameterExpression p)
    {
      return p;
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      Expression expression = Visit(m.Expression);
      if (expression!=m.Expression)
        return Expression.MakeMemberAccess(expression, m.Member);
      return m;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      Expression instance = Visit(mc.Object);
      IEnumerable<Expression> arguments = VisitExpressionList(mc.Arguments);
      if ((instance==mc.Object) && (arguments==mc.Arguments))
        return mc;
      return Expression.Call(instance, mc.Method, arguments);
    }

    /// <summary>
    /// Visits the member assignment expression.
    /// </summary>
    /// <param name="ma">The member assignment expression.</param>
    /// <returns>Visit result.</returns>
    protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment ma)
    {
      Expression expression = Visit(ma.Expression);
      if (expression!=ma.Expression)
        return Expression.Bind(ma.Member, expression);
      return ma;
    }

    /// <inheritdoc/>
    protected override Expression VisitLambda(LambdaExpression l)
    {
      Expression body = Visit(l.Body);
      if (body!=l.Body)
        return FastExpression.Lambda(l.Type, body, l.Parameters);
      return l;
    }

    /// <inheritdoc/>
    protected override Expression VisitNew(NewExpression n)
    {
      IEnumerable<Expression> arguments = VisitExpressionList(n.Arguments);
      if (arguments==n.Arguments)
        return n;
      if (n.Members!=null)
        return Expression.New(n.Constructor, arguments, n.Members);
      return Expression.New(n.Constructor, arguments);
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberInit(MemberInitExpression mi)
    {
      var newExpression = (NewExpression) VisitNew(mi.NewExpression);
      IEnumerable<MemberBinding> bindings = VisitBindingList(mi.Bindings);
      if ((newExpression==mi.NewExpression) && (bindings==mi.Bindings))
        return mi;
      return Expression.MemberInit(newExpression, bindings);
    }

    /// <inheritdoc/>
    protected override Expression VisitListInit(ListInitExpression li)
    {
      var newExpression = (NewExpression) VisitNew(li.NewExpression);
      IEnumerable<ElementInit> initializers = VisitElementInitializerList(li.Initializers);
      if ((newExpression==li.NewExpression) && (initializers==li.Initializers))
        return li;
      return Expression.ListInit(newExpression, initializers);
    }

    /// <inheritdoc/>
    protected override Expression VisitNewArray(NewArrayExpression na)
    {
      IEnumerable<Expression> initializers = VisitExpressionList(na.Expressions);
      if (initializers==na.Expressions)
        return na;
      if (na.NodeType==ExpressionType.NewArrayInit)
        return Expression.NewArrayInit(na.Type.GetElementType(), initializers);
      return Expression.NewArrayBounds(na.Type.GetElementType(), initializers);
    }

    /// <inheritdoc/>
    protected override Expression VisitInvocation(InvocationExpression i)
    {
      IEnumerable<Expression> arguments = VisitExpressionList(i.Arguments);
      Expression expression = Visit(i.Expression);
      if ((arguments==i.Arguments) && (expression==i.Expression))
        return i;
      return Expression.Invoke(expression, arguments);
    }

    #region Member bindings methods

    /// <summary>
    /// Visits the member binding.
    /// </summary>
    /// <param name="binding">The member binding.</param>
    /// <returns>Visit result.</returns>
    protected virtual MemberBinding VisitBinding(MemberBinding binding)
    {
      switch (binding.BindingType) {
        case MemberBindingType.Assignment:
          return VisitMemberAssignment((MemberAssignment) binding);
        case MemberBindingType.MemberBinding:
          return VisitMemberMemberBinding((MemberMemberBinding) binding);
        case MemberBindingType.ListBinding:
          return VisitMemberListBinding((MemberListBinding) binding);
        default:
          throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
      }
    }

    /// <summary>
    /// Visits the member member binding.
    /// </summary>
    /// <param name="binding">The member member binding.</param>
    /// <returns>Visit result.</returns>
    protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
      if (bindings!=binding.Bindings) {
        return Expression.MemberBind(binding.Member, bindings);
      }
      return binding;
    }

    /// <summary>
    /// Visits the binding list.
    /// </summary>
    /// <param name="original">The original binding list.</param>
    /// <returns>Visit result.</returns>
    protected virtual ReadOnlyCollection<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
      var results = new List<MemberBinding>();
      bool isChanged = false;
      for (int i = 0, n = original.Count; i < n; i++) {
        var originalBinding = original[i];
        MemberBinding p = VisitBinding(originalBinding);
        results.Add(p);
        isChanged |= !ReferenceEquals(originalBinding, p);
      }
      return isChanged ? results.AsReadOnly() : original;
    }

    protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
      if (initializers!=binding.Initializers)
        return Expression.ListBind(binding.Member, initializers);
      return binding;
    }

    #endregion

    // Constructors

    /// <inheritdoc/>
    protected ExpressionVisitor()
    {
    }

    /// <inheritdoc/>
    protected ExpressionVisitor(bool isCaching)
      : base(isCaching)
    {
    }
  }
}
