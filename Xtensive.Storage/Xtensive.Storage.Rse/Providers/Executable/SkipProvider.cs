// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public sealed class SkipProvider : UnaryExecutableProvider
  {
    public int Count { get; private set; }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Skip(Count);
    }

    protected override void Initialize()
    { }


    // Constructor

    public SkipProvider(Provider origin, ExecutableProvider source, int count)
      : base(origin, source)
    {
      Count = count;
    }
  }
}