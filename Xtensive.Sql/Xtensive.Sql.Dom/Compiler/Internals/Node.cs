// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal abstract class Node
  {
    internal Node Next;

    internal abstract void AcceptVisitor(INodeVisitor visitor);

    [DebuggerStepThrough]
    protected Node()
    {
    }
  }
}