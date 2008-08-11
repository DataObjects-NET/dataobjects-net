// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  [Serializable]
  public sealed class SkipProvider : UnaryProvider
  {
    public int Count { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header;
    }

    protected override void Initialize()
    {}


    // Constructors

    public SkipProvider(CompilableProvider provider, int count)
      : base(provider)
    {
      Count = count;
    }
  }
}