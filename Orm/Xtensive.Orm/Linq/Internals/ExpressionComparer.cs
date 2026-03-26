// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class ExpressionComparer
  {
    private readonly ParameterExpressionRegistry leftParameters = new ParameterExpressionRegistry();
    private readonly ParameterExpressionRegistry rightParameters = new ParameterExpressionRegistry();

    public bool AreEqual(Expression x, Expression y)
    {
      try {
        return Visit(x, y);
      }
      finally {
        leftParameters.Reset();
        rightParameters.Reset();
      }
    }

    #region Private / internal methods

    // Slightly copy-pasted from ExpressionVisitor, so keep in sync.
    // Also keep in sync with ExpressionHashCodeCalculator, because GetHashCode and Equals should be synchronized.
    private bool Visit(Expression x, Expression y)
    {
      if (ReferenceEquals(x, y))
        return true;
      if (x==null || y==null)
        return false;
      if (x.NodeType!=y.NodeType)
        return false;
      if (x.Type!=y.Type)
        return false;

      switch (x.NodeType) {
      case ExpressionType.Negate:
      case ExpressionType.NegateChecked:
      case ExpressionType.Not:
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
      case ExpressionType.ArrayLength:
      case ExpressionType.Quote:
      case ExpressionType.TypeAs:
        return VisitUnary((UnaryExpression) x, (UnaryExpression) y);
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
        return VisitBinary((BinaryExpression) x, (BinaryExpression) y);
      case ExpressionType.TypeIs:
        return VisitTypeIs((TypeBinaryExpression) x, (TypeBinaryExpression) y);
      case ExpressionType.Conditional:
        return VisitConditional((ConditionalExpression) x, (ConditionalExpression) y);
      case ExpressionType.Constant:
        return VisitConstant((ConstantExpression) x, (ConstantExpression) y);
      case ExpressionType.Parameter:
        return VisitParameter((ParameterExpression) x, (ParameterExpression) y);
      case ExpressionType.MemberAccess:
        return VisitMemberAccess((MemberExpression) x, (MemberExpression) y);
      case ExpressionType.Call:
        return VisitMethodCall((MethodCallExpression) x, (MethodCallExpression) y);
      case ExpressionType.Lambda:
        return VisitLambda((LambdaExpression) x, (LambdaExpression) y);
      case ExpressionType.New:
        return VisitNew((NewExpression) x, (NewExpression) y);
      case ExpressionType.NewArrayInit:
      case ExpressionType.NewArrayBounds:
        return VisitNewArray((NewArrayExpression) x, (NewArrayExpression) y);
      case ExpressionType.Invoke:
        return VisitInvocation((InvocationExpression) x, (InvocationExpression) y);
      case ExpressionType.MemberInit:
        return VisitMemberInit((MemberInitExpression) x, (MemberInitExpression) y);
      case ExpressionType.ListInit:
        return VisitListInit((ListInitExpression) x, (ListInitExpression) y);
      default:
        return x.Equals(y);
      }
    }

    private bool VisitListInit(ListInitExpression x, ListInitExpression y)
    {
      return VisitNew(x.NewExpression, y.NewExpression)
        && x.Initializers.Count==y.Initializers.Count
        && x.Initializers
          .Zip(y.Initializers)
          .All(p => VisitElementInit(p.First, p.Second));
    }

    /// <summary>
    /// Visits the member init.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    private bool VisitMemberInit(MemberInitExpression x, MemberInitExpression y)
    {
      return VisitNew(x.NewExpression, y.NewExpression)
        && x.Bindings.Count==y.Bindings.Count
        && x.Bindings
          .Zip(y.Bindings)
          .All(p => VisitMemberBinding(p.First, p.Second));
    }

    private bool VisitMemberBinding(MemberBinding x, MemberBinding y)
    {
      var result = x.BindingType==y.BindingType && x.Member==y.Member;
      if (!result)
        return false;
      switch (x.BindingType) {
        case MemberBindingType.Assignment:
          var ax = (MemberAssignment)x;
          var ay = (MemberAssignment)y;
          return Visit(ax.Expression, ay.Expression);
        case MemberBindingType.MemberBinding:
          var mbx = (MemberMemberBinding)x;
          var mby = (MemberMemberBinding)y;
          return mbx.Bindings.Count==mby.Bindings.Count
                 && mbx.Bindings
                    .Zip(mby.Bindings)
                    .All(p => VisitMemberBinding(p.First, p.Second));
        case MemberBindingType.ListBinding:
          var mlx = (MemberListBinding)x;
          var mly = (MemberListBinding)y;
          return mlx.Initializers.Count==mly.Initializers.Count
                 && mlx.Initializers
                    .Zip(mly.Initializers)
                    .All(p => VisitElementInit(p.First, p.Second));
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool VisitElementInit(ElementInit x, ElementInit y)
    {
      return x.AddMethod==y.AddMethod && CompareExpressionSequences(x.Arguments, y.Arguments);
    }

    private bool VisitInvocation(InvocationExpression x, InvocationExpression y)
    {
      return Visit(x.Expression, y.Expression) && CompareExpressionSequences(x.Arguments, y.Arguments);
    }

    private bool VisitNewArray(NewArrayExpression x, NewArrayExpression y)
    {
      return CompareExpressionSequences(x.Expressions, y.Expressions);
    }

    private bool VisitNew(NewExpression x, NewExpression y)
    {
      if (x.Constructor!=y.Constructor)
        return false;
      if (!CompareExpressionSequences(x.Arguments, y.Arguments))
        return false;
      if (x.Members == null)
        if (y.Members == null)
          return true;
        else
          return false;
      if (y.Members == null)
        return false;
      if (x.Members.Count != y.Members.Count)
        return false;
      for (int i = 0, count = x.Members.Count; i < count; i++)
        if (x.Members[i] != y.Members[i])
          return false;
      return true;
    }

    private bool VisitLambda(LambdaExpression x, LambdaExpression y)
    {
      leftParameters.AddRange(x.Parameters);
      rightParameters.AddRange(y.Parameters);
      return CompareExpressionSequences(x.Parameters, y.Parameters) && Visit(x.Body, y.Body);
    }

    private bool VisitMethodCall(MethodCallExpression x, MethodCallExpression y)
    {
      return x.Method==y.Method && Visit(x.Object, y.Object) && CompareExpressionSequences(x.Arguments, y.Arguments);
    }

    private bool VisitMemberAccess(MemberExpression x, MemberExpression y)
    {
      return x.Member==y.Member && Visit(x.Expression, y.Expression);
    }

    private bool VisitParameter(ParameterExpression x, ParameterExpression y)
    {
      return leftParameters.GetIndex(x)==rightParameters.GetIndex(y);
    }

    private bool VisitConstant(ConstantExpression x, ConstantExpression y)
    {
      if (ReferenceEquals(x.Value, y.Value))
        return true;
      if (x.Value == null || y.Value == null)
        return false;
      return x.Value.Equals(y.Value);
    }

    private bool VisitConditional(ConditionalExpression x, ConditionalExpression y)
    {
      return Visit(x.Test, y.Test) && Visit(x.IfTrue, y.IfTrue) && Visit(x.IfFalse, y.IfFalse);
    }

    private bool VisitTypeIs(TypeBinaryExpression x, TypeBinaryExpression y)
    {
      return x.TypeOperand==y.TypeOperand && Visit(x.Expression, y.Expression);
    }

    private bool VisitBinary(BinaryExpression x, BinaryExpression y)
    {
      return x.Method==y.Method && Visit(x.Left, y.Left) && Visit(x.Right, y.Right);
    }

    private bool VisitUnary(UnaryExpression x, UnaryExpression y)
    {
      return x.Method==y.Method && Visit(x.Operand, y.Operand);
    }

    private bool CompareExpressionSequences<T>(
      System.Collections.ObjectModel.ReadOnlyCollection<T> x,
      System.Collections.ObjectModel.ReadOnlyCollection<T> y)
      where T : Expression
    {
      if (x.Count != y.Count)
        return false;
      for (int i = 0, count = x.Count; i < count; i++)
        if (!Visit(x[i], y[i]))
          return false;
      return true;
    }

    #endregion
  }
}
