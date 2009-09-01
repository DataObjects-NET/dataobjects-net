// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Compiler.Internals
{
  public abstract class Node
  {
    public Node Next;

    internal abstract void AcceptVisitor(NodeVisitor visitor);
  }
}