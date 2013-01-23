// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections;

namespace Xtensive.Tuples.Packed
{
  internal sealed class PackedTuple : RegularTuple
  {
    internal readonly PackedTupleDescriptor PackedDescriptor;
    internal readonly BitArray Flags;
    internal readonly long[] Values;
    internal readonly object[] Objects;

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

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var result = TupleFieldState.Default;
      if (Flags[GetAvailableIndex(fieldIndex)])
        result |= TupleFieldState.Available;
      if (Flags[GetNullIndex(fieldIndex)])
        result |= TupleFieldState.Null;
      return result;
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      Flags[GetAvailableIndex(fieldIndex)] = (fieldState & TupleFieldState.Available)==TupleFieldState.Available;
      Flags[GetNullIndex(fieldIndex)] = (fieldState & TupleFieldState.Null)==TupleFieldState.Null;

      if (fieldState.HasValue())
        return;

      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      if (descriptor.PackingType==FieldPackingType.Object)
        Objects[descriptor.Index] = null;
    }

    internal void SetFieldAvailable(int fieldIndex, bool isNullable)
    {
      Flags[GetAvailableIndex(fieldIndex)] = true;
      Flags[GetNullIndex(fieldIndex)] = isNullable;
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var state = GetFieldState(fieldIndex);
      fieldState = state;
      if (!state.HasValue())
        return null;
      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      return descriptor.Accessor.GetUntypedValue(this, descriptor);
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      var descriptor = PackedDescriptor.FieldDescriptors[fieldIndex];
      descriptor.Accessor.SetUntypedValue(this, descriptor, fieldValue);
      SetFieldAvailable(fieldIndex, fieldValue==null);
    }

    protected internal override TupleFieldAccessor GetFieldAccessor(int fieldIndex)
    {
      return PackedDescriptor.FieldDescriptors[fieldIndex].Accessor;
    }

    private static int GetAvailableIndex(int fieldIndex)
    {
      return fieldIndex * 2;
    }

    private static int GetNullIndex(int fieldIndex)
    {
      return fieldIndex * 2 + 1;
    }

    public PackedTuple(PackedTupleDescriptor descriptor)
    {
      PackedDescriptor = descriptor;

      Flags = new BitArray(PackedDescriptor.FlagsLength);
      Values = new long[PackedDescriptor.ValuesLength];
      Objects = new object[PackedDescriptor.ObjectsLength];
    }

    private PackedTuple(PackedTuple origin)
    {
      PackedDescriptor = origin.PackedDescriptor;

      Flags = (BitArray) origin.Flags.Clone();
      Values = (long[]) origin.Values.Clone();
      Objects = (object[]) origin.Objects.Clone();
    }
  }
}