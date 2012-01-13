// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

namespace Xtensive.Sql.Compiler
{
  internal class VariantNode : Node
  {
    public readonly object Id;

    public Node Main;
    public Node Alternative;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public VariantNode(object id)
    {
      Id = id;
    }
  }
}