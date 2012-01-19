// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents base lockable named node.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Name = {Name}")]
  public abstract class Node: LockableBase
  {
    private string name;
    private string dbName;

    /// <summary>
    /// Gets or sets a name of the node.
    /// </summary>
    public virtual string Name
    {
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets a db name of the node.
    /// </summary>
    public virtual string DbName
    {
      get { return string.IsNullOrEmpty(dbName) ? Name : dbName; }
      set
      {
        this.EnsureNotLocked();
        dbName = value;
      }
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    protected Node(string name)
    {
      this.name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    protected Node()
    {
    }

    #endregion
  }
}