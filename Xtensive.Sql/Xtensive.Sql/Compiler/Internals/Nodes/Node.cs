// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Compiler
{
  public abstract class Node
  {
    public Node Next;

    internal abstract void AcceptVisitor(NodeVisitor visitor);
  }
}