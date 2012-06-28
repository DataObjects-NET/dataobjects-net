// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Aggregate column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public sealed class AggregateColumn : Column
  {
    private const string ToStringFormat = "{0} = {1} on ({2})";

    /// <summary>
    /// Gets the aggregate function.
    /// </summary>
    public AggregateType AggregateType { get; private set; }

    /// <summary>
    /// Gets the source column index.
    /// </summary>
    public int SourceIndex { get; private set; }

    /// <summary>
    /// Gets column descriptor.
    /// </summary>
    public AggregateColumnDescriptor Descriptor { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat,
        base.ToString(), AggregateType, SourceIndex);
    }

    /// <inheritdoc/>
    public override Column Clone(int newIndex)
    {
      return new AggregateColumn(this, newIndex);
    }

    /// <inheritdoc/>
    public override Column Clone(string newName)
    {
      return new AggregateColumn(this, newName);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="descriptor"><see cref="AggregateColumnDescriptor"/> property value.</param>
    /// <param name="index"><see cref="SourceIndex"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public AggregateColumn(AggregateColumnDescriptor descriptor, int index, Type type)
      : base(descriptor.Name, index, type)
    {
      AggregateType = descriptor.AggregateType;
      SourceIndex = descriptor.SourceIndex;
      Descriptor = descriptor;
    }

    #region Clone constructors

    private AggregateColumn(AggregateColumn column, string newName)
      : base(newName, column.Index, column.Type)
    {
      AggregateType = column.AggregateType;
      SourceIndex = column.SourceIndex;
    }

    private AggregateColumn(AggregateColumn column, int newIndex)
      : base(column.Name, newIndex, column.Type)
    {
      AggregateType = column.AggregateType;
      SourceIndex = column.SourceIndex;
    }

    #endregion
  }
}