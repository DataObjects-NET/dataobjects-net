// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that makes runtime index over the <see cref="UnaryProvider.Source"/> values using <see cref="IndexSortOrder"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexingProvider : UnaryProvider
  {
    /// <summary>
    /// Sort order of the index.
    /// </summary>
    public DirectionCollection<int> IndexSortOrder { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return new RecordSetHeader(Source.Header.TupleDescriptor, Source.Header.Columns, Source.Header.OrderDescriptor.Descriptor, Source.Header.ColumnGroups, IndexSortOrder);
    }

    protected override void Initialize()
    {}

    public override string GetStringParameters()
    {
      return IndexSortOrder
        .Select(pair => Header.Columns[pair.Key].Name + (pair.Value == Direction.Negative ? " desc" : string.Empty))
        .ToCommaDelimitedString();
    }

    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnIndexes">The <see cref="IndexSortOrder"/> property value.</param>
    public IndexingProvider(CompilableProvider source, DirectionCollection<int> columnIndexes)
      : base(source)
    {
      IndexSortOrder = columnIndexes;
    }
  }
}