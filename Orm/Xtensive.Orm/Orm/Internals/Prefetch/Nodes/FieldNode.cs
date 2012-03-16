// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.24

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class FieldNode : BaseFieldNode
  {
    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitFieldNode(this);
    }

    public FieldNode(string path, FieldInfo field)
      : base(path, field)
    {
    }
  }
}