// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using ObjectModel = System.Collections.ObjectModel;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class NodeBuilder
  {
    private readonly DomainModel model;
    private readonly Expression root;
    private readonly ExpressionMap map;

    public static KeyExtractorNode<T> Build<T, TValue>(DomainModel model, Expression<Func<T, TValue>> expression)
    {
      if (!typeof (IEntity).IsAssignableFrom(typeof (T)))
        return null;
      var parameter = expression.Parameters.First();
      if (expression.Body==parameter)
        return null;
      var map = ExpressionMapBuilder.Build(expression);
      var builder = new NodeBuilder(model, map, parameter);
      var nestedNodes = WrapNodes(builder.VisitRoot());
      return new KeyExtractorNode<T>(GetExtractor<T>(), nestedNodes);
    }

    private static Func<T, IEnumerable<Key>> GetExtractor<T>()
    {
      return target => EnumerableUtils.One(((IEntity) target).Key);
    }

    private IEnumerable<BaseFieldNode> VisitRoot()
    {
      return Visit(root);
    }

    private IEnumerable<BaseFieldNode> Visit(Expression expression)
    {
      switch (expression.NodeType) {
        case ExpressionType.Parameter:
          return VisitParameter((ParameterExpression) expression);
        case ExpressionType.MemberAccess:
          return VisitMemberAccess((MemberExpression) expression);
        case ExpressionType.Lambda:
          return VisitLambda((LambdaExpression) expression);
        case ExpressionType.New:
          return VisitNew((NewExpression) expression);
        default:
          throw Exceptions.InternalError("NodeBuilder failed to process expression", OrmLog.Instance);
      }
    }

    private IEnumerable<BaseFieldNode> VisitLambda(LambdaExpression expression)
    {
      var nestedParameter = expression.Parameters.First();
      return CreateNestedBuilder(nestedParameter).VisitRoot();
    }

    private IEnumerable<BaseFieldNode> VisitParameter(ParameterExpression expression)
    {
      return VisitChildren(expression);
    }

    private IEnumerable<BaseFieldNode> VisitNew(NewExpression expression)
    {
      return VisitChildren(expression);
    }

    private IEnumerable<BaseFieldNode> VisitMemberAccess(MemberExpression access)
    {
      var currentType = access.Expression.Type;

      if (!typeof (IEntity).IsAssignableFrom(currentType))
        throw new NotSupportedException("Only persistent properties are supported");

      var currentEntity = model.Types[currentType];
      var path = access.Member.Name;
      var field = currentEntity.Fields[path];

      if (field.IsStructure) {
        // Expand structure access sequence
        do {
          access = map.GetChildren(access)
            .OfType<MemberExpression>()
            .FirstOrDefault();
          if (access==null)
            break; // Prefetch of a structure it-self
          path += "." + access.Member.Name;
          field = currentEntity.Fields[path];
        } while (field.IsStructure);
      }

      BaseFieldNode result;

      if (field.IsEntitySet) {
        var elementType = model.Types[field.ItemType];
        var nestedNodes = WrapNodes(VisitChildren(access));
        result = new SetNode(path, field, elementType, nestedNodes);
      }
      else if (field.IsEntity) {
        var referenceType = model.Types[field.ValueType];
        var nestedNodes = WrapNodes(VisitChildren(access));
        result = new ReferenceNode(path, field, referenceType, nestedNodes);
      }
      else {
        result = new FieldNode(path, field);
      }

      return EnumerableUtils.One(result);
    }

    private static ObjectModel.ReadOnlyCollection<BaseFieldNode> WrapNodes(IEnumerable<BaseFieldNode> nodes)
    {
      return nodes.ToList().AsReadOnly();
    }

    private IEnumerable<BaseFieldNode> VisitChildren(Expression expression)
    {
      var result = new List<BaseFieldNode>();
      foreach (var child in map.GetChildren(expression))
        result.AddRange(Visit(child));
      return result;
    }

    private NodeBuilder CreateNestedBuilder(ParameterExpression newParameter)
    {
      return new NodeBuilder(model, map, newParameter);
    }

    // Constructors

    private NodeBuilder(DomainModel model, ExpressionMap map, Expression root)
    {
      this.model = model;
      this.root = root;
      this.map = map;
    }
  }
}