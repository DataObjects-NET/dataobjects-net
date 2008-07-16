// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers
{
  [Serializable]
  public class RangeProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public Range<IEntire<Tuple>> Range { get; private set; }

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