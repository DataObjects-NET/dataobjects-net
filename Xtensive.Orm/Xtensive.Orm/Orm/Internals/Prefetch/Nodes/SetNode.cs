// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class SetNode : BaseFieldNode, IHasNestedNodes
  {
    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; private set; }

    public TypeInfo ElementType { get; private set; }

    public IEnumerable<Key> ExtractKeys(object target)
    {
      if (target==null)
        return Enumerable.Empty<Key>();
      var entity = (Entity) target;
      var entitySet = (EntitySetBase) entity.GetFieldValue(Field);
      return entitySet==null
        ? Enumerable.Empty<Key>()
        : entitySet.Entities.Select(e => e.Key).ToList();
    }

    public IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<BaseFieldNode> nestedNodes)
    {
      return new SetNode(Path, Field, ElementType, NestedNodes);
    }

    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitSetNode(this);
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