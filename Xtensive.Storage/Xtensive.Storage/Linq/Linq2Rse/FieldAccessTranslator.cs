// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Model;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class FieldAccessTranslator : ExpressionVisitor
  {
    private readonly RseQueryTranslator translator;
    private ProjectionExpression source;
    private ParameterExpression parameter;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo genericAccessor;
    private static readonly PropertyInfo keyValueAccessor;
    private static readonly MemberInfo identifierAccessor;

    public LambdaExpression Translate(ProjectionExpression source, LambdaExpression le)
    {
      this.source = source;
      parameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(le.Body);
      var lambda = Expression.Lambda(typeof(Func<Tuple, bool>), body, parameter);
      return lambda;
    }

    public Deque<MappingPathItem> Translate(Expression me)
    {
      var result = new Deque<MappingPathItem>();
      string fieldName = null;
      bool isJoined = false;
      Expression expression = me;
      if (typeof(Key).IsAssignableFrom(me.Type))
        expression = ((MemberExpression)expression).Expression;
      while (expression.NodeType == ExpressionType.MemberAccess) {
        var memberAccess = (MemberExpression)expression;
        var member = (PropertyInfo)memberAccess.Member;
        expression = memberAccess.Expression;
        if (typeof (IEntity).IsAssignableFrom(memberAccess.Type)) {
          var type = translator.Model.Types[memberAccess.Type];
          if (fieldName == null) {
            fieldName = member.Name;
            isJoined = true;
          }
          else {
            if (type.Fields[fieldName].IsPrimaryKey)
              fieldName = member.Name + "." + fieldName;
            else {
              var pathItem = new MappingPathItem(
                type,
                isJoined ? null : fieldName, 
                isJoined ? fieldName : null);
              result.AddHead(pathItem);
              fieldName = member.Name;
              isJoined = true;
            }
          }
        }
        else
          fieldName = fieldName==null ? member.Name : member.Name + "." + fieldName;
      }
      if (expression.NodeType == ExpressionType.Parameter) {
        var type = translator.Model.Types[expression.Type];
        if (fieldName != null) {
          var pathItem = new MappingPathItem(
            type,
            isJoined ? null : fieldName,
            isJoined ? fieldName : null);
          result.AddHead(pathItem);
        }
      }
      else
        return null;
      return result;
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (translator.Evaluator.CanBeEvaluated(e)) {
        if (translator.ParameterExtractor.IsParameter(e))
          return e;
        return translator.Evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      var fieldPath = Translate(m);
      if (fieldPath == null)
        return m;
      var method = m.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(m.Type);
      return Expression.Call(parameter, method, Expression.Constant(source.GetColumnIndex(fieldPath)));
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      bool isKey = typeof(Key).IsAssignableFrom(b.Left.Type);
      bool isEntity = typeof(IEntity).IsAssignableFrom(b.Left.Type);
      bool isStructure = typeof(Structure).IsAssignableFrom(b.Left.Type);
      bool leftIsParameter = translator.ParameterExtractor.IsParameter(b.Left);
      bool rightIsParameter = translator.ParameterExtractor.IsParameter(b.Right);
      var first = b.Left;
      var second = b.Right;
      if (isKey || isEntity || isStructure) {
        if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual) 
          throw new InvalidOperationException();
        if (isStructure)
          throw new NotImplementedException();
        if (!leftIsParameter && !rightIsParameter) {
          throw new NotImplementedException();
        }
        if (leftIsParameter) {
          first = b.Right;
          second = b.Left;
        }
        TypeInfo type;
        if (isKey) {
          var keyAccess = (MemberExpression) first;
          type = translator.Model.Types[keyAccess.Expression.Type];
        }
        else
          type = translator.Model.Types[first.Type];
        Expression result = null;
        var key = isKey ? second : Expression.MakeMemberAccess(second, identifierAccessor);
        var fieldStack = Translate(first);
        if (fieldStack == null)
          throw new InvalidOperationException();
        MappingPathItem pathItem = null;
        if (fieldStack.Count!=0)
          pathItem = fieldStack.ExtractTail();
        var keyColumns = type.Hierarchy.KeyFields.Select((kf, i) => new { FieldName = kf.Key.Name, ParameterIndex = i });
        foreach (var pair in keyColumns) {
          var fieldName = pathItem == null ? pair.FieldName : pathItem.JoinedFieldName + "." + pair.FieldName;
          var keyItem = new MappingPathItem(pathItem == null ? type : pathItem.Type, fieldName, null);
          fieldStack.AddTail(keyItem);
          var columnIndex = source.GetColumnIndex(fieldStack);
          fieldStack.ExtractTail();
          if (columnIndex < 0)
            throw new InvalidOperationException(string.Format("Field '{0}' is not found.", pair.FieldName));
          var method = genericAccessor.MakeGenericMethod(source.RecordSet.Header.TupleDescriptor[columnIndex]);
          Expression left = Expression.Call(parameter, method, Expression.Constant(columnIndex));
          Expression right = Expression.Call(Expression.MakeMemberAccess(key, keyValueAccessor), method, Expression.Constant(pair.ParameterIndex));
          if (result == null) {
            result = b.NodeType == ExpressionType.Equal ?
              Expression.Equal(left, right) :
              Expression.NotEqual(left, right);
          }
          else {
            result = b.NodeType == ExpressionType.Equal ?
              Expression.AndAlso(result, Expression.Equal(left, right)) :
              Expression.AndAlso(result, Expression.NotEqual(left, right));
          }
        }
        return result;
      }
      return base.VisitBinary(b);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FieldAccessTranslator(RseQueryTranslator translator)
    {
      this.translator = translator;
    }

    static FieldAccessTranslator()
    {
      keyValueAccessor = typeof (Key).GetProperty("Value");
      identifierAccessor = typeof(IIdentified<Key>).GetMember("Identifier")[0];
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
