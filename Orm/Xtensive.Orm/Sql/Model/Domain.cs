// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a domain object that is a set of permissible values. 
  /// A domain is defined in a schema and is identified by a domain name. 
  /// The purpose of a domain is to constrain the set of valid values 
  /// that can be stored in SQL-data by various operations.
  /// </summary>
  [Serializable]
  public class Domain : SchemaNode,IConstrainable
  {
    private SqlValueType dataType;
    private SqlExpression defaultValue;
    private readonly PairedNodeCollection<Domain, DomainConstraint> constraints;
    private Collation collation;

    /// <summary>
    /// Creates the domain constraint.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="condition">The condition.</param>
    public DomainConstraint CreateConstraint(string name, SqlExpression condition)
    {
      return new DomainConstraint(this, name, condition);
    }

    /// <summary>
    /// Gets or sets the data type of the domain.
    /// </summary>
    /// <value>The data type.</value>
    public SqlValueType DataType
    {
      get { return dataType; }
      set
      {
        this.EnsureNotLocked();
        dataType = value;
      }
    }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <value>The default value.</value>
    public SqlExpression DefaultValue
    {
      get { return defaultValue; }
      set
      {
        this.EnsureNotLocked();
        defaultValue = value;
      }
    }

    /// <summary>
    /// Gets or sets the collation.
    /// </summary>
    /// <value>The collation.</value>
    public Collation Collation
    {
      get { return collation; }
      set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    /// <summary>
    /// Gets the constraints.
    /// </summary>
    /// <value>The constraints.</value>
    public PairedNodeCollection<Domain, DomainConstraint> DomainConstraints
    {
      get { return constraints; }
    }

    #region IConstrainable members

    /// <summary>
    /// Gets the constraints.
    /// </summary>
    /// <value>The constraints.</value>
    IList<Constraint> IConstrainable.Constraints
    {
      get
      {
        return constraints.ToArray().Convert(i => (Constraint)i);
      }
    }

    #endregion

    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Domains.Remove(this);
      if (value!=null)
        value.Domains.Add(this);
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
      constraints.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal Domain(Schema schema, string name, SqlValueType dataType, SqlExpression defaultValue) : base(schema, name)
    {
      this.dataType = dataType;
      this.defaultValue = defaultValue;
      constraints =
        new PairedNodeCollection<Domain, DomainConstraint>(this, "DomainConstraints");
    }

    #endregion
  }
}