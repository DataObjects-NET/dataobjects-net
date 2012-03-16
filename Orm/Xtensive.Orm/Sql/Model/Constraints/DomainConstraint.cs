// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a <see cref="Domain"/> constraint object.
  /// </summary>
  [Serializable]
  public class DomainConstraint : Constraint<Domain>
  {
    /// <summary>
    /// Gets or sets the <see cref="Domain"/> this instance belongs to.
    /// </summary>
    /// <value>The domain.</value>
    public Domain Domain
    {
      get { return Owner; }
      set { Owner = value; }
    }

    #region Constraint<T> Members

    /// <summary>
    /// Changes the domain.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeOwner(Domain value)
    {
      if (Domain!=null)
        Domain.DomainConstraints.Remove(this);
      if (value!=null)
        value.DomainConstraints.Add(this);
    }

    #endregion

    #region Constructors

    internal DomainConstraint(Domain domain, string name, SqlExpression condition, bool? isDeferrable, bool? isInitiallyDeferred) : base(name, condition, isDeferrable, isInitiallyDeferred)
    {
      Domain = domain;
    }

    internal DomainConstraint(Domain domain, string name, SqlExpression condition) : this(domain, name, condition, null, null)
    {
      Domain = domain;
    }

    #endregion
  }
}