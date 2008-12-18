// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
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


    public ResultExpression FlattenFieldAccess(ResultExpression source, LambdaExpression le)
    {
      return source;
    }

    public LambdaExpression Translate(ResultExpression source, LambdaExpression le)
    {
      result = source;
      var lambda = (LambdaExpression)VisitLambda(le);
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
        return new FieldAccessExpression(m.Type, field);
      }
      return base.VisitMemberAccess(m);
    }



    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FieldAccessTranslator(DomainModel model, Expression query)
    {
      this.model = model;
      evaluator = new ExpressionEvaluator(query);
      parameterExtractor = new ParameterExtractor(evaluator);
    }
  }
}