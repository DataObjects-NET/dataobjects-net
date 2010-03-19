// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.19

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;

namespace Xtensive.Core.Tuples.Internals
{
  [DataContract]
  [Serializable]
  public sealed class TupleExtender : RegularTuple
  {
    public const int FirstCount = MaxGeneratedTupleLength.Value;
    [DataMember]
    public RegularTuple First;
    [DataMember]
    public RegularTuple Second;

    public override int Count
    {
      get { return descriptor.Count; }
    }

    public override Tuple CreateNew()
    {
      return new TupleExtender(descriptor, (RegularTuple) First.CreateNew(), (RegularTuple) Second.CreateNew());
    }

    public override Tuple Clone()
    {
      return new TupleExtender(this);
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return fieldIndex < FirstCount 
        ? First.GetFieldState(fieldIndex) 
        : Second.GetFieldState(fieldIndex - FirstCount);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      if (fieldIndex < FirstCount)
        First.SetFieldState(fieldIndex, fieldState);
      else
        Second.SetFieldState(fieldIndex - FirstCount, fieldState);
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      return fieldIndex < FirstCount
        ? First.GetValue(fieldIndex, out fieldState)
        : Second.GetValue(fieldIndex - FirstCount, out fieldState);
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (fieldIndex < FirstCount)
        First.SetValue(fieldIndex, fieldValue);
      else
        Second.SetValue(fieldIndex - FirstCount, fieldValue);
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(First.Descriptor.Concat(Second.Descriptor));
    }


    // Constructors

    public TupleExtender(TupleDescriptor descriptor, RegularTuple first, RegularTuple second)
      : base(descriptor)
    {
      First = first;
      Second = second;
    }

    private TupleExtender(TupleExtender template)
      : base(template.descriptor)
    {
      First = template.First;
      Second = template.Second;
    }
  }
}