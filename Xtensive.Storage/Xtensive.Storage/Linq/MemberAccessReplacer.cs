// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  internal class MemberAccessReplacer : MemberPathVisitor
  {
    private readonly TranslatorContext context;
    private ParameterExpression resultParameter;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo genericAccessor;
    private static readonly PropertyInfo keyValueAccessor;
    private static readonly PropertyInfo keyAccessor;

    public LambdaExpression ProcessPredicate(LambdaExpression le)
    {
      resultParameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(le.Body);
      var lambda = Expression.Lambda(typeof(Func<Tuple, bool>), body, resultParameter);
      return lambda;
    }

    public LambdaExpression ProcessCalculated(LambdaExpression le)
    {
      resultParameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(le.Body);
      var lambda = Expression.Lambda(typeof(Func<Tuple, object>), Expression.Convert(body, typeof(object)), resultParameter);
      return lambda;
    }

    public LambdaExpression ProcessCalculated(Expression e)
    {
      e = e.NodeType == ExpressionType.Lambda ? ((LambdaExpression)e).Body : e;
      resultParameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(e);
      var lambda = Expression.Lambda(typeof(Func<Tuple, object>), Expression.Convert(body, typeof(object)), resultParameter);
      return lambda;
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (context.ParameterExtractor.IsParameter(e))
          return e;
        return context.Evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      var rewriter = new ParameterRewriter(resultParameter, null);
      var source = context.GetBound(p);
      return rewriter.Rewrite(source.ItemProjector.Body).First;
    }

    protected override Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind)
    {
      throw new System.NotImplementedException();
    }

    protected override Expression VisitMemberPath(MemberPath path, Expression e)
    {
      var method = e.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(e.Type);
      if (path.PathType != MemberType.Primitive)
        throw new NotSupportedException();
      var source = context.GetBound(path.Parameter);
      return Expression.Call(resultParameter, method, Expression.Constant(source.GetMemberSegment(path).Offset));
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var memberType = b.Left.GetMemberType();
      switch (memberType) {
        case MemberType.Unknown:
        case MemberType.Primitive:
          break;
        case MemberType.Key:
        case MemberType.Entity:
        case MemberType.Anonymous: 
        case MemberType.Structure: {
          Expression result = null;
          bool isKey = memberType == MemberType.Key;
          bool isEntity = memberType == MemberType.Entity;
          bool isAnonymous = memberType == MemberType.Anonymous;
          bool leftIsParameter = context.ParameterExtractor.IsParameter(b.Left);
          bool rightIsParameter = context.ParameterExtractor.IsParameter(b.Right);
          if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual)
            throw new NotSupportedException();
          if (isKey) {
            if (!leftIsParameter && !rightIsParameter)
              result = MakeComplexBinaryExpression(b.Left, b.Right, b.NodeType);
            else {
              var bLeft = b.Left;
              var bRight = b.Right;
              if (leftIsParameter) {
                bLeft = b.Right;
                bRight = b.Left;
              }
              var path = MemberPath.Parse(bLeft, context.Model);
              var source = context.GetBound(path.Parameter);
              var segment = source.GetMemberSegment(path);
              foreach (var pair in segment.GetItems().Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
                Expression left = Expression.Call(resultParameter, nonGenericAccessor, Expression.Constant(pair.ColumnIndex));
                Expression right = Expression.Condition(
                  Expression.Equal(bRight, Expression.Constant(null, bRight.Type)),
                  Expression.Constant(null, typeof (object)),
                  Expression.Call(Expression.MakeMemberAccess(bRight, keyValueAccessor), nonGenericAccessor, Expression.Constant(pair.ParameterIndex)));
                result = MakeBinaryExpression(result, left, right, b.NodeType);
              }
            }
          }
          else if (isEntity) {
            if (!leftIsParameter && !rightIsParameter) {
              var bLeft = b.Left.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Left).Value == null ? b.Left : Expression.MakeMemberAccess(b.Left, keyAccessor);
              var bRight = b.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Right).Value == null ? b.Right : Expression.MakeMemberAccess(b.Right, keyAccessor);
              result = MakeComplexBinaryExpression(bLeft, bRight, b.NodeType);
            }
            else {
              var bLeft = Expression.MakeMemberAccess(b.Left, keyAccessor);
              var bRight = b.Right;
              if (leftIsParameter) {
                bLeft = Expression.MakeMemberAccess(b.Right, keyAccessor);
                bRight = b.Left;
              }
              var path = MemberPath.Parse(bLeft, context.Model);
              var source = context.GetBound(path.Parameter);
              var segment = source.GetMemberSegment(path);
              foreach (var pair in segment.GetItems().Select((ci, pi) => new { ColumnIndex = ci, ParameterIndex = pi })) {
                Expression left = Expression.Call(resultParameter, nonGenericAccessor, Expression.Constant(pair.ColumnIndex));
                Expression right = Expression.Condition(
                  Expression.Equal(bRight, Expression.Constant(null, bRight.Type)),
                  Expression.Constant(null, typeof(object)),
                  Expression.Call(
                    Expression.MakeMemberAccess(Expression.MakeMemberAccess(bRight, keyAccessor), keyValueAccessor),
                    nonGenericAccessor, Expression.Constant(pair.ParameterIndex)));
                result = MakeBinaryExpression(result, left, right, b.NodeType);
              }
            }
          }
          else if (isAnonymous) {
            if (!leftIsParameter && !rightIsParameter)
              return MakeComplexBinaryExpression(b.Left, b.Right, b.NodeType);
            throw new NotSupportedException();
          }
          else {
            if (!leftIsParameter && !rightIsParameter)
              return MakeComplexBinaryExpression(b.Left, b.Right, b.NodeType);
            throw new NotSupportedException();
          }
          return result;
        }
        case MemberType.EntitySet:
          throw new NotSupportedException();
        default:
          throw new ArgumentOutOfRangeException();
      }
      return base.VisitBinary(b);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right, ExpressionType operationType)
    {
      if (previous == null) {
        previous = operationType == ExpressionType.Equal
          ? Expression.Equal(left, right)
          : Expression.NotEqual(left, right);
      }
      else {
        previous = operationType == ExpressionType.Equal
          ? Expression.AndAlso(previous, Expression.Equal(left, right))
          : Expression.AndAlso(previous, Expression.NotEqual(left, right));
      }
      return previous;
    }

    private Expression MakeComplexBinaryExpression(Expression bLeft, Expression bRight, ExpressionType operationType)
    {
      Expression result = null;
      if (bLeft.NodeType == ExpressionType.Constant || bRight.NodeType == ExpressionType.Constant) {
        var constant = bLeft.NodeType == ExpressionType.Constant
          ? (ConstantExpression)bLeft
          : (ConstantExpression)bRight;
        var member = bLeft.NodeType != ExpressionType.Constant
          ? bLeft
          : bRight;
        if (constant.Value == null) {
          var path = MemberPath.Parse(member, context.Model);
          var source = context.GetBound(path.Parameter);
          var segment = source.GetMemberSegment(path);
          foreach (var i in segment.GetItems()) {
            Expression left = Expression.Call(resultParameter, nonGenericAccessor, Expression.Constant(i));
            Expression right = Expression.Constant(null);
            result = MakeBinaryExpression(result, left, right, operationType);
          }
          return result;
        }
      }
      var leftPath = MemberPath.Parse(bLeft, context.Model);
      var leftSource = context.GetBound(leftPath.Parameter);
      var leftSegment = leftSource.GetMemberSegment(leftPath);
      var rightPath = MemberPath.Parse(bRight, context.Model);
      var rightSource = context.GetBound(rightPath.Parameter);
      var rightSegment = rightSource.GetMemberSegment(rightPath);
      foreach (var pair in leftSegment.GetItems().ZipWith(rightSegment.GetItems(), (l,r) => new {l,r})) {
        var method = genericAccessor.MakeGenericMethod(leftSource.RecordSet.Header.TupleDescriptor[pair.l]);
        Expression left = Expression.Call(resultParameter, method, Expression.Constant(pair.l));
        Expression right = Expression.Call(resultParameter, method, Expression.Constant(pair.r));
        result = MakeBinaryExpression(result, left, right, operationType);
      }
      return result;
    }


    // Constructors

    public MemberAccessReplacer(TranslatorContext context)
      : base(context.Model)
    {
      this.context = context;
    }

    // Type initializer

    static MemberAccessReplacer()
    {
      keyValueAccessor = typeof (Key).GetProperty("Value");
      keyAccessor = typeof (IEntity).GetProperty("Key");
      foreach (var method in typeof(Tuple).GetMethods()) {
        if (method.Name == "GetValueOrDefault") {
          if (method.IsGenericMethod)
            genericAccessor = method;
          else
            nonGenericAccessor = method;
        }
      }
    }
  }
}
