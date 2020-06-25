// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class SetNode : BaseFieldNode, IHasNestedNodes
  {
    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; private set; }

    public TypeInfo ElementType { get; private set; }

    public IReadOnlyCollection<Key> ExtractKeys(object target)
    {
      if (target==null) {
        return Array.Empty<Key>();
      }

      var entity = (Entity) target;
      var entitySet = (EntitySetBase) entity.GetFieldValue(Field);
      var fetchedKeys = entitySet.State.FetchedKeys;
      return fetchedKeys.ToArray(fetchedKeys.Count);
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