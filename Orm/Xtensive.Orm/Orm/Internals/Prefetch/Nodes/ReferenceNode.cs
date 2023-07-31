// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class ReferenceNode : BaseFieldNode, IHasNestedNodes
  {
    public TypeInfo ReferenceType { get; private set; }

    public IReadOnlyList<BaseFieldNode> NestedNodes { get; private set; }

    public IReadOnlyCollection<Key> ExtractKeys(object target)
    {
      if (target == null) {
        return Array.Empty<Key>();
      }

      var entity = (Entity) target;
      var referenceKey = entity.GetReferenceKey(Field);
      return referenceKey == null
        ? Array.Empty<Key>()
        : new[] {referenceKey};
    }

    public IHasNestedNodes ReplaceNestedNodes(IReadOnlyList<BaseFieldNode> nestedNodes) =>
      new ReferenceNode(Path, Field, ReferenceType, NestedNodes);

    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitReferenceNode(this);
    }

    // Constructors

    public ReferenceNode(string path, FieldInfo field, TypeInfo referenceType, IReadOnlyList<BaseFieldNode> nestedNodes)
      : base(path, field)
    {
      ReferenceType = referenceType;
      NestedNodes = nestedNodes;
    }
  }
}