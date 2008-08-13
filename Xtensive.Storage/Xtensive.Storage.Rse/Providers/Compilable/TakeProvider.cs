// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  [Serializable]
  public sealed class TakeProvider : UnaryProvider
  {
    public Func<int> Count { get; private set; }


    // Constructor

    public TakeProvider(CompilableProvider provider, Func<int> count)
      : base(provider)
    {
      Count = count;
    }

    public TakeProvider(CompilableProvider provider, int count)
      : this(provider, () => count)
    {
    }
  }
}