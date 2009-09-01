// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Compiler.Internals
{
  [DebuggerDisplay("Text = {Text}")]
  internal class NodeDelimiter : Node
  {
    public readonly SqlDelimiterType Type;
    public readonly string Text;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public NodeDelimiter(SqlDelimiterType type, string text)
    {
      Type = type;
      Text = text;
    }
  }
}