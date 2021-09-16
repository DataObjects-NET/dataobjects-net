// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xtensive.Sql.Compiler
{
  public interface IOutput
  {
    StringBuilder StringBuilder { get; }
    IOutput Append(string text);
    IOutput Append(char v);
    IOutput Append(long v);
  }

  /// <summary>
  /// Container node in SQL DOM query model.
  /// </summary>
  public class ContainerNode : Node, IOutput, IEnumerable<Node>
  {
    private static readonly IFormatProvider invarianCulture = CultureInfo.InvariantCulture;

    private readonly StringBuilder stringBuilder = new StringBuilder();
    private readonly List<Node> children = new List<Node>();

    public IReadOnlyList<Node> Children
    {
      get {
        FlushBuffer();
        return children;
      }
    }

    public IEnumerator<Node> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool RequireIndent;

    public Node Current => Children.Last();
    public bool IsEmpty => !Children.Any();

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public void AppendCycleItem(int index)
    {
      Add(new CycleItemNode(index));
    }

    public void FlushBuffer()
    {
      if (stringBuilder.Length > 0) {
        children.Add(new TextNode(stringBuilder.ToString()));
        stringBuilder.Clear();
      }
    }

    public void Add(Node node)
    {
      FlushBuffer();
      children.Add(node);
    }

    internal void AppendPlaceholder(PlaceholderNode placeholder) =>
      Add(placeholder);

    public void AppendPlaceholderWithId(object id) =>
      AppendPlaceholder(new PlaceholderNode(id));

    public StringBuilder StringBuilder => stringBuilder;

    public IOutput Append(string text)
    {
      if (!string.IsNullOrEmpty(text)) {
        stringBuilder.Append(text);
      }
      return this;
    }

    public IOutput Append(char v)
    {
      stringBuilder.Append(v);
      return this;
    }

    public IOutput Append(long v)
    {
      stringBuilder.AppendFormat(invarianCulture, "{0}", v);
      return this;
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