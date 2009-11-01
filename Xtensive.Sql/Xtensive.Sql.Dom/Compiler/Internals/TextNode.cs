// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;
using Xtensive.Sql.Dom.Compiler.Internals;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  [DebuggerDisplay("Text = {Text}")]
  internal class TextNode : Node
  {
    internal string Text;

    internal override void AcceptVisitor(INodeVisitor visitor)
    {
      visitor.Visit((TextNode)this);
    }

    [DebuggerStepThrough]
    internal TextNode(string text)
    {
      Text = text;
    }
  }
}