// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents integrity constraint object.
  /// </summary>
  [Serializable]
  public class Constraint : Node
  {
    private bool? isDeferrable;
    private bool? isInitiallyDeferred;
    private SqlExpression condition;

    /// <summary>
    /// Indicates that the constraint is deferrable or not.
    /// </summary>
    /// <value></value>
    public bool? IsDeferrable
    {
      get { return isDeferrable; }
      set
      {
        this.EnsureNotLocked();
        isDeferrable = value;
      }
    }

    /// <summary>
    /// Indicates that the initial constraint mode is deferred or not.
    /// </summary>
    /// <value></value>
    public bool? IsInitiallyDeferred
    {
      get { return isInitiallyDeferred; }
      set
      {
        this.EnsureNotLocked();
        isInitiallyDeferred = value;
      }
    }

    /// <summary>
    /// Gets or sets the check condition for this instance.
    /// </summary>
    /// <value>The condition.</value>
    public SqlExpression Condition
    {
      get { return condition; }
      set
      {
        this.EnsureNotLocked();
        condition = value;
      }
    }
    
    #region Constructors

    internal protected Constraint(string name, SqlExpression condition, bool? isDeferrable, bool? isInitiallyDeferred) : base(name)
    {
      this.isDeferrable = isDeferrable;
      this.isInitiallyDeferred = isInitiallyDeferred;
      this.condition = condition;
    }

    #endregion
  }

  /// <summary>
  /// Represents integrity constraint object.
  /// </summary>
  [Serializable]
  public abstract class Constraint<T> : Constraint, IPairedNode<T> where T: Node, IConstrainable
  {
    private T owner;

    /// <summary>
    /// Gets or sets the constraint owner.
    /// </summary>
    /// <value>The owner.</value>
    public T Owner
    {
      get { return owner; }
      set
      {
        this.EnsureNotLocked();
        ChangeOwner(value);
      }
    }

    /// <summary>
    /// Changes the owner.
    /// </summary>
    /// <param name="value">The owner.</param>
    protected abstract void ChangeOwner(T value);

    #region IPairedNode<T> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<T>.UpdatePairedProperty(string property, T value)
    {
      this.EnsureNotLocked();
      owner = value;
    }

    #endregion

    #region Constructors

    internal protected Constraint(string name, SqlExpression condition, bool? isDeferrable, bool? isInitiallyDeferred) : base(name, condition, isDeferrable, isInitiallyDeferred)
    {
    }

    #endregion
  }
}
