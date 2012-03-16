// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal abstract class BaseFieldNode : Node
  {
    public FieldInfo Field { get; private set; }

    // Constructors

    protected BaseFieldNode(string path, FieldInfo field)
      : base(path)
    {
      Field = field;
    }
  }
}