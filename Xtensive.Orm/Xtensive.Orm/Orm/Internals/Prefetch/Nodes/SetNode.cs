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
  internal class SetNode : ReferenceNode
  {
    /// <summary>
    /// Gets count of elements to be prefetched 0 means it should prefetch only Count. 
    /// <see langword="null" /> means it should prefetch all elements.
    /// </summary>
    /// <value>The top.</value>
    public int? Top { get; private set; }

    public SetNode(string path, TypeInfo elementType, FieldInfo field, int? top, IEnumerable<FieldNode> nestedNodes)
      : base(path, elementType, field, nestedNodes)
    {
      Top = top;
    }
  }
}