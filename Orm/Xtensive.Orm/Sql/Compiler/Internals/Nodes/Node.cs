// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Node in SQL DOM query model.
  /// </summary>
  public abstract class Node
  {
    public Node Next;

    internal abstract void AcceptVisitor(NodeVisitor visitor);
  }
}