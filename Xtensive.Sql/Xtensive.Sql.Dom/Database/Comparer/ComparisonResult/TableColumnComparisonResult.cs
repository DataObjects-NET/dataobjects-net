// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using Xtensive.Sql.Dom.Dml;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class TableColumnComparisonResult : ComparisonResult<TableColumn>, IComparisonResult<DataTableColumn>
  {
    private IComparisonResult<SqlValueType> dataType;
    private DomainComparisonResult domain;
    private IComparisonResult<SqlExpression> defaultValue;
    private SequenceDescriptorComparisonResult sequenceDescriptor;
    private IComparisonResult<SqlExpression> expression;
    private IComparisonResult<bool> isPersisted;
    private NodeComparisonResult<Collation> collation;
    private IComparisonResult<bool> isNullable;

    #region IComparisonResult<DataTableColumn>

    /// <inheritdoc/>
    public DataTableColumn NewValue
    {
      get { return base.NewValue; }
    }

    /// <inheritdoc/>
    public DataTableColumn OriginalValue
    {
      get { return base.OriginalValue; }
    }

    #endregion

    public IComparisonResult<SqlValueType> DataType
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

    public IComparisonResult<SqlExpression> DefaultValue
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

    public IComparisonResult<SqlExpression> Expression
    {
      get { return expression; }
      set
      {
        this.EnsureNotLocked();
        expression = value;
      }
    }

    public IComparisonResult<bool> IsPersisted
    {
      get { return isPersisted; }
      set
      {
        this.EnsureNotLocked();
        isPersisted = value;
      }
    }

    public NodeComparisonResult<Collation> Collation
    {
      get { return collation; }
      set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    public IComparisonResult<bool> IsNullable
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
  }
}