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
  internal sealed class ReferenceNode : BaseFieldNode, IHasNestedNodes
  {
    public TypeInfo ReferenceType { get; private set; }

    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; private set; }

    public IEnumerable<Key> ExtractKeys(object target)
    {
      if (target==null)
        return Enumerable.Empty<Key>();
      var entity = (Entity) target;
      var referenceKey = entity.GetReferenceKey(Field);
      return referenceKey==null
        ? Enumerable.Empty<Key>()
        : Enumerable.Repeat(referenceKey, 1);
    }

    public IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<BaseFieldNode> nestedNodes)
    {
      return new ReferenceNode(Path, Field, ReferenceType, NestedNodes);
    }

    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitReferenceNode(this);
    }

    // Constructors

    public ReferenceNode(string path, FieldInfo field, TypeInfo referenceType, ReadOnlyCollection<BaseFieldNode> nestedNodes)
      : base(path, field)
    {
      ReferenceType = referenceType;
      NestedNodes = nestedNodes;
    }
  }
}