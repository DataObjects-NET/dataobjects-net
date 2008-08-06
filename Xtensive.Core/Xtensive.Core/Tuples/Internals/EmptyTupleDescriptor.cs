// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.29

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tuples.Internals
{
  /// <summary>
  /// Empty tuple descriptor.
  /// </summary>
  [Serializable]
  public class EmptyTupleDescriptor: GeneratedTupleDescriptor
  {
    private readonly static EmptyTupleDescriptor instance = new EmptyTupleDescriptor();

    /// <summary>
    /// Provides the only instance of this class.
    /// </summary>
    [DebuggerHidden]
    public static EmptyTupleDescriptor Instance
    {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override bool Execute<TActionData>(ITupleActionHandler<TActionData> actionHandler, ref TActionData actionData, int fieldIndex)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }


    // Constructors

    private EmptyTupleDescriptor()
      : base(ArrayUtils<Type>.EmptyArray)
    {
    }
  }
}