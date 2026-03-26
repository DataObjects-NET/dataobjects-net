// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Compiler
{
  [DebuggerDisplay("Text = {Text}")]
  internal class TextNode : Node
  {
    private const string CommaString = ", ";
    private static readonly TextNode CommaNode = new TextNode(CommaString);

    public readonly string Text;

    public static TextNode Create(string text)
    {
      if (text.Length < 3) {
        text = string.Intern(text);
        if (text == CommaString) {
          return CommaNode;
        }
      }
      return new TextNode(text);
    }

    internal override void AcceptVisitor(NodeVisitor visitor) => visitor.Visit(this);

    // Constructor

    public TextNode(string text)
    {
      Text = text;
    }
  }
}
