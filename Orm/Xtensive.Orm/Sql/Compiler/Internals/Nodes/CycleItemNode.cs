// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.07

namespace Xtensive.Sql.Compiler
{
  internal class CycleItemNode : Node
  {
    public readonly int Index;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    public CycleItemNode(int index)
    {
      Index = index;
    }
  }
}