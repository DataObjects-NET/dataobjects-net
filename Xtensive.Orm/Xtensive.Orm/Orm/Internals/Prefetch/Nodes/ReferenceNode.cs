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
  internal class ReferenceNode : FieldNode
  {
    public TypeInfo ElementType { get; private set; }
    public IEnumerable<FieldNode> NestedNodes { get; private set; }

    public ReferenceNode(string path, TypeInfo elementType, FieldInfo field, IEnumerable<FieldNode> nestedNodes)
      : base(path, field)
    {
      ElementType = elementType;
      NestedNodes = nestedNodes;
    }
  }
}