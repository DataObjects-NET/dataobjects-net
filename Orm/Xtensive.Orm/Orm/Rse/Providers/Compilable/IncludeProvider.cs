// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

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
    public bool IsInlined { get; }

    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string ResultColumnName { get; }

    /// <summary>
    /// Gets the algorithm that performs filtering.
    /// For non-SQL storages value of this field has no effect.
    /// </summary>
    public IncludeAlgorithm Algorithm { get; }

    /// <summary>
    /// Gets the filtered columns.
    /// </summary>
    public IReadOnlyList<int> FilteredColumns { [DebuggerStepThrough] get; }

    /// <summary>
    /// Gets filter data.
    /// </summary>
    public Expression<Func<ParameterContext, IEnumerable<Tuple>>> FilterDataSource { get; }

    public TupleDescriptor FilteredTupleDescriptor { get; }


    // Constructors

    private static RecordSetHeader BuildHeaderAndFilteredTupleDescriptor(
      CompilableProvider source, IReadOnlyList<int> filteredColumns, string resultColumnName, out TupleDescriptor filteredTupleDescriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(filteredColumns, nameof(filteredColumns));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(resultColumnName, nameof(resultColumnName));

      var header = source.Header.Add(new SystemColumn(resultColumnName, 0, WellKnownTypes.Bool));
      var columnCount = filteredColumns.Count;
      var fieldTypes = new Type[columnCount];
      for (var index = 0; index < columnCount; index++) {
        fieldTypes[index] = header.Columns[filteredColumns[index]].Type;
      }
      filteredTupleDescriptor = TupleDescriptor.Create(fieldTypes);
      return header;
    }

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
      Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource, string resultColumnName, IReadOnlyList<int> filteredColumns)
      : base(ProviderType.Include, BuildHeaderAndFilteredTupleDescriptor(source, filteredColumns, resultColumnName, out var filteredTupleDescriptor), source)
    {
      ArgumentValidator.EnsureArgumentNotNull(filterDataSource, nameof(FilterDataSource));
      Algorithm = algorithm;
      IsInlined = isInlined;
      FilterDataSource = filterDataSource;
      ResultColumnName = resultColumnName;

      FilteredColumns = filteredColumns;
      FilteredTupleDescriptor = filteredTupleDescriptor;
    }
  }
}