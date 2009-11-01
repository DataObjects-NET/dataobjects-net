// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  [DebuggerDisplay("Text = {Text}")]
  internal class NodeDelimiter : Node
  {
    internal readonly DelimiterType Type;
    internal readonly string Text;

    internal override void AcceptVisitor(INodeVisitor visitor)
    {
      visitor.Visit((NodeDelimiter)this);
    }

    internal NodeDelimiter(DelimiterType type, string text)
    {
      Type = type;
      Text = text;
    }
  }
}