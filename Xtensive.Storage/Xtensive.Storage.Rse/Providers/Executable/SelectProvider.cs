// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class SelectProvider : UnaryExecutableProvider
  {
    // Constructors

    public SelectProvider(CompilableProvider origin, ExecutableProvider source, int[] columnIndexes)
      : base(origin, source)
    {
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      throw new NotImplementedException();
    }
  }
}