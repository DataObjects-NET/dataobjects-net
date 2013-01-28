// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal sealed class PackedTuple : RegularTuple
  {
    public readonly TupleDescriptor PackedDescriptor;
    public readonly long[] Values;
    public readonly object[] Objects;

    public override TupleDescriptor Descriptor
    {
      get { return PackedDescriptor; }
    }

    public override Tuple Clone()
    {
      return new PackedTuple(this);
    }

    public override Tuple CreateNew()
    {
      return new PackedTuple(PackedDescriptor);
    }

    public override bool Equals(Tuple other)
    {
      var packedOther = other as PackedTuple;
      if (packedOther==null)
        return base.Equals(other);

      if (ReferenceEquals(packedOther, this))
        return true;
      if (Descriptor!=packedOther.Descriptor)
        return false;

      var count = Count;
      for (int i = 0; i < count; i++) {
        var descriptor = PackedDescriptor.FieldDescriptors[i];
        var thisState = GetFieldState(descriptor);
        var otherState = packedOther.GetFieldState(descriptor);
        if (thisState!=otherState)
          return false;
        if (thisState!=TupleFieldState.Available)
          continue;
        if (!descriptor.Accessor.ValueEquals(this, descriptor, packedOther, descriptor))
          return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      var count = Count;
      int result = 0;
      for (int i = 0; i < count; i++) {
        var descriptor = PackedDescriptor.FieldDescriptors[i];
        var state = GetFieldState(descriptor);
        var fieldHash = state==TupleFieldState.Available
          ? descriptor.Accessor.GetValueHashCode(this, descriptor)
          : 0;
        result = HashCodeMultiplier * result ^ fieldHash;
      }
      return result;
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return GetFieldState(PackedDescriptor.FieldDescriptors[fieldIndex]);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      if (fieldState==TupleFieldState.Null)
        throw new ArgumentOutOfRangeException("fieldState");

      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      SetFieldState(descriptor, fieldState);

      if (fieldState!=TupleFieldState.Available && descriptor.PackingType==FieldPackingType.Object)
        Objects[descriptor.ValueIndex] = null;
    }

    public void SetFieldState(PackedFieldDescriptor d, TupleFieldState fieldState)
    {
      var bits = (long) fieldState;
      var block = Values[d.StateIndex];
      Values[d.StateIndex] = (block & ~(3L << d.StateBitOffset)) | (bits << d.StateBitOffset);
    }

    public TupleFieldState GetFieldState(PackedFieldDescriptor d)
    {
      var block = Values[d.StateIndex];
      return (TupleFieldState) ((block >> d.StateBitOffset) & 3);
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      var state = GetFieldState(descriptor);
      fieldState = state;
      if (state==TupleFieldState.Available)
        return descriptor.Accessor.GetUntypedValue(this, descriptor);
      return null;
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      descriptor.Accessor.SetUntypedValue(this, descriptor, fieldValue);
      if (fieldValue==null)
        SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
      else
        SetFieldState(descriptor, TupleFieldState.Available);
    }

    protected internal override TupleFieldAccessor GetFieldAccessor(int fieldIndex)
    {
      return PackedDescriptor.FieldDescriptors[fieldIndex].Accessor;
    }

    public PackedTuple(TupleDescriptor descriptor)
    {
      PackedDescriptor = descriptor;

      Values = new long[PackedDescriptor.ValuesLength];
      Objects = new object[PackedDescriptor.ObjectsLength];
    }

    private PackedTuple(PackedTuple origin)
    {
      PackedDescriptor = origin.PackedDescriptor;

      Values = (long[]) origin.Values.Clone();
      Objects = (object[]) origin.Objects.Clone();
    }
  }
}