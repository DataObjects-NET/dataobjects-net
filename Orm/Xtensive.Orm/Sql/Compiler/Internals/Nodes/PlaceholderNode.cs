// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

namespace Xtensive.Sql.Compiler
{
  internal class PlaceholderNode : Node
  {
    public readonly object Id;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public PlaceholderNode(object id)
    {
      Id = id;
    }
  }
}