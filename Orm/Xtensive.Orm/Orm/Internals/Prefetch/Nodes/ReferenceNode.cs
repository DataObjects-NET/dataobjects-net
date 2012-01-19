// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class ReferenceNode : FieldNode, IHasNestedNodes
  {
    public TypeInfo ElementType { get; private set; }
    public ReadOnlyCollection<FieldNode> NestedNodes { get; private set; }

    public virtual IEnumerable<Key> ExtractKeys(object target)
    {
      if (target == null)
        return Enumerable.Empty<Key>();
      var entity = (Entity)target;
      var referenceKey = entity.GetReferenceKey(Field);
      return referenceKey == null 
        ? Enumerable.Empty<Key>() 
        : Enumerable.Repeat(referenceKey, 1);
    }

    public virtual IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<FieldNode> nestedNodes)
    {
      return new ReferenceNode(this, nestedNodes);
    }

    protected internal override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitReferenceNode(this);
    }

    public ReferenceNode(ReferenceNode source, ReadOnlyCollection<FieldNode> nestedNodes)
      : base(source.Path, source.Field)
    {
      ElementType = source.ElementType;
      NestedNodes = nestedNodes;
    }

    public ReferenceNode(string path, TypeInfo elementType, FieldInfo field, ReadOnlyCollection<FieldNode> nestedNodes)
      : base(path, field)
    {
      ElementType = elementType;
      NestedNodes = nestedNodes;
    }
  }
}