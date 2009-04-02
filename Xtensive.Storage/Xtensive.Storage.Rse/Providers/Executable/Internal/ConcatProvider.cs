// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ConcatProvider : BinaryExecutableProvider<Compilable.ConcatProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      return left.Concat(right);
    }


    // Constructor

    public ConcatProvider(Compilable.ConcatProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
    }
  }
}