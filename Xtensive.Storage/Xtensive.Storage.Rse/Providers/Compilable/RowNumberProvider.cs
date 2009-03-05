// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.05

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that adds row number to <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RowNumberProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the row number column.
    /// </summary>
    public RowNumberColumn RowNumberColumn { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform ResizeTransform { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var columnIndexes = new int[Header.Columns.Count];
      for (int i = 0; i < columnIndexes.Length; i++)
        columnIndexes[i] = (i < Source.Header.Columns.Count) ? i : MapTransform.NoMapping;
      ResizeTransform = new MapTransform(false, Header.TupleDescriptor, columnIndexes);
    }

    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header.Add(EnumerableUtils.One<Column>(RowNumberColumn));
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnName">The name of <see cref="RowNumberColumn"/>.</param>
    public RowNumberProvider(CompilableProvider source, string columnName)
      : base(ProviderType.RowNumber, source)
    {
      RowNumberColumn = new RowNumberColumn(columnName, Source.Header.Columns.Count);
    }
  }
}