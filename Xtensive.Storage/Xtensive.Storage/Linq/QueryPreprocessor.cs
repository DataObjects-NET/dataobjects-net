// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

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

namespace Xtensive.Storage.Linq
{
  // TODO: Split to a set of visitors, + use visitors specific to RSE
  public sealed class QueryPreprocessor : ExpressionVisitor
  {
    #region Nested helper classes

    // TODO: Extract to "Internals" namespace
    class MemberAccessChecker : ExpressionVisitor
    {
      private bool containsMemberAccess;

      public static bool ContainsMemberAccess(Expression expression)
      {
        var mac = new MemberAccessChecker();
        mac.Visit(expression);
        return mac.containsMemberAccess;
      }

      protected override Expression VisitMemberAccess(MemberExpression m)
      {
        containsMemberAccess = true;
        return base.VisitMemberAccess(m);
      }

      protected override Expression VisitUnknown(Expression e)
      {
        return e;
      }

      private MemberAccessChecker()
      {}
    }

    #endregion

    private readonly DomainModel model;
    private readonly HashSet<Expression> evaluationCandidates;

    public static Expression Translate(Expression expression, DomainModel model)
    {
      var candidates = EvaluationChecker.GetCandidates(expression);
      var queryProcessor = new QueryPreprocessor(model, candidates);
      expression = queryProcessor.Visit(expression);
      return expression;
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (evaluationCandidates.Contains(e)) {
        if (MemberAccessChecker.ContainsMemberAccess(e))
          return ExtractParameter(e);
        return Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;
        return new IndexAccessExpression(c.Type, index);
      }
      return base.VisitConstant(c);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object != null && mc.Object.Type == typeof(Tuple)) {
        // TODO: => Core.Wellknown
        if (mc.Method.Name == "GetValue" || mc.Method.Name == "GetValueOrDefault") {
          var columnArgument = mc.Arguments[0];
          if (evaluationCandidates.Contains(columnArgument)) {
            int columnIndex;
            if (columnArgument.NodeType==ExpressionType.Constant)
              columnIndex = (int) ((ConstantExpression) columnArgument).Value;
            else {
              var columnFunc = Expression.Lambda<Func<int>>(columnArgument).Compile();
              columnIndex = columnFunc();
            }
            return Expression.Call(mc.Object, mc.Method, Expression.Constant(columnIndex));
          }
        }
      }
      return base.VisitMethodCall(mc);
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
        return new FieldAccessExpression(m.Type, field);
      }
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      bool isKey = typeof(Key).IsAssignableFrom(b.Left.Type);
      bool isEntity = typeof(IEntity).IsAssignableFrom(b.Left.Type);
      if (isKey || isEntity) {
        if (b.NodeType!=ExpressionType.Equal && b.NodeType!=ExpressionType.NotEqual) 
          throw new InvalidOperationException();

        TypeInfo type;
        if (isKey) {
          var keyAccess = (MemberExpression) b.Left;
          type = model.Types[keyAccess.Expression.Type];
        }
        else
          type = model.Types[b.Left.Type];
        Expression result = null;
        Expression<Func<Key>> lambda = null;
        if (isKey)
          lambda = Expression.Lambda<Func<Key>>(b.Right);
        else {
          var identifier = typeof(IIdentified<Key>).GetMember("Identifier")[0];
          lambda = Expression.Lambda<Func<Key>>(Expression.MakeMemberAccess(b.Right, identifier));
        }
        Func<Key> getKey = lambda.Compile();
        foreach (var keyItem in type.Hierarchy.KeyFields.Select((p,i) => new {Field = type.Fields[p.Key.Name], Index = i})) {
          Expression left = new FieldAccessExpression(keyItem.Field.ValueType, keyItem.Field);
          var item = keyItem;
          Expression<Func<object>> binding = () => getKey().Value.GetValueOrDefault(item.Index);
          Expression right = new ParameterAccessExpression(keyItem.Field.ValueType, binding);
          if (result==null)
            result = b.NodeType==ExpressionType.Equal ? 
              Expression.Equal(left, right) : 
              Expression.NotEqual(left, right);
          else
            result = b.NodeType==ExpressionType.Equal ? 
              Expression.AndAlso(result, Expression.Equal(left, right)) : 
              Expression.AndAlso(result, Expression.NotEqual(left, right));
        }
        return result;
      }
      return base.VisitBinary(b);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    private static Expression ExtractParameter(Expression expression)
    {
      Type type = expression.Type;
      if (type.IsValueType)
        expression = Expression.Convert(expression, typeof(object));
      var lambda = Expression.Lambda<Func<object>>(expression);
      return new ParameterAccessExpression(type, lambda);
    }

    private static Expression Evaluate(Expression e)
    {
      if (e.NodeType == ExpressionType.Constant)
        return e;
      Type type = e.Type;
      if (type.IsValueType)
        e = Expression.Convert(e, typeof(object));
      var lambda = Expression.Lambda<Func<object>>(e);
      var func = lambda.Compile();
      return Expression.Constant(func(), type);
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    private QueryPreprocessor(DomainModel model, HashSet<Expression> evaluationCandidates)
    {
      this.model = model;
      this.evaluationCandidates = evaluationCandidates;
    }
  }
}