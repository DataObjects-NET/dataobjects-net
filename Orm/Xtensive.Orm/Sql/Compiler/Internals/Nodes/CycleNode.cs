// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.06

namespace Xtensive.Sql.Compiler
{
  internal class CycleNode : Node
  {
    public readonly object Id;

    public Node Body;
    public Node EmptyCase;

    public string Delimiter;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public CycleNode(object id)
    {
      Id = id;
    }
  }
}