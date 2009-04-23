// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  [DebuggerDisplay("Text = {Text}")]
  internal class TextNode : Node
  {
    public readonly string Text;

    public override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public TextNode(string text)
    {
      Text = text;
    }
  }
}