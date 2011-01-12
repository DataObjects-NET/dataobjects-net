// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class PrefetchReferenceNode : PrefetchFieldNode
  {
    public TypeInfo ElementType { get; private set; }
    public IEnumerable<PrefetchFieldNode> NestedNodes { get; private set; }

    public PrefetchReferenceNode(TypeInfo elementType, FieldInfo field, IEnumerable<PrefetchFieldNode> nestedNodes)
      : base(field)
    {
      ElementType = elementType;
      NestedNodes = nestedNodes;
    }
  }
}