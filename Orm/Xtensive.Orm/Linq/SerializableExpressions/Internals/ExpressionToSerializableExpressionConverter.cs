// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq.SerializableExpressions.Internals
{
  internal sealed class ExpressionToSerializableExpressionConverter
    : ExpressionVisitor<SerializableExpression>
  {
    private readonly Expression source;

    public SerializableExpression Convert()
    {
      return Visit(source);
    }

    #region ExpressionVisitor<SerializableExpression>

    protected override SerializableUnaryExpression VisitUnary(UnaryExpression u)
    {
      return new SerializableUnaryExpression
        {
          NodeType = u.NodeType,
          Type = u.Type,
          Method = u.Method,
          Operand = Visit(u.Operand)
        };
    }

    protected override SerializableBinaryExpression VisitBinary(BinaryExpression b)
    {
      return new SerializableBinaryExpression
        {
          NodeType = b.NodeType,
          Type = b.Type,
          Method = b.Method,
          IsLiftedToNull = b.IsLiftedToNull,
          Left = Visit(b.Left),
          Right = Visit(b.Right)
        };
    }

    protected override SerializableTypeBinaryExpression VisitTypeIs(TypeBinaryExpression tb)
    {
      return new SerializableTypeBinaryExpression
        {
          NodeType = tb.NodeType,
          Type = tb.Type,
          Expression = Visit(tb.Expression),
          TypeOperand = tb.TypeOperand
        };
    }

    protected override SerializableConstantExpression VisitConstant(ConstantExpression c)
    {
      return new SerializableConstantExpression
        {
          NodeType = c.NodeType,
          Type = c.Type,
          Value = c.Value
        };
    }

    protected override SerializableDefaultExpression VisitDefault(DefaultExpression d)
    {
      return new SerializableDefaultExpression {
        NodeType = d.NodeType,
        Type = d.Type
      };
    }

    protected override SerializableConditionalExpression VisitConditional(ConditionalExpression c)
    {
      return new SerializableConditionalExpression
        {
          NodeType = c.NodeType,
          Type = c.Type,
          Test = Visit(c.Test),
          IfTrue = Visit(c.IfTrue),
          IfFalse = Visit(c.IfFalse)
        };
    }

    protected override SerializableParameterExpression VisitParameter(ParameterExpression p)
    {
      return new SerializableParameterExpression
        {
          NodeType = p.NodeType,
          Type = p.Type,
          Name = p.Name
        };
    }

    protected override SerializableMemberExpression VisitMemberAccess(MemberExpression m)
    {
      return new SerializableMemberExpression
        {
          NodeType = m.NodeType,
          Type = m.Type,
          Expression = Visit(m.Expression),
          Member = m.Member
        };
    }

    protected override SerializableMethodCallExpression VisitMethodCall(MethodCallExpression mc)
    {
      return new SerializableMethodCallExpression
        {
          NodeType = mc.NodeType,
          Type = mc.Type,
          Method = mc.Method,
          Arguments = VisitExpressionSequence(mc.Arguments),
          Object = Visit(mc.Object)
        };
    }

    protected override SerializableLambdaExpression VisitLambda(LambdaExpression l)
    {
      return new SerializableLambdaExpression
        {
          NodeType = l.NodeType,
          Type = l.Type,
          Body = Visit(l.Body),
          Parameters = l.Parameters.Select(p => (SerializableParameterExpression) Visit(p)).ToArraySafely()
        };
    }

    protected override SerializableNewExpression VisitNew(NewExpression n)
    {
      return new SerializableNewExpression
        {
          NodeType = n.NodeType,
          Type = n.Type,
          Constructor = n.Constructor,
          Arguments = VisitExpressionSequence(n.Arguments),
          Members = n.Members.ToArraySafely()
        };
    }

    protected override SerializableMemberInitExpression VisitMemberInit(MemberInitExpression mi)
    {
      return new SerializableMemberInitExpression
        {
          NodeType = mi.NodeType,
          Type = mi.Type,
          Bindings = VisitMemberBindingSequence(mi.Bindings),
          NewExpression = (SerializableNewExpression) Visit(mi.NewExpression)
        };
    }

    protected override SerializableListInitExpression VisitListInit(ListInitExpression li)
    {
      return new SerializableListInitExpression
        {
          NodeType = li.NodeType,
          Type = li.Type,
          NewExpression = (SerializableNewExpression) Visit(li.NewExpression),
          Initializers = VisitElementInitSequence(li.Initializers)
        };
    }

    protected override SerializableNewArrayExpression VisitNewArray(NewArrayExpression na)
    {
      return new SerializableNewArrayExpression
        {
          NodeType = na.NodeType,
          Type = na.Type,
          Expressions = VisitExpressionSequence(na.Expressions)
        };
    }

    protected override SerializableInvocationExpression VisitInvocation(InvocationExpression i)
    {
      return new SerializableInvocationExpression
        {
          NodeType = i.NodeType,
          Type = i.Type,
          Arguments = VisitExpressionSequence(i.Arguments),
          Expression = Visit(i.Expression)
        };
    }

    #endregion

    #region Private / internal methods

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
            Arguments = VisitExpressionSequence(initializer.Arguments)
          })
        .ToArray();
    }

    private SerializableExpression[] VisitExpressionSequence<T>(IEnumerable<T> expressions)
      where T : Expression
    {
      return expressions.Select(e => Visit(e)).ToArray();
    }

    #endregion

    public ExpressionToSerializableExpressionConverter(Expression source)
      : base(true)
    {
      this.source = source;
    }
  }
}