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
    private static readonly object[] EmptyObjectArray = new object[0];

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

      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      var count = Count;
      for (int i = 0; i < count; i++) {
        var thisState = GetFieldState(ref fieldDescriptors[i]);
        var otherState = packedOther.GetFieldState(ref fieldDescriptors[i]);
        if (thisState!=otherState)
          return false;
        if (thisState!=TupleFieldState.Available)
          continue;
        var accessor = fieldDescriptors[i].Accessor;
        if (!accessor.ValueEquals(this, ref fieldDescriptors[i], packedOther, ref fieldDescriptors[i]))
          return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      var count = Count;
      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      int result = 0;
      for (int i = 0; i < count; i++) {
        var accessor = fieldDescriptors[i].Accessor;
        var state = GetFieldState(ref fieldDescriptors[i]);
        var fieldHash = state==TupleFieldState.Available
          ? accessor.GetValueHashCode(this, ref fieldDescriptors[i])
          : 0;
        result = HashCodeMultiplier * result ^ fieldHash;
      }
      return result;
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return GetFieldState(ref PackedDescriptor.FieldDescriptors[fieldIndex]);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      if (fieldState==TupleFieldState.Null)
        throw new ArgumentOutOfRangeException("fieldState");

      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      SetFieldState(ref fieldDescriptors[fieldIndex], fieldState);
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      return fieldDescriptors[fieldIndex].Accessor.GetUntypedValue(this, ref fieldDescriptors[fieldIndex], out fieldState);
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      fieldDescriptors[fieldIndex].Accessor.SetUntypedValue(this, ref fieldDescriptors[fieldIndex], fieldValue);
    }

    public void SetFieldState(ref PackedFieldDescriptor d, TupleFieldState fieldState)
    {
      var bits = (long) fieldState;
      var block = Values[d.StateIndex];
      Values[d.StateIndex] = (block & ~(3L << d.StateBitOffset)) | (bits << d.StateBitOffset);

      if (fieldState!=TupleFieldState.Available && d.PackingType==FieldPackingType.Object) {
        Objects[d.ValueIndex] = null;
      }
    }

    public TupleFieldState GetFieldState(ref PackedFieldDescriptor d)
    {
      var block = Values[d.StateIndex];
      return (TupleFieldState) ((block >> d.StateBitOffset) & 3);
    }

    public PackedTuple(TupleDescriptor descriptor)
    {
      PackedDescriptor = descriptor;

      Values = new long[PackedDescriptor.ValuesLength];
      Objects = PackedDescriptor.ObjectsLength > 0
        ? new object[PackedDescriptor.ObjectsLength]
        : EmptyObjectArray;
    }

    private PackedTuple(PackedTuple origin)
    {
      PackedDescriptor = origin.PackedDescriptor;

      Values = (long[]) origin.Values.Clone();
      Objects = PackedDescriptor.ObjectsLength > 0
        ? (object[]) origin.Objects.Clone()
        : EmptyObjectArray;
    }
  }
}