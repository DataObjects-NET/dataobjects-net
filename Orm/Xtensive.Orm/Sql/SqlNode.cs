// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  /// <summary>
  /// Represents any node in Sql expression tree.
  /// </summary>
  [Serializable]
  public abstract class SqlNode : ISqlNode
  {
    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    /// <value>The type of the node.</value>
    public SqlNodeType NodeType { get; internal set; }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    public virtual SqlNode Clone() => Clone(new SqlNodeCloneContext(false));

    object ICloneable.Clone() => Clone(new SqlNodeCloneContext(false));

    internal abstract SqlNode Clone(SqlNodeCloneContext context);

    internal SqlNode(SqlNodeType nodeType)
    {
      NodeType = nodeType;
    }

    public abstract void AcceptVisitor(ISqlVisitor visitor);
  }
}