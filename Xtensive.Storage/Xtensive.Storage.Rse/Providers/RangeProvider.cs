// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using Xtensive.Core.Tuples;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers
{
  public class RangeProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public Range<IEntire<Tuple>> Range { get; private set; }

    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Indexed | ProviderOptions.Ordered | ProviderOptions.FastCount; }
    }

    protected override RecordHeader BuildHeader()
    {
      return Source.Header; 
    }

    // Constructor

    public RangeProvider(CompilableProvider provider, Range<IEntire<Tuple>> range)
      : base(provider)
    {
      Source = provider;
      Range = range;
    }
  }
}