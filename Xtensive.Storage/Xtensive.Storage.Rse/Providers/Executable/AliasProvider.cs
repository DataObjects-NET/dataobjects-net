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
  [Serializable]
  internal sealed class AliasProvider : UnaryExecutableProvider
  {
    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context);
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Source.GetService<T>();
    }

    protected override void Initialize()
    {
    }


    // Constructor

    public AliasProvider(CompilableProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}