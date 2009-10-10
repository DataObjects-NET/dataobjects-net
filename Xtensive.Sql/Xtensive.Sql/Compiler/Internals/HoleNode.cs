// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

namespace Xtensive.Sql.Compiler.Internals
{
  internal class HoleNode : Node
  {
    public readonly object Id;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public HoleNode(object id)
    {
      Id = id;
    }
  }
}