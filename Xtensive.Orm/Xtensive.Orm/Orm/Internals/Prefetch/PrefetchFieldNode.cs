// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Diagnostics;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class PrefetchFieldNode
  {
    public FieldInfo Field { get; private set; }

    public PrefetchFieldNode(FieldInfo field)
    {
      Field = field;
    }
  }
}