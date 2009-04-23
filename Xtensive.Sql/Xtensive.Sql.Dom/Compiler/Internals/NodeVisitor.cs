// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class NodeVisitor
  {
    public virtual void Visit(TextNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(NodeContainer node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(NodeDelimiter node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(VariantNode node)
    {
      throw new InvalidOperationException();
    }

    protected virtual void VisitNodeSequence(Node node)
    {
      while (node != null) {
        node.AcceptVisitor(this);
        node = node.Next;
      }
    }
  }
}