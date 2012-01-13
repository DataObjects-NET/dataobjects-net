// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Container node in SQL DOM query model.
  /// </summary>
  public class ContainerNode : Node
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

    public void AppendPlaceholder(object id)
    {
      Add(new PlaceholderNode(id));
    }

    public void AppendCycleItem(int index)
    {
      Add(new CycleItemNode(index));
    }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      Add(new DelimiterNode(SqlDelimiterType.Row, text));
    }

    public void AppendDelimiter(string text, SqlDelimiterType type)
    {
      Add(new DelimiterNode(type, text));
    }


    // Constructors

    public ContainerNode(bool requireIndent)
    {
      RequireIndent = requireIndent;
    }

    public ContainerNode()
    {
      RequireIndent = false;
    }
  }
}