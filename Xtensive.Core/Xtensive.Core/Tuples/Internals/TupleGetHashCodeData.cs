// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

namespace Xtensive.Core.Tuples.Internals
{
  internal struct TupleGetHashCodeData: ITupleFunctionData<int>
  {
    public object[] FieldData;
    public ITuple Tuple;
    public int Result;

    int ITupleFunctionData<int>.Result
    {
      get { return Result; }
    }


    // Constructors

    public TupleGetHashCodeData(ITuple tuple)
    {
      FieldData = null;
      Tuple = tuple;
      Result = 0;
    }
  }
}