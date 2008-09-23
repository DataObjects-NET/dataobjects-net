// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.15

using System;
using System.Collections;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Transform.Internals
{
  /// <summary>
  /// A tuple wrapper for <see cref="ReadOnlyTransform"/>.
  /// </summary>
  [Serializable]
  public sealed class ReadOnlyTransformTuple : WrappingTransformTupleBase
  {
    /// <inheritdoc/>
    public override ITupleTransform Transform {
      get {
        return ReadOnlyTransform.Instance;
      }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method always returns <see cref="ArrayUtils{TItem}.EmptyArray"/> of <see cref="object"/>s
    /// to block any access to the original tuple.
    /// </remarks>
    public override object[] Arguments {
      get {
        return ArrayUtils<object>.EmptyArray;
      }
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="tuple">Tuple to provide read-only wrapper for.</param>
    public ReadOnlyTransformTuple(Tuple tuple)
      : base(tuple)
    {
    }
  }
}
