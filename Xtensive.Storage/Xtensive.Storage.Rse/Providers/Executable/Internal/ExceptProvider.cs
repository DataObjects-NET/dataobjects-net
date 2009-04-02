// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ExceptProvider : BinaryExecutableProvider<Compilable.ExceptProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      return left.Except(right);
    }


    // Constructor

    public ExceptProvider(Compilable.ExceptProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
    }
  }
}