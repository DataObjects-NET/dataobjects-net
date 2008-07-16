// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Providers
{
  public class IndexingProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public DirectionCollection<int> ColumnIndexes { get; private set; }

    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Indexed | ProviderOptions.Ordered | ProviderOptions.FastCount; }
    }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Source.Header.TupleDescriptor, Source.Header.RecordColumnCollection, Source.Header.OrderInfo.KeyDescriptor, Source.Header.Keys, ColumnIndexes);
    }

    // Constructor
    public IndexingProvider(CompilableProvider source, DirectionCollection<int> columnIndexes)
      : base(source)
    {
      Source = source;
      ColumnIndexes = columnIndexes;
    }
  }
}