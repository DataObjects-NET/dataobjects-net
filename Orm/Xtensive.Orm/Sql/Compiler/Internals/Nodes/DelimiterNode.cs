// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Compiler
{
  [DebuggerDisplay("Text = {Text}")]
  internal class DelimiterNode : Node
  {
    public readonly SqlDelimiterType Type;
    public readonly string Text;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public DelimiterNode(SqlDelimiterType type, string text)
    {
      Type = type;
      Text = text;
    }
  }
}