// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.28

using System;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Storage.Internals
{
  public class GuidLongGuidKeyProvider : DefaultGenerator
  {
    private long currentLongValue = 1;

    public override Tuple Next()
    {
      Tuple tuple = Tuple.Create(Hierarchy.TupleDescriptor);
      tuple.SetValue(0, Guid.NewGuid());
      tuple.SetValue(1, currentLongValue++);
      tuple.SetValue(2, Guid.NewGuid());
      return tuple;
    }
  }
}