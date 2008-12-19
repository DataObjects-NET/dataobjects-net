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
  public class FieldAccessTranslator : ExpressionVisitor
  {
    private readonly DomainModel model;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private ResultExpression result;
    private ParameterExpression parameter;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo genericAccessor;


    public ResultExpression FlattenFieldAccess(ResultExpression source, LambdaExpression le)
    {
      return source;
    }

    public LambdaExpression Translate(ResultExpression source, LambdaExpression le)
    {
      result = source;
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
          return parameterExtractor.ExtractParameter(e);
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
        var type = model.Types[e.Type];
        var fields = type.Fields;
        FieldInfo field = null;
        while(memberNames.Count > 0) {
          var name = memberNames.Pop();
          if (name == "Key")
            return m;
          field = fields[name];
          fields = field.Fields;
        }
        var method = m.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(m.Type);
        return Expression.Call(parameter, method, Expression.Constant(field.MappingInfo.Offset));
      }
      return base.VisitMemberAccess(m);
    }

//    protected override Expression VisitBinary(BinaryExpression b)
//    {
//      bool isKey = typeof(Key).IsAssignableFrom(b.Left.Type);
//      bool isEntity = typeof(IEntity).IsAssignableFrom(b.Left.Type);
//      if (isKey || isEntity) {
//        if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual) 
//          throw new InvalidOperationException();
//
//        TypeInfo type;
//        if (isKey) {
//          var keyAccess = (MemberExpression) b.Left;
//          type = model.Types[keyAccess.Expression.Type];
//        }
//        else
//          type = model.Types[b.Left.Type];
//        Expression result = null;
//        Expression<Func<Key>> lambda = null;
//        if (isKey)
//          lambda = Expression.Lambda<Func<Key>>(b.Right);
//        else {
//          var identifier = typeof(IIdentified<Key>).GetMember("Identifier")[0];
//          lambda = Expression.Lambda<Func<Key>>(Expression.MakeMemberAccess(b.Right, identifier));
//        }
//        Func<Key> getKey = lambda.Compile();
//        foreach (var keyItem in type.Hierarchy.KeyFields.Select((p,i) => new {Field = type.Fields[p.Key.Name], Index = i})) {
//          Expression left = new FieldAccessExpression(keyItem.Field.ValueType, keyItem.Field);
//          var item = keyItem;
//          Expression<Func<object>> binding = () => getKey().Value.GetValueOrDefault(item.Index);
//          Expression right = new ParameterAccessExpression(keyItem.Field.ValueType, binding);
//          if (result==null)
//            result = b.NodeType==ExpressionType.Equal ? 
//              Expression.Equal(left, right) : 
//              Expression.NotEqual(left, right);
//          else
//            result = b.NodeType==ExpressionType.Equal ? 
//              Expression.AndAlso(result, Expression.Equal(left, right)) : 
//              Expression.AndAlso(result, Expression.NotEqual(left, right));
//        }
//        return result;
//      }
//      return base.VisitBinary(b);
//    }



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