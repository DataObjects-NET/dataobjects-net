// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class IntersectProvider : BinaryExecutableProvider<Compilable.IntersectProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      return left.Intersect(right);
    }


    // Constructors

    public IntersectProvider(Compilable.IntersectProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
    }
  }
}