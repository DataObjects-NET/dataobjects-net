// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Linq;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Core;
using Xtensive.Orm.Resources;
using Xtensive.Parameters;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class NodeParser : ExpressionVisitor
  {
    private readonly DomainModel model;
    private readonly ParameterExpression parameter;
    private Expression extractorExpression;
    private int? top;
    private readonly List<FieldNode> nodes;

    public static KeyExtractorNode<T> Parse<T, TValue>(DomainModel model, Expression<Func<T, TValue>> expression)
    {
      var parameter = expression.Parameters.Single();
      if (expression.Body==parameter)
        return null;
      var parser = new NodeParser(model, parameter);
      parser.Visit(expression.Body);
      if (parser.extractorExpression==null && parser.nodes.Count==0)
        return null;
      var extractor = parser.extractorExpression ?? parameter;
      var path = GetMemberPath(extractor);
      var keyExtractor = BuildKeyExtractor<T>(extractor, parameter);
      return new KeyExtractorNode<T>(path, keyExtractor, new ReadOnlyCollection<FieldNode>(parser.nodes));
    }

    private static string GetMemberPath(Expression extractor)
    {
      var pathStack = new Stack<string>();
      var memberSequence = extractor;
      while (memberSequence.NodeType==ExpressionType.MemberAccess) {
        var memberExpression = (MemberExpression) memberSequence;
        pathStack.Push(memberExpression.Member.Name);
        memberSequence = memberExpression.Expression;
      }
      var path = pathStack.ToDelimitedString(".");
      if (path.IsNullOrEmpty())
        path = "*";
      return path;
    }

    private static Expression<Func<T, IEnumerable<Key>>> BuildKeyExtractor<T>(
      Expression keyExtractorExpression, ParameterExpression parameter)
    {
      if (typeof (IEntity).IsAssignableFrom(keyExtractorExpression.Type)) {
        Expression<Func<IEntity, IEnumerable<Key>>> extractor = e => Enumerable.Repeat(e.Key, 1);
        var body = extractor.BindParameters(keyExtractorExpression);
        return Expression.Lambda<Func<T, IEnumerable<Key>>>(body, parameter);
      }
      if (typeof (IEnumerable).IsAssignableFrom(keyExtractorExpression.Type)) {
        Expression<Func<IEnumerable, IEnumerable<Key>>> extractor
          = enumerable => enumerable.OfType<IEntity>().Select(e => e.Key);
        var body = extractor.BindParameters(keyExtractorExpression);
        return Expression.Lambda<Func<T, IEnumerable<Key>>>(body, parameter);
      }
      return _ => Enumerable.Empty<Key>();
    }

    private static IList<FieldNode> Parse(DomainModel model, LambdaExpression expression)
    {
      var parameter = expression.Parameters.Single();
      if (expression.Body==parameter)
        return null;
      var parser = new NodeParser(model, parameter);
      parser.Visit(expression.Body);
      return parser.nodes;
    }

    protected override Expression Visit(Expression e)
    {
      e = e.StripCasts();

      var isSupported = e.NodeType.In(
        ExpressionType.MemberAccess, ExpressionType.Call, ExpressionType.New,
        ExpressionType.Lambda, ExpressionType.Parameter);

      if (!isSupported)
        throw new NotSupportedException(string.Format(
          Strings.ExOnlyPropertAccessPrefetchOrAnonymousTypeSupportedButFoundX, e));

      return base.Visit(e);
    }

    protected override Expression VisitMemberAccess(MemberExpression e)
    {
      if (e.Expression==null)
        return e;

      if (typeof (IEntity).IsAssignableFrom(e.Expression.Type)) {
        var entityType = model.Types[e.Expression.Type];
        var path = e.Member.Name;
        var field = entityType.Fields[path];
        var nestedNodes = new ReadOnlyCollection<FieldNode>(nodes.ToList());
        nodes.Clear();
        if (typeof (IEntity).IsAssignableFrom(e.Type))
          nodes.Add(new ReferenceNode(path, model.Types[field.ValueType], field, nestedNodes));
        else if (typeof (EntitySetBase).IsAssignableFrom(e.Type))
          nodes.Add(new SetNode(path, model.Types[field.ItemType], field, top, nestedNodes));
        else
          nodes.Add(new FieldNode(path, field));
        var visited = (MemberExpression) base.VisitMemberAccess(e);
        return visited;
      }
      if (extractorExpression==null)
        extractorExpression = e;
      return e;
    }

    protected override Expression VisitNew(NewExpression n)
    {
      foreach (var argument in n.Arguments) {
        var lambda = Expression.Lambda(argument, parameter);
        var fieldNodes = Parse(model, lambda);
        nodes.AddRange(fieldNodes);
      }
      return n;
    }

    protected override Expression VisitParameter(ParameterExpression e)
    {
      if (extractorExpression==null)
        extractorExpression = e;
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression e)
    {
      var isPrefetchMethod = e.Method.DeclaringType==typeof (PrefetchExtensions)
        && e.Method.Name=="Prefetch"
        && e.Method.GetParameters().Length==2;

      if (!isPrefetchMethod) {
        throw new NotSupportedException(
          string.Format(Strings.ExOnlyPrefetchMethodSupportedButFoundX, e.ToString(true)));
      }

      var source = e.Arguments[0];
      var lambda = e.Arguments[1].StripQuotes();
      var fieldNodes = Parse(model, lambda);
      nodes.AddRange(fieldNodes);
      Visit(source);
      return e;
    }

    private NodeParser(DomainModel model, ParameterExpression parameter)
    {
      this.model = model;
      this.parameter = parameter;
      nodes = new List<FieldNode>();
    }
  }
}