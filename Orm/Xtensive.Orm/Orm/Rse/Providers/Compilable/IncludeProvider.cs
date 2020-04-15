// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using System.Linq;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if source value equal to one of provided values; 
  /// otherwise, <see langword="false" />.
  /// </summary>
  [Serializable]
  public sealed class IncludeProvider: UnaryProvider,
    IInlinableProvider
  {
    /// <summary>
    /// Gets a value indicating whether result column should be inlined.
    /// </summary>
    public bool IsInlined { get; private set; }

    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string ResultColumnName { get; private set; }

    /// <summary>
    /// Gets the algorithm that performs filtering.
    /// For non-SQL storages value of this field has no effect.
    /// </summary>
    public IncludeAlgorithm Algorithm { get; private set; }

    /// <summary>
    /// Gets the filtered columns.
    /// </summary>
    public IReadOnlyList<int> FilteredColumns { [DebuggerStepThrough] get; }

    /// <summary>
    /// Gets filter data.
    /// </summary>
    public Expression<Func<IEnumerable<Tuple>>> FilterDataSource { get; private set; }

    public MapTransform FilteredColumnsExtractionTransform { get; private set; }

    public CombineTransform ResultTransform { get; private set; }

    private static readonly Type BoolType = typeof(bool);
    private static readonly TupleDescriptor BoolTupleDescriptor = TupleDescriptor.Create(new[] {BoolType});

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      var newHeader = Source.Header.Add(new SystemColumn(ResultColumnName, 0, BoolType));
      var fieldTypes = new Type[FilteredColumns.Count];
      for (var index = 0; index < fieldTypes.Length; index++) {
        fieldTypes[index] = newHeader.Columns[FilteredColumns[index]].Type;
      }
      var tupleDescriptor = TupleDescriptor.Create(fieldTypes);
      FilteredColumnsExtractionTransform = new MapTransform(true, tupleDescriptor, FilteredColumns);
      ResultTransform = new CombineTransform(true, Source.Header.TupleDescriptor, BoolTupleDescriptor);
      return newHeader;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">A value for <see cref="UnaryProvider.Source"/>.</param>
    /// <param name="algorithm">A value for <see cref="Algorithm"/>.</param>
    /// <param name="isInlined">A value for <see cref="IsInlined"/>.</param>
    /// <param name="filterDataSource">A value for <see cref="FilterDataSource"/>.</param>
    /// <param name="resultColumnName">A value for <see cref="ResultColumnName"/>.</param>
    /// <param name="filteredColumns">A value for <see cref="FilteredColumns"/>.</param>
    public IncludeProvider(CompilableProvider source, IncludeAlgorithm algorithm, bool isInlined,
      Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, IReadOnlyList<int> filteredColumns)
      : base(ProviderType.Include, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(filterDataSource, "filterDataSource");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(resultColumnName, "resultColumnName");
      ArgumentValidator.EnsureArgumentNotNull(filteredColumns, "filteredColumns");
      Algorithm = algorithm;
      IsInlined = isInlined;
      FilterDataSource = filterDataSource;
      ResultColumnName = resultColumnName;

      switch (filteredColumns) {
        case int[] columnArray:
          FilteredColumns = Array.AsReadOnly(columnArray);
          break;
        case List<int> columnList:
          FilteredColumns = columnList.AsReadOnly();
          break;
        default:
          FilteredColumns = filteredColumns;
          break;
      }

      Initialize();
    }
  }
}