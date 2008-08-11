// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public sealed class TakeProvider  : UnaryExecutableProvider
  {
    public int Count { get; private set; }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Take(Count);
    }

    protected override void Initialize()
    {}


    // Constructor

    public TakeProvider(Provider origin, ExecutableProvider source, int count)
      : base(origin, source)
    {
      Count = count;
    }
  }
}