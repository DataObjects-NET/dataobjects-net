// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class TableColumnComparisonResult : ComparisonResult<TableColumn>, IComparisonResult<DataTableColumn>
  {
    private IComparisonResult<SqlValueType> dataType;
    private IComparisonResult<Domain> domain;
    private IComparisonResult<SqlExpression> defaultValue;
    private IComparisonResult<SequenceDescriptor> sequenceDescriptor;
    private IComparisonResult<SqlExpression> expression;
    private IComparisonResult<bool> isPersisted;
    private IComparisonResult<Collation> collation;
    private IComparisonResult<bool> isNullable;

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
  }
}