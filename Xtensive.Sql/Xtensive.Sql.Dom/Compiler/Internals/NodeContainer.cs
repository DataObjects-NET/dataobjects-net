// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class NodeContainer : Node
  {
    internal Node Child;
    private Node current;
    internal bool RequireIndent;

    internal void Add(Node node)
    {
      if (Child==null)
        Child = node;
      else
        current.Next = node;
      current = node;
    }

    internal bool IsEmpty
    {
      get { return Child==null; }
    }

    internal void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      Add(new TextNode(text));
    }

    internal void AppendDelimiter(string text)
    {
      AppendDelimiter(text, DelimiterType.Row);
    }

    internal void AppendDelimiter(string text, DelimiterType type)
    {
      Add(new NodeDelimiter(type, text));
    }

    internal override void AcceptVisitor(INodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal NodeContainer(bool requireIndent)
    {
      RequireIndent = requireIndent;
    }

    internal NodeContainer()
    {
      RequireIndent = false;
    }
  }
}