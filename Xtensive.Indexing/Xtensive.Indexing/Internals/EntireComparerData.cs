// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kochetov
// Created:    2008.02.07

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing
{
  internal struct EntireComparerData<TX, TY> : ITupleFunctionData<int>
    where TX : ITuple
    where TY : ITuple
  {
    public Pair<int, object>[] FieldData;
    public TX X;
    public TY Y;
    public int XCount;
    public int Result;

    int ITupleFunctionData<int>.Result
    {
      get { return Result; }
    }

    public EntireComparerData(TX x, TY y)
    {
      FieldData = null;
      Result = Int32.MinValue;
      X = x;
      Y = y;
      XCount = x.Count;
    }
  }
}