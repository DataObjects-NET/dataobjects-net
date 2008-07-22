// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
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

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Source.Header.TupleDescriptor, Source.Header.RecordColumnCollection, Source.Header.OrderInfo.KeyDescriptor, Source.Header.Keys, IndexSortOrder);
    }

    protected override void Initialize()
    {}


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