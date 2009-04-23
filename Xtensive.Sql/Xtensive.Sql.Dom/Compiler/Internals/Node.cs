// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal abstract class Node
  {
    public Node Next;

    public abstract void AcceptVisitor(NodeVisitor visitor);
  }
}