// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class VariantNode : Node
  {
    public readonly object Key;

    public Node Main;
    public Node Alternative;
    
    public override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public VariantNode(object key)
    {
      Key = key;
    }
  }
}