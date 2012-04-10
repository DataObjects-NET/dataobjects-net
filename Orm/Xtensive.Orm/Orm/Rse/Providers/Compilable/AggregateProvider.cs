// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.18

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that applies aggregate functions to grouped columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class AggregateProvider : UnaryProvider
  {
    private const string ToStringFormat = "{0}, Group by ({1})";

    /// <summary>
    /// Gets the aggregate columns.
    /// </summary>
    public AggregateColumn[] AggregateColumns { get; private set; }

    /// <summary>
    /// Gets column indexes to group by.
    /// </summary>
    public int[] GroupColumnIndexes { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform Transform { get; private set; }

    
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header
        .Select(GroupColumnIndexes)
        .Add(AggregateColumns);
    }

    
    protected override string ParametersToString()
    {
      if (GroupColumnIndexes.Length==0)
        return AggregateColumns.ToCommaDelimitedString();
      else
        return string.Format(ToStringFormat,
          AggregateColumns.ToCommaDelimitedString(), 
          GroupColumnIndexes.ToCommaDelimitedString());
    }

    
    protected override void Initialize()
    {
      base.Initialize();
      var types = new List<Type>();
      int i = 0;
      var columnIndexes = new int[GroupColumnIndexes.Length];
      foreach (var index in GroupColumnIndexes) {
        types.Add(Source.Header.Columns[index].Type);
        columnIndexes[i++] = index;
      }
      Transform = new MapTransform(false, TupleDescriptor.Create(types), columnIndexes);
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
        return typeof (long);
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
        if (sourceColumnType == typeof(TimeSpan))
          return sourceColumnType;
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
        return typeof(double);
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

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="AggregateColumns"/>.</param>
    /// <param name="groupIndexes">The column indexes to group by.</param>
    public AggregateProvider(CompilableProvider source, int[] groupIndexes, params AggregateColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Aggregate, source)
    {
      groupIndexes = groupIndexes ?? ArrayUtils<int>.EmptyArray;
      var columns = new AggregateColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        AggregateColumnDescriptor descriptor = columnDescriptors[i];
        var type = GetAggregateColumnType(Source.Header.Columns[descriptor.SourceIndex].Type, descriptor.AggregateType);
        columns[i] = new AggregateColumn(descriptor, groupIndexes.Length + i, type);
      }
      AggregateColumns = columns;
      GroupColumnIndexes = groupIndexes;
    }
  }
}
