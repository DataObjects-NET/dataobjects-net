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
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class FieldAccessTranslator : ExpressionVisitor
  {
    private readonly DomainModel model;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private ResultExpression source;
    private ParameterExpression parameter;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo genericAccessor;
    private static readonly PropertyInfo keyValueAccessor;
    private static readonly MemberInfo identifierAccessor;


    public LambdaExpression Translate(ResultExpression source, LambdaExpression le)
    {
      this.source = source;
      parameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(le.Body);
      var lambda = Expression.Lambda(typeof(Func<Tuple, bool>), body, parameter);
      return lambda;
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (evaluator.CanBeEvaluated(e)) {
        if (parameterExtractor.IsParameter(e))
          return e;
        return evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      var memberNames = new Stack<string>();
      Expression e = m;
      while(e.NodeType == ExpressionType.MemberAccess) {
        var memberAccess = (MemberExpression)e;
        var member = (PropertyInfo)memberAccess.Member;
        TypeInfo type;
        if (model.Types.TryGetValue(member.PropertyType, out type) && !type.IsStructure) {
          memberNames.Push(memberAccess.Member.Name);
        }
        else {
          if (memberNames.Count > 0) {
            var name = memberNames.Pop();
            name = string.Format("{0}.{1}", memberAccess.Member.Name, name);
            memberNames.Push(name);
          }
          else
            memberNames.Push(memberAccess.Member.Name);
        }
        e = memberAccess.Expression;
      }
      if (e.NodeType == ExpressionType.Parameter) {
        var name = string.Join(".", memberNames.ToArray());
//        var type = model.Types[e.Type];
//        var fields = type.Fields;
//        FieldInfo field = null;
//        while(memberNames.Count > 0) {
//          var name = memberNames.Pop();
//          field = fields[name];
//          fields = field.Fields;
//        }
        var method = m.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(m.Type);
        return Expression.Call(parameter, method, Expression.Constant(source.GetColumnIndex(name)));
      }
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      bool isKey = typeof(Key).IsAssignableFrom(b.Left.Type);
      bool isEntity = typeof(IEntity).IsAssignableFrom(b.Left.Type);
      bool isStructure = typeof(Structure).IsAssignableFrom(b.Left.Type);
      bool leftIsParameter = parameterExtractor.IsParameter(b.Left);
      bool rightIsParameter = parameterExtractor.IsParameter(b.Right);
      var first = b.Left;
      var second = b.Right;
      if (isKey || isEntity || isStructure) {
        if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual) 
          throw new InvalidOperationException();
        if (isStructure)
          throw new NotImplementedException();
        if (!leftIsParameter && !rightIsParameter) {
          
        }
        if (leftIsParameter) {
          first = b.Right;
          second = b.Left;
        }
        TypeInfo type;
        if (isKey) {
          var keyAccess = (MemberExpression) first;
          type = model.Types[keyAccess.Expression.Type];
        }
        else
          type = model.Types[first.Type];
        Expression result = null;
        var key = isKey ? second : Expression.MakeMemberAccess(second, identifierAccessor);
        string fieldName = null;
        first = isKey ? ((MemberExpression) first).Expression : first;
        while (first.NodeType == ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression)first;
          var memberName = memberAccess.Member.Name;
          if (fieldName == null)
            fieldName = memberName;
          else
            fieldName = string.Format("{0}.{1}", memberName, fieldName);
          first = memberAccess.Expression;
        }

        var keyColumns = type.Hierarchy.KeyFields.Select((kf, i) => new { FieldName = fieldName == null ? kf.Key.Name :  string.Format("{0}.{1}", fieldName, kf.Key.Name), ParameterIndex = i });
        foreach (var pair in keyColumns) {
          var columnIndex = source.GetColumnIndex(pair.FieldName);
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
    public FieldAccessTranslator(DomainModel model, Expression query)
    {
      this.model = model;
      evaluator = new ExpressionEvaluator(query);
      parameterExtractor = new ParameterExtractor(evaluator);
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
