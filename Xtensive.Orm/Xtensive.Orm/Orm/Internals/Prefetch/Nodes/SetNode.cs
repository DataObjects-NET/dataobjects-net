// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Linq;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class SetNode : BaseFieldNode, IHasNestedNodes
  {
    private LambdaExpression keySelector;

    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; private set; }

    public TypeInfo ElementType { get; private set; }

    public IEnumerable<Key> ExtractKeys(object target)
    {
      if (target==null)
        return Enumerable.Empty<Key>();
      var entity = (Entity) target;
      var entitySetQuery = entity.GetFieldValue(Field) as IQueryable;
      if (entitySetQuery==null)
        return Enumerable.Empty<Key>();

      return SelectKeys(entitySetQuery);
    }

    private IEnumerable<Key> SelectKeys(IQueryable query)
    {
      var provider = query.Provider;
      var selectMethod = WellKnownMembers.Queryable.Select
        .MakeGenericMethod(ElementType.UnderlyingType, typeof (Key));
      // query.Select(keySelector)
      var selectCall = Expression.Call(
        selectMethod, query.Expression, Expression.Quote(GetKeySelector()));
      var resultQuery = provider.CreateQuery(selectCall);
      // Result query is IQueryable<Key> now
      // It's safe to upcast it to IEnumerable<Key>
      return (IEnumerable<Key>) resultQuery;
    }

    public IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<BaseFieldNode> nestedNodes)
    {
      return new SetNode(Path, Field, ElementType, NestedNodes);
    }

    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitSetNode(this);
    }

    private LambdaExpression GetKeySelector()
    {
      // item => item.Key
      if (keySelector!=null)
        return keySelector;
      var parameter = Expression.Parameter(ElementType.UnderlyingType, "item");
      var keyAccess = Expression.Property(parameter, "Key");
      keySelector = FastExpression.Lambda(keyAccess, parameter);
      return keySelector;
    }

    // Constructors


    public SetNode(string path, FieldInfo field, TypeInfo elementType, ReadOnlyCollection<BaseFieldNode> nestedNodes)
      : base(path, field)
    {
      ElementType = elementType;
      NestedNodes = nestedNodes;
    }
  }
}