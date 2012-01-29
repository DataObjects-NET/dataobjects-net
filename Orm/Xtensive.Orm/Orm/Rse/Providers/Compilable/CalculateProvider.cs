// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples.Transform;
using Xtensive.Collections;

namespace Xtensive.Orm.Rse.Providers.Compilable
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
    public bool IsInlined { get; private set; }

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
    protected override string ParametersToString()
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
    /// <param name="isInlined">The <see cref="IsInlined"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    public CalculateProvider(CompilableProvider source, bool isInlined, params CalculatedColumnDescriptor[] columnDescriptors)
      : base(ProviderType.Calculate, source)
    {
      IsInlined = isInlined;
      var columns = new CalculatedColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        var col = new CalculatedColumn(columnDescriptors[i], Source.Header.Length + i);
        columns.SetValue(col, i);
      }
      CalculatedColumns = columns;
    }
  }
}