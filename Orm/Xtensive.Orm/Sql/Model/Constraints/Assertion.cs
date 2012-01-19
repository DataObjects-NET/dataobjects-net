// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a named constraint that may relate to the content
  /// of individual rows of a table, to the entire contents of a table,
  /// or to a state required to exist among a number of tables.
  /// </summary>
  [Serializable]
  public class Assertion : SchemaNode
  {
    private Constraint constraint;

    /// <summary>
    /// Gets or sets the check condition for this instance.
    /// </summary>
    /// <value>The condition.</value>
    public SqlExpression Condition
    {
      get { return constraint.Condition; }
      set { constraint.Condition = value; }
    }

    /// <summary>
    /// Indicates that the constraint is deferrable or not.
    /// </summary>
    /// <value></value>
    public bool? IsDeferrable
    {
      get { return constraint.IsDeferrable; }
      set { constraint.IsDeferrable = value; }
    }

    /// <summary>
    /// Indicates that the initial constraint mode is deferred or not.
    /// </summary>
    /// <value></value>
    public bool? IsInitiallyDeferred
    {
      get { return constraint.IsInitiallyDeferred; }
      set { constraint.IsInitiallyDeferred = value; }
    }

    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Assertions.Remove(this);
      if (value!=null)
        value.Assertions.Add(this);
    }

    #endregion

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      constraint.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal Assertion(Schema schema, string name, SqlExpression condition, bool? isDeferrable, bool? isInitiallyDeferred) : base(schema, name)
    {
      constraint = new Constraint(name, condition, isDeferrable, isInitiallyDeferred);
    } 

    #endregion
  }
}