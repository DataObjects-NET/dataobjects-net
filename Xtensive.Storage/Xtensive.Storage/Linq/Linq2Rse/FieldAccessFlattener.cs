// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using System.Linq;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public class FieldAccessFlattener : ExpressionVisitor
  {
    private readonly DomainModel model;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private ResultExpression result;

    public ResultExpression FlattenFieldAccess(ResultExpression source, LambdaExpression le)
    {
      result = source;
      Visit(le);
      return result;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (!evaluator.CanBeEvaluated(m)) {
        var typesStack = new Stack<TypeInfo>();
        var typesPath = new Stack<Pair<TypeInfo, string>>();
        string fieldName = null;
        string lastFieldName = null;
        Expression expression = m;
        if (typeof(Key).IsAssignableFrom(m.Type))
          expression = ((MemberExpression) expression).Expression;
        while (expression.NodeType==ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression) expression;
          var member = (PropertyInfo) memberAccess.Member;
          expression = memberAccess.Expression;
          if (fieldName == null)
            fieldName = member.Name;
          else
            fieldName = member.Name + "." + fieldName;
          if (expression.NodeType==ExpressionType.MemberAccess) {
            if (typeof (IEntity).IsAssignableFrom(expression.Type)) {
              var type = model.Types[expression.Type];
              var field = type.Fields[fieldName];
              if(!field.IsPrimaryKey)
                typesStack.Push(type);
              if (lastFieldName == null)
                lastFieldName = fieldName;
              else {
                typesPath.Push(new Pair<TypeInfo, string>(typesStack.Peek(),fieldName));
              }
              fieldName = null;
            }
          }
        }
        if (typesStack.Count > 0)
          typesPath.Push(new Pair<TypeInfo, string>(typesStack.Peek(), fieldName));
        List<Pair<TypeInfo, string>> list = typesPath.ToList();
        foreach (var pair in list) {
          throw new NotImplementedException();
        }
      }
      return m;
    }


    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FieldAccessFlattener(DomainModel model, Expression query)
    {
      this.model = model;
      evaluator = new ExpressionEvaluator(query);
      parameterExtractor = new ParameterExtractor(evaluator);
    }
  }
}