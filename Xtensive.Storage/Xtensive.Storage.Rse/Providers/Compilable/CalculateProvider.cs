// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that calculates columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class CalculateProvider : UnaryProvider
  {
    /// <summary>
    /// Gets or sets a value indicating whether calculated columns could be inlined.
    /// </summary>
    /// <value><see langword="true" /> if columns could be inlined; otherwise, <see langword="false" />.</value>
    public bool CouldBeInlined { get; private set; }

    /// <summary>
    /// Gets the calculated columns.
    /// </summary>
    public CalculatedColumn[] CalculatedColumns { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform ResizeTransform { get; private set; }


    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header.Add(CalculatedColumns);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return CalculatedColumns.ToCommaDelimitedString();
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var columnIndexes = new int[Header.Length];
      for (int i = 0; i < columnIndexes.Length; i++)
        columnIndexes[i] = (i < Source.Header.Length) ? i : MapTransform.NoMapping;
      ResizeTransform = new MapTransform(false, Header.TupleDescriptor, columnIndexes);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    public CalculateProvider(CompilableProvider source, params CalculatedColumnDescriptor[] columnDescriptors)
      : this(source, false, columnDescriptors)
    {
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="couldBeInlined">The <see cref="CouldBeInlined"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    public CalculateProvider(CompilableProvider source, bool couldBeInlined, params CalculatedColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Calculate, source)
    {
      CouldBeInlined = couldBeInlined;
      var columns = new CalculatedColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        var col = new CalculatedColumn(columnDescriptors[i], Source.Header.Length + i);
        columns.SetValue(col, i);
      }
      CalculatedColumns = columns;
    }
  }
}