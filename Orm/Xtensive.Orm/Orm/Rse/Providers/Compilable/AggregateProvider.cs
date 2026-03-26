// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2008.09.18

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that applies aggregate functions to grouped columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class AggregateProvider : UnaryProvider
  {
    private const string ToStringFormatGroupOnly = "Group by ({0})";
    private const string ToStringFormatAggregateOnly = "{0}";
    private const string ToStringFormatFull = "{0}, Group by ({1})";

    /// <summary>
    /// Gets the aggregate columns.
    /// </summary>
    public AggregateColumn[] AggregateColumns { get; }

    /// <summary>
    /// Gets column indexes to group by.
    /// </summary>
    public int[] GroupColumnIndexes { get; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      if (AggregateColumns.Length==0)
        return string.Format(
          ToStringFormatGroupOnly,
          GroupColumnIndexes.ToCommaDelimitedString());

      if (GroupColumnIndexes.Length==0)
        return string.Format(
          ToStringFormatAggregateOnly,
          AggregateColumns.ToCommaDelimitedString());

      return string.Format(
        ToStringFormatFull,
        AggregateColumns.ToCommaDelimitedString(),
        GroupColumnIndexes.ToCommaDelimitedString());
    }

    /// <summary>
    /// Gets the type of the aggregate column according to a <see cref="AggregateType"/> and original column type.
    /// </summary>
    /// <param name="sourceColumnType">Type of the source column.</param>
    /// <param name="aggregateType">Type of the aggregate.</param>
    /// <returns>The type of aggregate column.</returns>
    public static Type GetAggregateColumnType(Type sourceColumnType, AggregateType aggregateType)
    {
      // TODO: very stupid - remove when nullables handing fixed everywhere.
      if (sourceColumnType.IsNullable())
        sourceColumnType = sourceColumnType.GetGenericArguments()[0];
      switch (aggregateType) {
      case AggregateType.Count:
        return WellKnownTypes.Int64;
      case AggregateType.Min:
      case AggregateType.Max:
        return GetMinMaxColumnType(sourceColumnType, aggregateType);
      case AggregateType.Sum:
        return GetSumColumnType(sourceColumnType);
      case AggregateType.Avg:
        return GetAvgColumnType(sourceColumnType);
      default:
        throw AggregateNotSupported(sourceColumnType, aggregateType);
      }
    }

    #region Private / internal methods

    private static Type GetMinMaxColumnType(Type sourceColumnType, AggregateType aggregateType)
    {
      switch (System.Type.GetTypeCode(sourceColumnType)) {
      case TypeCode.Char:
      case TypeCode.SByte:
      case TypeCode.Byte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
      case TypeCode.Decimal:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.String:
      case TypeCode.DateTime:
        return sourceColumnType;
      default:
        if (sourceColumnType==WellKnownTypes.TimeSpan || sourceColumnType==WellKnownTypes.DateTimeOffset)
          return sourceColumnType;
#if NET6_0_OR_GREATER
        if (sourceColumnType == WellKnownTypes.DateOnly || sourceColumnType == WellKnownTypes.TimeOnly)
          return sourceColumnType;
#endif
        throw AggregateNotSupported(sourceColumnType, aggregateType);
      }
    }

    private static Type GetSumColumnType(Type sourceColumnType)
    {
      switch (System.Type.GetTypeCode(sourceColumnType)) {
      case TypeCode.SByte:
      case TypeCode.Byte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
      case TypeCode.Decimal:
      case TypeCode.Single:
      case TypeCode.Double:
        return sourceColumnType;
      default:
        throw AggregateNotSupported(sourceColumnType, AggregateType.Sum);
      }
    }

    private static Type GetAvgColumnType(Type sourceColumnType)
    {
      switch (System.Type.GetTypeCode(sourceColumnType)) {
      case TypeCode.SByte:
      case TypeCode.Byte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
        return WellKnownTypes.Double;
      case TypeCode.Decimal:
      case TypeCode.Single:
      case TypeCode.Double:
        return sourceColumnType;
      default:
        throw AggregateNotSupported(sourceColumnType, AggregateType.Avg);
      }
    }

    private static NotSupportedException AggregateNotSupported(Type sourceColumnType, AggregateType aggregateType)
    {
      return new NotSupportedException(string.Format(
        Strings.ExAggregateXIsNotSupportedForTypeY, aggregateType, sourceColumnType));
    }

    #endregion

    private static RecordSetHeader BuildHeaderAndColumns(
      CompilableProvider source,
      IReadOnlyList<AggregateColumnDescriptor> columnDescriptors,
      ref int[] groupIndexes,
      out AggregateColumn[] aggregateColumns)
    {
      groupIndexes ??= Array.Empty<int>();
      var descriptorsCount = columnDescriptors.Count;
      aggregateColumns = new AggregateColumn[descriptorsCount];
      var sourceHeader = source.Header;
      var sourceHeaderColumns = sourceHeader.Columns;
      for (int i = 0; i < descriptorsCount; i++) {
        var agrColumnDescriptor = columnDescriptors[i];
        var type = GetAggregateColumnType(sourceHeaderColumns[agrColumnDescriptor.SourceIndex].Type, agrColumnDescriptor.AggregateType);
        aggregateColumns[i] = new AggregateColumn(agrColumnDescriptor, groupIndexes.Length + i, type);
      }

      return sourceHeader.Select(groupIndexes).Add(aggregateColumns);
    }

    private static RecordSetHeader BuildHeaderAndColumns(
      CompilableProvider source,
      IReadOnlyList<AggregateColumn> columns,
      ref int[] groupIndexes,
      out AggregateColumn[] aggregateColumns)
    {
      groupIndexes ??= Array.Empty<int>();
      var descriptorsCount = columns.Count;
      aggregateColumns = new AggregateColumn[descriptorsCount];
      var sourceHeader = source.Header;
      var sourceHeaderColumns = sourceHeader.Columns;
      for (int i = 0; i < descriptorsCount; i++) {
        var agrColumnDescriptor = columns[i].Descriptor;
        var type = GetAggregateColumnType(sourceHeaderColumns[agrColumnDescriptor.SourceIndex].Type, agrColumnDescriptor.AggregateType);
        aggregateColumns[i] = new AggregateColumn(agrColumnDescriptor, groupIndexes.Length + i, type);
      }

      return sourceHeader.Select(groupIndexes).Add(aggregateColumns);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="groupIndexes">The column indexes to group by.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="AggregateColumns"/>.</param>
    [Obsolete]
    public AggregateProvider(CompilableProvider source, int[] groupIndexes, params AggregateColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Aggregate, BuildHeaderAndColumns(source, columnDescriptors, ref groupIndexes, out var columns), source)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="groupIndexes">The column indexes to group by.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="AggregateColumns"/>.</param>
    public AggregateProvider(CompilableProvider source, int[] groupIndexes, IReadOnlyList<AggregateColumnDescriptor> columnDescriptors)
      : base(ProviderType.Aggregate, BuildHeaderAndColumns(source, columnDescriptors, ref groupIndexes, out var columns), source)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnDescriptors, nameof(columnDescriptors));
      AggregateColumns = columns;
      GroupColumnIndexes = groupIndexes;
    }

    /// <summary>
    /// Initializes a new instance of this class. Internal use only!
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="groupIndexes">The column indexes to group by.</param>
    /// <param name="descriptorSource">Columns of old AggregateProvider as source of descriptors.</param>
    internal AggregateProvider(CompilableProvider source, int[] groupIndexes, IReadOnlyList<AggregateColumn> descriptorSource)
      : base(ProviderType.Aggregate, BuildHeaderAndColumns(source, descriptorSource, ref groupIndexes, out var columns), source)
    {
      // Having this dedicated ctor saves some resources on not having to make
      // an array just to pass descriptors for simple enumeration
      AggregateColumns = columns;
      GroupColumnIndexes = groupIndexes;
    }

  }
}
