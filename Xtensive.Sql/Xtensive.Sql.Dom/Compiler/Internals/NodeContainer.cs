// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class NodeContainer : Node
  {
    private Node current;

    public Node Child;
    public bool RequireIndent;

    public bool IsEmpty { get { return Child==null; } }

    public void Add(Node node)
    {
      if (Child==null)
        Child = node;
      else
        current.Next = node;
      current = node;
    }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      AppendDelimiter(text, DelimiterType.Row);
    }

    public void AppendDelimiter(string text, DelimiterType type)
    {
      Add(new NodeDelimiter(type, text));
    }

    public override void AcceptVisitor(INodeVisitor visitor)
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