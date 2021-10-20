// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    20.06.2008

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Reflection;
using Xtensive.Tuples.Transform;
using Xtensive.Tuples.Transform.Internals;



namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Cuts in specified value to the <see cref="Tuple"/>.
  /// </summary>
  public sealed class CutInTransform<T> : CutInTransform
  {

    /// <see cref="MapTransform.Apply(TupleTransformType,Tuple)" copy="true" />
    public Tuple Apply(TupleTransformType transformType, Tuple source1, T source2)
    {
      return Apply(transformType, source1, Tuple.Create(source2));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"Index {Index}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="MapTransform.IsReadOnly"/> property value.</param>
    /// <param name="index">Start index.</param>
    /// <param name="source1">Source tuple descriptor.</param>
    public CutInTransform(bool isReadOnly, int index, TupleDescriptor source1)
      : base(isReadOnly, index, source1, TupleDescriptor.Create(new Type[]{typeof(T)}))
    {
    }
  }
}