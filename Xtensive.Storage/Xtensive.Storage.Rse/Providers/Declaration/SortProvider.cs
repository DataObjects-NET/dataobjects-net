// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public class SortProvider : CompilableProvider
  {
    public CompilableProvider Provider { get; private set; }
    public DirectionCollection<int> TupleSortOrder { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Provider.Header.TupleDescriptor, Provider.Header.RecordColumnCollection, Provider.Header.OrderInfo.KeyDescriptor, TupleSortOrder);
    }


    // Constructor

    public SortProvider(CompilableProvider provider, DirectionCollection<int> tupleSortOrder)
      : base(provider)
    {
      Provider = provider;
      TupleSortOrder = tupleSortOrder;
    }
  }
}