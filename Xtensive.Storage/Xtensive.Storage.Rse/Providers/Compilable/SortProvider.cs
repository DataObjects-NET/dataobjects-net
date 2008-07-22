// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares sort operation over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SortProvider : UnaryProvider
  {
    private RecordHeader header;

    /// <summary>
    /// Sort order columns indexes.
    /// </summary>
    public DirectionCollection<int> SortOrder { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return header;
    }

    protected override void Initialize()
    {
      header = new RecordHeader(Source.Header.TupleDescriptor, Source.Header.RecordColumnCollection, Source.Header.OrderInfo.KeyDescriptor, Source.Header.Keys, SortOrder);
    }

    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SortProvider(CompilableProvider provider, DirectionCollection<int> tupleSortOrder)
      : base(provider)
    {
      SortOrder = tupleSortOrder;
      Initialize();
    }
  }
}