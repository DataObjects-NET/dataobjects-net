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

    [Obsolete("Use explicit ICloneable.Clone().")]
    // User code couldn't implement any new SqlNode, but could call Clone() method of existing nodes,
    // e.g of SqlSelect instances. Since we change publically available method in the middle of
    // 7.1, we have to make sure that user code will be at least compilable.
    //
    // Internal code base uses internal Clone(SqlNodeCloneContext) method or explicit interface implementation.
    // The method will be removed later but not in any of 7.1.x.
    public SqlNode Clone() => Clone(new SqlNodeCloneContext());

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    object ICloneable.Clone()
    {
      var context = new SqlNodeCloneContext();
      return Clone(context);
    }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    internal abstract SqlNode Clone(SqlNodeCloneContext context);

    internal SqlNode(SqlNodeType nodeType)
    {
      NodeType = nodeType;
    }

    public abstract void AcceptVisitor(ISqlVisitor visitor);
  }
}