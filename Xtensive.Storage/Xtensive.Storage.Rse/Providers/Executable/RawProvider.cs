// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class RawProvider : ExecutableProvider,
    IListProvider
  {
    private readonly Tuple[] tuples;


    public long Count
    {
      get { return tuples.Length; }
    }

    public Tuple GetItem(int index)
    {
      return tuples[index];
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return tuples;
    }

    protected override void Initialize()
    {
    }


    // Constructors

    public RawProvider(Provider origin, params Tuple[] tuples)
      : base(origin)
    {
      AddService<IListProvider>();
      this.tuples = tuples;
    }
  }
}