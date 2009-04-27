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
    public SystemColumn SystemColumn { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform ResizeTransform { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var columnIndexes = new int[Header.Length];
      for (int i = 0; i < columnIndexes.Length; i++)
        columnIndexes[i] = (i < Source.Header.Length) ? i : MapTransform.NoMapping;
      ResizeTransform = new MapTransform(false, Header.TupleDescriptor, columnIndexes);
    }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header.Add(EnumerableUtils.One((Column) SystemColumn));
    }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return new DirectionCollection<int> {Header.Columns.IndexOf(SystemColumn)};
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnName">The name of <see cref="SystemColumn"/>.</param>
    public RowNumberProvider(CompilableProvider source, string columnName)
      : base(ProviderType.RowNumber, source)
    {
      SystemColumn = new SystemColumn(columnName, Source.Header.Length, typeof(long));
    }
  }
}