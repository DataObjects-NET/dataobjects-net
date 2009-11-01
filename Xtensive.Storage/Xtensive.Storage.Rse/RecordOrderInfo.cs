// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse
{
  public struct RecordOrderInfo
  {
    private readonly DirectionCollection<int> orderedBy;
    private readonly TupleDescriptor keyDescriptor;

    public DirectionCollection<int> OrderedBy
    {
      [DebuggerStepThrough]
      get { return orderedBy; }
    }

    public TupleDescriptor KeyDescriptor
    {
      [DebuggerStepThrough]
      get { return keyDescriptor; }
    }


    // Constructors

    public RecordOrderInfo(DirectionCollection<int> orderedBy, TupleDescriptor keyDescriptor)
    {
      this.orderedBy = orderedBy;
      this.keyDescriptor = keyDescriptor;
    }
  }
}