// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections;

namespace Xtensive.Tuples.Packed
{
  public sealed class PackedTuple : RegularTuple
  {
    private new readonly PackedTupleDescriptor descriptor;
    private readonly BitArray bits;

    private readonly long[] values;
    private readonly object[] objects;

    public override TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }

    public override Tuple Clone()
    {
      return new PackedTuple(this);
    }

    public override Tuple CreateNew()
    {
      return new PackedTuple(descriptor);
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var result = TupleFieldState.Default;
      if (bits[2 * fieldIndex])
        result |= TupleFieldState.Available;
      if (bits[2 * fieldIndex + 1])
        result |= TupleFieldState.Null;
      return result;
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      bits[2 * fieldIndex] = (fieldState & TupleFieldState.Available)==TupleFieldState.Available;
      bits[2 * fieldIndex + 1] = (fieldState & TupleFieldState.Null)==TupleFieldState.Null;

      if (fieldState.HasValue())
        return;

      var fieldDescriptor = descriptor.FieldDescriptors[fieldIndex];
      if (fieldDescriptor.IsObject)
        objects[fieldDescriptor.ObjectIndex] = null;
      else
        values[fieldDescriptor.ValueIndex] = 0L;
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var state = GetFieldState(fieldIndex);
      fieldState = state;
      if (!state.HasValue())
        return null;
      var fieldDescriptor = descriptor.FieldDescriptors[fieldIndex];
      if (fieldDescriptor.IsObject)
        return objects[fieldDescriptor.ObjectIndex];
      return fieldDescriptor.Boxer.Invoke(values[fieldDescriptor.ValueIndex]);
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      bits[2 * fieldIndex] = true;
      bits[2 * fieldIndex + 1] = fieldValue==null;

      var fieldDescriptor = descriptor.FieldDescriptors[fieldIndex];
      if (fieldDescriptor.IsObject)
        objects[fieldDescriptor.ObjectIndex] = fieldValue;
      else {
        values[fieldDescriptor.ValueIndex] = fieldValue!=null
          ? fieldDescriptor.Unboxer.Invoke(fieldValue)
          : 0L;
      }
    }

    internal long GetPackedValue(int fieldIndex)
    {
      var valueIndex = descriptor.FieldDescriptors[fieldIndex].ValueIndex;
      return values[valueIndex];
    }

    internal void SetPackedValue(int fieldIndex, long value)
    {
      var valueIndex = descriptor.FieldDescriptors[fieldIndex].ValueIndex;
      bits[2 * fieldIndex] = true;
      bits[2 * fieldIndex + 1] = false;
      values[valueIndex] = value;
    }

    protected override Delegate GetGetValueDelegate(int fieldIndex)
    {
      return descriptor.FieldDescriptors[fieldIndex].GetValueDelegate;
    }

    protected override Delegate GetSetValueDelegate(int fieldIndex)
    {
      return descriptor.FieldDescriptors[fieldIndex].SetValueDelegate;
    }

    protected override Delegate GetGetNullableValueDelegate(int fieldIndex)
    {
      return descriptor.FieldDescriptors[fieldIndex].GetNullableValueDelegate;
    }

    protected override Delegate GetSetNullableValueDelegate(int fieldIndex)
    {
      return descriptor.FieldDescriptors[fieldIndex].SetNullableValueDelegate;
    }

    public PackedTuple(TupleDescriptor descriptorObj)
      : base(descriptorObj)
    {
      descriptor = (PackedTupleDescriptor) descriptorObj;

      bits = new BitArray(descriptor.BitsLength);
      values = new long[descriptor.ValuesLength];
      objects = new object[descriptor.ObjectsLength];
    }

    private PackedTuple(PackedTuple origin)
      : base(origin.descriptor)
    {
      descriptor = origin.descriptor;

      bits = (BitArray) origin.bits.Clone();
      values = (long[]) origin.values.Clone();
      objects = (object[]) origin.objects.Clone();
    }
  }
}