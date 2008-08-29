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
  [Serializable]
  public class TableColumnComparisonResult : DataTableColumnComparisonResult,
    IComparisonResult<TableColumn>
  {
    private ComparisonResult<SqlValueType> dataType;
    private DomainComparisonResult domain;
    private ComparisonResult<SqlExpression> defaultValue;
    private SequenceDescriptorComparisonResult sequenceDescriptor;
    private ComparisonResult<SqlExpression> expression;
    private ComparisonResult<bool> isPersisted;
    private CollationComparisonResult collation;
    private ComparisonResult<bool> isNullable;


    /// <inheritdoc/>
    public new TableColumn NewValue
    {
      get { return (TableColumn) base.NewValue; }
    }

    /// <inheritdoc/>
    public new TableColumn OriginalValue
    {
      get { return (TableColumn) base.OriginalValue; }
    }

    public ComparisonResult<SqlValueType> DataType
    {
      get { return dataType; }
      set
      {
        this.EnsureNotLocked();
        dataType = value;
      }
    }

    public DomainComparisonResult Domain
    {
      get { return domain; }
      set
      {
        this.EnsureNotLocked();
        domain = value;
      }
    }

    public ComparisonResult<SqlExpression> DefaultValue
    {
      get { return defaultValue; }
      set
      {
        this.EnsureNotLocked();
        defaultValue = value;
      }
    }

    public SequenceDescriptorComparisonResult SequenceDescriptor
    {
      get { return sequenceDescriptor; }
      set
      {
        this.EnsureNotLocked();
        sequenceDescriptor = value;
      }
    }

    public ComparisonResult<SqlExpression> Expression
    {
      get { return expression; }
      set
      {
        this.EnsureNotLocked();
        expression = value;
      }
    }

    public ComparisonResult<bool> IsPersisted
    {
      get { return isPersisted; }
      set
      {
        this.EnsureNotLocked();
        isPersisted = value;
      }
    }

    public CollationComparisonResult Collation
    {
      get { return collation; }
      set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    public ComparisonResult<bool> IsNullable
    {
      get { return isNullable; }
      set
      {
        this.EnsureNotLocked();
        isNullable = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        dataType.LockSafely(recursive);
        domain.LockSafely(recursive);
        defaultValue.LockSafely(recursive);
        sequenceDescriptor.LockSafely(recursive);
        expression.LockSafely(recursive);
        isPersisted.LockSafely(recursive);
        collation.LockSafely(recursive);
        isNullable.LockSafely(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TableColumnComparisonResult(TableColumn originalValue, TableColumn newValue)
      : base(originalValue, newValue)
    {
    }
  }
}