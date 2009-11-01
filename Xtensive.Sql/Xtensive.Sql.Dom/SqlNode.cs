// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom
{
  /// <summary>
  /// Represents any node in Sql expression tree.
  /// </summary>
  [Serializable]
  public abstract class SqlNode: ISqlNode
  {
    private SqlNodeType nodeType;

    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    /// <value>The type of the node.</value>
    public SqlNodeType NodeType
    {
      get { return nodeType; }
      internal set { nodeType = value; }
    }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    public object Clone()
    {
      SqlNodeCloneContext context = new SqlNodeCloneContext();
      return Clone(context);
    }

    internal abstract object Clone(SqlNodeCloneContext context);

    internal SqlNode(SqlNodeType nodeType)
    {
      this.nodeType = nodeType;
    }

    public abstract void AcceptVisitor(ISqlVisitor visitor);
  }
}