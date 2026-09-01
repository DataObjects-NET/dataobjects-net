// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.SerializableExpressions.Internals
{
  internal sealed class ExpressionToSerializableExpressionConverter
  //  : Xtensive.Linq.ExpressionVisitor<SerializableExpression>
  {
    private readonly Dictionary<Expression, SerializableExpression> cache =  new();
    private readonly Expression source;

    public SerializableExpression Convert() => Visit(source);

    private  SerializableExpression Visit(Expression e)
    {
      if (e == null)
        return default(SerializableExpression);

      if (cache.TryGetValue(e, out var result))
        return result;

      switch (e.NodeType) {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.ArrayLength:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
        case ExpressionType.Decrement:
        case ExpressionType.Increment:
        case ExpressionType.IsFalse:
        case ExpressionType.IsTrue:
        case ExpressionType.OnesComplement:
          result = VisitUnary((UnaryExpression) e);
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
        case ExpressionType.Power:
        case ExpressionType.Assign:
          result = VisitBinary((BinaryExpression) e);
          break;
        case ExpressionType.TypeIs:
        case ExpressionType.TypeEqual:
          result = VisitTypeIs((TypeBinaryExpression) e);
          break;
        case ExpressionType.Conditional:
          result = VisitConditional((ConditionalExpression) e);
          break;
        case ExpressionType.Constant:
          result = VisitConstant((ConstantExpression) e);
          break;
        case ExpressionType.Default:
          result = VisitDefault((DefaultExpression) e);
          break;
        case ExpressionType.Parameter:
          result = VisitParameter((ParameterExpression) e);
          break;
        case ExpressionType.MemberAccess:
          result = VisitMemberAccess((MemberExpression) e);
          break;
        case ExpressionType.Call:
          result = VisitMethodCall((MethodCallExpression) e);
          break;
        case ExpressionType.Lambda:
          result = VisitLambda((LambdaExpression) e);
          break;
        case ExpressionType.New:
          result = VisitNew((NewExpression) e);
          break;
        case ExpressionType.NewArrayInit:
        case ExpressionType.NewArrayBounds:
          result = VisitNewArray((NewArrayExpression) e);
          break;
        case ExpressionType.Invoke:
          result = VisitInvocation((InvocationExpression) e);
          break;
        case ExpressionType.MemberInit:
          result = VisitMemberInit((MemberInitExpression) e);
          break;
        case ExpressionType.ListInit:
          result = VisitListInit((ListInitExpression) e);
          break;
        default:
          result = VisitUnknown(e);
          break;
      }

      cache[e] = result;
      return result;
    }

    #region Visitors

    private SerializableUnaryExpression VisitUnary(UnaryExpression u)
    {
      return new SerializableUnaryExpression {
        NodeType = u.NodeType,
        Type = u.Type,
        Method = u.Method,
        Operand = Visit(u.Operand)
      };
    }

    private SerializableBinaryExpression VisitBinary(BinaryExpression b)
    {
      return new SerializableBinaryExpression {
        NodeType = b.NodeType,
        Type = b.Type,
        Method = b.Method,
        IsLiftedToNull = b.IsLiftedToNull,
        Left = Visit(b.Left),
        Right = Visit(b.Right)
      };
    }

    private SerializableTypeBinaryExpression VisitTypeIs(TypeBinaryExpression tb)
    {
      return new SerializableTypeBinaryExpression {
        NodeType = tb.NodeType,
        Type = tb.Type,
        Expression = Visit(tb.Expression),
        TypeOperand = tb.TypeOperand
      };
    }

    private SerializableConstantExpression VisitConstant(ConstantExpression c)
    {
      return new SerializableConstantExpression {
        NodeType = c.NodeType,
        Type = c.Type,
        Value = c.Value
      };
    }

    private SerializableDefaultExpression VisitDefault(DefaultExpression d)
    {
      return new SerializableDefaultExpression {
        NodeType = d.NodeType,
        Type = d.Type
      };
    }

    private SerializableConditionalExpression VisitConditional(ConditionalExpression c)
    {
      return new SerializableConditionalExpression {
        NodeType = c.NodeType,
        Type = c.Type,
        Test = Visit(c.Test),
        IfTrue = Visit(c.IfTrue),
        IfFalse = Visit(c.IfFalse)
      };
    }

    private SerializableParameterExpression VisitParameter(ParameterExpression p)
    {
      return new SerializableParameterExpression {
        NodeType = p.NodeType,
        Type = p.Type,
        Name = p.Name
      };
    }

    private SerializableMemberExpression VisitMemberAccess(MemberExpression m)
    {
      return new SerializableMemberExpression {
        NodeType = m.NodeType,
        Type = m.Type,
        Expression = Visit(m.Expression),
        Member = m.Member
      };
    }

    private SerializableMethodCallExpression VisitMethodCall(MethodCallExpression mc)
    {
      return new SerializableMethodCallExpression {
        NodeType = mc.NodeType,
        Type = mc.Type,
        Method = mc.Method,
        Arguments = VisitExpressionList(mc.Arguments),
        Object = Visit(mc.Object)
      };
    }

    private SerializableLambdaExpression VisitLambda(LambdaExpression l)
    {
      return new SerializableLambdaExpression {
        NodeType = l.NodeType,
        Type = l.Type,
        Body = Visit(l.Body),
        Parameters = (l.Parameters.Count == 0)
            ? Array.Empty<SerializableParameterExpression>()
            : l.Parameters.Select(p => (SerializableParameterExpression) Visit(p)).ToArray()
      };
    }

    private SerializableNewExpression VisitNew(NewExpression n)
    {
      return new SerializableNewExpression {
        NodeType = n.NodeType,
        Type = n.Type,
        Constructor = n.Constructor,
        Arguments = VisitExpressionList(n.Arguments),
        Members = n.Members?.Select(static m => (SerializableMemberInfo) m).ToArray()
      };
    }

    private SerializableMemberInitExpression VisitMemberInit(MemberInitExpression mi)
    {
      return new SerializableMemberInitExpression {
        NodeType = mi.NodeType,
        Type = mi.Type,
        Bindings = VisitMemberBindingSequence(mi.Bindings),
        NewExpression = (SerializableNewExpression) Visit(mi.NewExpression)
      };
    }

    private SerializableListInitExpression VisitListInit(ListInitExpression li)
    {
      return new SerializableListInitExpression {
        NodeType = li.NodeType,
        Type = li.Type,
        NewExpression = (SerializableNewExpression) Visit(li.NewExpression),
        Initializers = VisitElementInitSequence(li.Initializers)
      };
    }

    private SerializableNewArrayExpression VisitNewArray(NewArrayExpression na)
    {
      return new SerializableNewArrayExpression {
        NodeType = na.NodeType,
        Type = na.Type,
        Expressions = VisitExpressionList(na.Expressions)
      };
    }

    private SerializableInvocationExpression VisitInvocation(InvocationExpression i)
    {
      return new SerializableInvocationExpression {
        NodeType = i.NodeType,
        Type = i.Type,
        Arguments = VisitExpressionList(i.Arguments),
        Expression = Visit(i.Expression)
      };
    }

    private SerializableExpression VisitUnknown(Expression e)
    {
      throw new NotSupportedException($"Unknown Expression : {e.GetType().GetShortName()} ({e.NodeType})");
    }

    #endregion

    #region Helper methods

    private SerializableMemberBinding[] VisitMemberBindingSequence(IEnumerable<MemberBinding> bindings)
    {
      var result = new List<SerializableMemberBinding>();
      foreach (var binding in bindings)
        switch (binding.BindingType) {
        case MemberBindingType.Assignment:
          result.Add(new SerializableMemberAssignment
            {
              BindingType = MemberBindingType.Assignment,
              Member = binding.Member,
              Expression = Visit(((MemberAssignment) binding).Expression)
            });
          break;
        case MemberBindingType.ListBinding:
          result.Add(new SerializableMemberListBinding
            {
              BindingType = MemberBindingType.ListBinding,
              Member = binding.Member,
              Initializers = VisitElementInitSequence(((MemberListBinding) binding).Initializers)
            });
          break;
        case MemberBindingType.MemberBinding:
          result.Add(new SerializableMemberMemberBinding
            {
              BindingType = MemberBindingType.MemberBinding,
              Member = binding.Member,
              Bindings = VisitMemberBindingSequence(((MemberMemberBinding) binding).Bindings)
            });
          break;
        default:
          throw new ArgumentOutOfRangeException();
        }
      return result.ToArray();
    }

    private SerializableElementInit[] VisitElementInitSequence(IEnumerable<ElementInit> initializers)
    {
      return initializers
        .Select(initializer => new SerializableElementInit
          {
            AddMethod = initializer.AddMethod,
            Arguments = VisitExpressionList(initializer.Arguments)
          })
        .ToArray();
    }

    private SerializableExpression[] VisitExpressionList<TExpression>(ReadOnlyCollection<TExpression> expressions)
      where TExpression : Expression
    {
      var results = new List<SerializableExpression>(expressions.Count);
      for (int i = 0, n = expressions.Count; i < n; i++) {
        var p = Visit(expressions[i]);
        results.Add(p);
      }
      return results.ToArray();
    }

    #endregion

    public ExpressionToSerializableExpressionConverter(Expression source)
      //: base(true)
    {
      this.source = source;
    }
  }
}