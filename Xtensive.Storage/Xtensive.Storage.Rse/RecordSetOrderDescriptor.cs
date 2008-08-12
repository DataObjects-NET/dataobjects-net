// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Describes the order of records in the <see cref="RecordSet"/>.
  /// Used by <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public struct RecordSetOrderDescriptor
  {
    private readonly DirectionCollection<int> order;
    private readonly TupleDescriptor tupleDescriptor;

    /// <summary>
    /// Gets the indexes of columns <see cref="RecordSet"/> is ordered by.
    /// </summary>
    public DirectionCollection<int> Order
    {
      [DebuggerStepThrough]
      get { return order; }
    }

    /// <summary>
    /// Gets the tuple descriptor describing 
    /// a set of <see cref="Order"/> columns.
    /// </summary>
    public TupleDescriptor TupleDescriptor
    {
      [DebuggerStepThrough]
      get { return tupleDescriptor; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="order">The <see cref="Order"/> property value.</param>
    /// <param name="keyDescriptor">The key tupleDescriptor.</param>
    public RecordSetOrderDescriptor(DirectionCollection<int> order, TupleDescriptor keyDescriptor)
    {
      this.order = order;
      tupleDescriptor = keyDescriptor;
    }
  }
}