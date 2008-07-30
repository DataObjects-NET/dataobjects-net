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
  public struct RecordSetOrderDescriptor
  {
    private readonly DirectionCollection<int> order;
    private readonly TupleDescriptor tupleDescriptor;

    public DirectionCollection<int> Order
    {
      [DebuggerStepThrough]
      get { return order; }
    }

    public TupleDescriptor TupleDescriptor
    {
      [DebuggerStepThrough]
      get { return tupleDescriptor; }
    }


    // Constructors

    public RecordSetOrderDescriptor(DirectionCollection<int> orderedBy, TupleDescriptor tupleDescriptor)
    {
      this.order = orderedBy;
      this.tupleDescriptor = tupleDescriptor;
    }
  }
}