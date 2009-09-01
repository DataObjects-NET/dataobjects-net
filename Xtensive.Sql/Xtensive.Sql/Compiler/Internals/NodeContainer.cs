// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Compiler.Internals
{
  public class NodeContainer : Node
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

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public void AppendHole(string prefix, object key)
    {
      Add(new HoleNode(prefix, key));
    }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      Add(new NodeDelimiter(SqlDelimiterType.Row, text));
    }

    public void AppendDelimiter(string text, SqlDelimiterType type)
    {
      Add(new NodeDelimiter(type, text));
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