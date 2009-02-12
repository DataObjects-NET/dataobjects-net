// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Resources;

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
    private static readonly MemberInfo identifierAccessor;

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

    protected override Expression VisitMemberPath(MemberPathExpression mpe)
    {
      var path = mpe.Path;
      var method = mpe.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(mpe.Type);
      // TODO: handle structures
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
          bool isKey = memberType == MemberType.Key;
          bool isEntity = memberType == MemberType.Entity;
          bool isAnonymous = memberType == MemberType.Anonymous;
          bool leftIsParameter = context.ParameterExtractor.IsParameter(b.Left);
          bool rightIsParameter = context.ParameterExtractor.IsParameter(b.Right);
          if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual)
            throw new NotSupportedException();
          if (isKey) {
            Expression result = null;
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
              foreach (var pair in Enumerable.Range(segment.Offset, segment.Length).Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
                Expression left = Expression.Call(resultParameter, nonGenericAccessor, Expression.Constant(pair.ColumnIndex));
                Expression right = Expression.Condition(
                  Expression.Equal(bRight, Expression.Constant(null, bRight.Type)),
                  Expression.Constant(null, typeof (object)),
                  Expression.Call(Expression.MakeMemberAccess(bRight, keyValueAccessor), nonGenericAccessor, Expression.Constant(pair.ParameterIndex)));
                result = MakeBinaryExpression(result, left, right, b.NodeType);
              }
            }
            return result;
          }
          else if (isEntity) {
            Expression result = null;
            if (!leftIsParameter && !rightIsParameter) {
              var bLeft = Expression.MakeMemberAccess(b.Left, keyAccessor);
              var bRight = Expression.MakeMemberAccess(b.Right, keyAccessor);
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
              foreach (var pair in Enumerable.Range(segment.Offset, segment.Length).Select((ci, pi) => new { ColumnIndex = ci, ParameterIndex = pi })) {
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
            return result;
          }
          else if (isAnonymous) {
            
          }
          else {
            throw new NotImplementedException();
          }
          throw new NotImplementedException();
        }
        case MemberType.EntitySet:
          throw new NotSupportedException();
        default:
          throw new ArgumentOutOfRangeException();
      }

//      bool isKey = typeof(Key).IsAssignableFrom(b.Left.Type);
//      bool isEntity = typeof(IEntity).IsAssignableFrom(b.Left.Type);
//      bool isStructure = typeof(Structure).IsAssignableFrom(b.Left.Type);
//      bool leftIsParameter = context.ParameterExtractor.IsParameter(b.Left);
//      bool rightIsParameter = context.ParameterExtractor.IsParameter(b.Right);
//      var first = b.Left;
//      var second = b.Right;
//      if (isKey || isEntity || isStructure) {
//        if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual) 
//          throw new InvalidOperationException();
//        if (isStructure)
//          throw new NotImplementedException();
//        if (!leftIsParameter && !rightIsParameter) {
//          throw new NotImplementedException();
//        }
//        if (leftIsParameter) {
//          first = b.Right;
//          second = b.Left;
//        }
//        Expression result = null;
//        var key = isKey ? second : Expression.MakeMemberAccess(second, identifierAccessor);
//        first = isKey ? first : Expression.MakeMemberAccess(first, keyAccessor);
//        var path = MemberPath.Parse(first, context.Model);
//        var source = context.GetBound(path.Parameter);
//        var segment = source.GetMemberSegment(path);
//        if (segment.Length == 0)
//          throw new InvalidOperationException();
//        foreach (var pair in Enumerable.Range(segment.Offset, segment.Length).Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
//          var method = genericAccessor.MakeGenericMethod(source.RecordSet.Header.TupleDescriptor[segment.Offset]);
//          Expression left = Expression.Call(resultParameter, method, Expression.Constant(pair.ColumnIndex));
//          Expression right = Expression.Call(Expression.MakeMemberAccess(key, keyValueAccessor), method, Expression.Constant(pair.ParameterIndex));
//          if (result == null) {
//            result = b.NodeType == ExpressionType.Equal ?
//              Expression.Equal(left, right) :
//              Expression.NotEqual(left, right);
//          }
//          else {
//            result = b.NodeType == ExpressionType.Equal ?
//              Expression.AndAlso(result, Expression.Equal(left, right)) :
//              Expression.AndAlso(result, Expression.NotEqual(left, right));
//          }
//        }
//        return result;
//      }
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
      var leftPath = MemberPath.Parse(bLeft, context.Model);
      var leftSource = context.GetBound(leftPath.Parameter);
      var leftSegment = leftSource.GetMemberSegment(leftPath);
      var rightPath = MemberPath.Parse(bRight, context.Model);
      var rightSource = context.GetBound(rightPath.Parameter);
      var rightSegment = rightSource.GetMemberSegment(rightPath);
      for (int i = leftSegment.Offset, j = rightSegment.Offset;
        i < leftSegment.EndOffset && j < rightSegment.EndOffset;
        i++, j++) 
      {
        var method = genericAccessor.MakeGenericMethod(leftSource.RecordSet.Header.TupleDescriptor[i]);
        Expression left = Expression.Call(resultParameter, method, Expression.Constant(i));
        Expression right = Expression.Call(resultParameter, method, Expression.Constant(j));
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
      identifierAccessor = typeof (IIdentified<Key>).GetMember("Identifier")[0];
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
