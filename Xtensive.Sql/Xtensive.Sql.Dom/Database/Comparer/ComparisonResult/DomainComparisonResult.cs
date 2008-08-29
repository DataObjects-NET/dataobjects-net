// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Domain"/> comparison result.
  /// </summary>
  [Serializable]
  public class DomainComparisonResult : NodeComparisonResult, 
    IComparisonResult<Domain>
  {
    private ComparisonResult<SqlValueType> dataType;
    private ComparisonResult<SqlExpression> defaultValue;
    private readonly ComparisonResultCollection<DomainConstraintComparisonResult> domainConstraints = new ComparisonResultCollection<DomainConstraintComparisonResult>();
    private CollationComparisonResult collation;

    /// <inheritdoc/>
    public new Domain NewValue
    {
      get { return (Domain) base.NewValue; }
    }

    /// <inheritdoc/>
    public new Domain OriginalValue
    {
      get { return (Domain) base.OriginalValue; }
    }

    /// <summary>
    /// Gets <see cref="Domain.DataType"/> comparison result.
    /// </summary>
    public ComparisonResult<SqlValueType> DataType
    {
      get { return dataType; }
      internal set
      {
        this.EnsureNotLocked();
        dataType = value;
      }
    }

    /// <summary>
    /// Gets <see cref="Domain.DefaultValue"/> comparison result.
    /// </summary>
    public ComparisonResult<SqlExpression> DefaultValue
    {
      get { return defaultValue; }
      internal set
      {
        this.EnsureNotLocked();
        defaultValue = value;
      }
    }

    /// <summary>
    /// Gets nested constraints comparison results.
    /// </summary>
    public ComparisonResultCollection<DomainConstraintComparisonResult> DomainConstraints
    {
      get { return domainConstraints; }
    }

    /// <summary>
    /// Gets <see cref="Domain.Collation"/> comparison result.
    /// </summary>
    public CollationComparisonResult Collation
    {
      get { return collation; }
      internal set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        dataType.LockSafely(recursive);
        defaultValue.LockSafely(recursive);
        domainConstraints.Lock(recursive);
        collation.LockSafely(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DomainComparisonResult(Domain originalValue, Domain newValue)
      : base(originalValue, newValue)
    {
    }
  }
}