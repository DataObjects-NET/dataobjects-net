// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that calculates columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class CalculationProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the calculated columns.
    /// </summary>
    public CalculatedColumn[] CalculatedColumns { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform ResizeTransform{ get; private set; }


    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      RecordSetHeader header = Source.Header;
      header = header.Add(CalculatedColumns);
      return header;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var columnIndexes = new int[Header.Columns.Count];
      for (int i = 0; i < columnIndexes.Length; i++)
        columnIndexes[i] = (i < Source.Header.Columns.Count) ? i : MapTransform.NoMapping;
      ResizeTransform = new MapTransform(false, Header.TupleDescriptor, columnIndexes);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="CalculatedColumns"/>.</param>
    public CalculationProvider(CompilableProvider source, params CalculatedColumnDescriptor[] columnDescriptors)
      : base(source)
    {
      var columns = new CalculatedColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        var col = new CalculatedColumn(columnDescriptors[i], Source.Header.Columns.Count + i);
        columns.SetValue(col, i);
      }
      CalculatedColumns = columns;
    }
  }
}