// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that calculates columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class CalculateProvider : UnaryProvider,
    IInlinableProvider
  {
    /// <summary>
    /// Gets a value indicating whether calculated columns should be inlined.
    /// </summary>
    public bool IsInlined { get; }

    /// <summary>
    /// Gets the calculated columns.
    /// </summary>
    public CalculatedColumn[] CalculatedColumns { get; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return CalculatedColumns.ToCommaDelimitedString();
    }


    // Constructors

    private static RecordSetHeader BuildHeaderAndColumns(
      CompilableProvider source,
      IReadOnlyList<CalculatedColumnDescriptor> columnDescriptors,
      out CalculatedColumn[] calculatedColumns)
    {
      var sourceHeader = source.Header;
      var sourceHeaderLength = sourceHeader.Length;
      var descriptorsCount = columnDescriptors.Count;
      calculatedColumns = new CalculatedColumn[descriptorsCount];
      for (int i = 0; i < descriptorsCount; i++) {
        calculatedColumns[i] = new CalculatedColumn(columnDescriptors[i], sourceHeaderLength + i);
      }

      return sourceHeader.Add(calculatedColumns);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    [Obsolete]
    public CalculateProvider(CompilableProvider source, params CalculatedColumnDescriptor[] columnDescriptors)
      : this(source, false, columnDescriptors)
    {
    }

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="isInlined">The <see cref="IsInlined"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    [Obsolete]
    public CalculateProvider(CompilableProvider source, bool isInlined, params CalculatedColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Calculate, BuildHeaderAndColumns(source, columnDescriptors, out var calculatedColumns), source)
    {
      IsInlined = isInlined;
      CalculatedColumns = calculatedColumns;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="isInlined">The <see cref="IsInlined"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    public CalculateProvider(CompilableProvider source, IReadOnlyList<CalculatedColumnDescriptor> columnDescriptors, bool isInlined = false)
      : base(ProviderType.Calculate, BuildHeaderAndColumns(source, columnDescriptors, out var calculatedColumns), source)
    {
      IsInlined = isInlined;
      CalculatedColumns = calculatedColumns;
    }
  }
}