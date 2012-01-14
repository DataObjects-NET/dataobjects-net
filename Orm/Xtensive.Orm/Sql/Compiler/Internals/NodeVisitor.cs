// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;

namespace Xtensive.Sql.Compiler
{
  internal class NodeVisitor
  {
    public virtual void Visit(TextNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(ContainerNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(DelimiterNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(VariantNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(PlaceholderNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(CycleNode node)
    {
      throw new InvalidOperationException();
    }

    public virtual void Visit(CycleItemNode node)
    {
      throw new InvalidOperationException();
    }

    protected void VisitNodeSequence(Node node)
    {
      while (node != null) {
        node.AcceptVisitor(this);
        node = node.Next;
      }
    }
  }
}