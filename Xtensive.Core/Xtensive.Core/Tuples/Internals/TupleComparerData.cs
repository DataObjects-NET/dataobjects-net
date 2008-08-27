// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.29

using System;

namespace Xtensive.Core.Tuples.Internals
{
  internal struct TupleComparerData : ITupleFunctionData<int>
  {
    public Pair<int, object>[] FieldData;
    public Tuple X;
    public Tuple Y;
    public int Result;

    int ITupleFunctionData<int>.Result
    {
      get { return Result; }
    }


    // Constructors

    public TupleComparerData(Tuple x, Tuple y)
    {
      FieldData = null;
      Result = int.MinValue;
      X = x;
      Y = y;
    }
  }
}