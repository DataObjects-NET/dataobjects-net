// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class RawProvider : ExecutableProvider,
    ISupportRandomAccess<Tuple>
  {
    private readonly Tuple[] tuples;


    /// <inheritdoc/>
    public Tuple this[int index]
    {
      get { return tuples[index]; }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return tuples;
    }


    // Constructors

    public RawProvider(CompilableProvider origin, params Tuple[] tuples)
      : base(origin)
    {
      this.tuples = tuples;
    }
  }
}