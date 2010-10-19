// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.29

using System;
using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Tuples.Internals
{
  /// <summary>
  /// Empty tuple descriptor.
  /// </summary>
  [Serializable]
  public sealed class EmptyTupleDescriptor: TupleDescriptor
  {
    private readonly static EmptyTupleDescriptor instance;

    /// <summary>
    /// Provides the only instance of this class.
    /// </summary>
    public static EmptyTupleDescriptor Instance
    {
      [DebuggerStepThrough]
      get { return instance; }
    }


    // Constructors

    private EmptyTupleDescriptor()
      : base(ArrayUtils<Type>.EmptyArray)
    {
    }

    static EmptyTupleDescriptor()
    {
      instance = new EmptyTupleDescriptor();
      instance.Initialize(EmptyTuple.Instance);
    }
  }
}