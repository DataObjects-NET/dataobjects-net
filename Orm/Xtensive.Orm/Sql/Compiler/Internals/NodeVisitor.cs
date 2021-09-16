// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;
using System.Collections.Generic;

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

    protected void VisitNodes(IEnumerable<Node> nodes)
    {
      foreach (var node in nodes) {
        node.AcceptVisitor(this);
      }
    }
  }
}