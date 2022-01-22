// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
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
      CompilableProvider source, CalculatedColumnDescriptor[] columnDescriptors, out CalculatedColumn[] calculatedColumns)
    {
      var sourceHeader = source.Header;
      var sourceHeaderLength = sourceHeader.Length;
      calculatedColumns = new CalculatedColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        calculatedColumns[i] = new CalculatedColumn(columnDescriptors[i], sourceHeaderLength + i);
      }

      return sourceHeader.Add(calculatedColumns);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
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
    public CalculateProvider(CompilableProvider source, bool isInlined, params CalculatedColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Calculate, BuildHeaderAndColumns(source, columnDescriptors, out var calculatedColumns), source)
    {
      IsInlined = isInlined;
      CalculatedColumns = calculatedColumns;
    }
  }
}