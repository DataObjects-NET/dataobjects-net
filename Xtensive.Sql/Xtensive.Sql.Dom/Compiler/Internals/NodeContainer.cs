// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class NodeContainer : Node
  {
    private Node current;

    public Node Child;
    public bool RequireIndent;

    public Node Current { get { return current; } }
    public bool IsEmpty { get { return Child==null; } }

    public void Add(Node node)
    {
      if (Child==null)
        Child = node;
      else
        current.Next = node;
      current = node;
    }

    public override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    public NodeContainer(bool requireIndent)
    {
      RequireIndent = requireIndent;
    }

    public NodeContainer()
    {
      RequireIndent = false;
    }
  }
}