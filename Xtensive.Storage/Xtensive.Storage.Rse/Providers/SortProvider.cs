// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Providers
{
  [Serializable]
  public class SortProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public DirectionCollection<int> TupleSortOrder { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Source.Header.TupleDescriptor, Source.Header.RecordColumnCollection, Source.Header.OrderInfo.KeyDescriptor, Source.Header.Keys, TupleSortOrder);
    }


    // Constructor

    public SortProvider(CompilableProvider provider, DirectionCollection<int> tupleSortOrder)
      : base(provider)
    {
      Source = provider;
      TupleSortOrder = tupleSortOrder;
    }
  }
}