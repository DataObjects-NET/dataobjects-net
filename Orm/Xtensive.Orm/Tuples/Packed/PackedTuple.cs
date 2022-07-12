// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      if (!(other is PackedTuple packedOther)) {
        return base.Equals(other);
      }

      if (ReferenceEquals(packedOther, this)) {
        return true;
      }
      if (Descriptor != packedOther.Descriptor) {
        return false;
      }

      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      var count = Count;
      for (int i = 0; i < count; i++) {
        ref readonly var descriptor = ref fieldDescriptors[i];
        var thisState = GetFieldState(descriptor);
        var otherState = packedOther.GetFieldState(descriptor);
        if (thisState != otherState) {
          return false;
        }
        if (thisState == TupleFieldState.Available &&
            !descriptor.GetAccessor().ValueEquals(this, descriptor, packedOther, descriptor)) {
          return false;
        }
      }

      return true;
    }

    public override int GetHashCode()
    {
      var count = Count;
      var fieldDescriptors = PackedDescriptor.FieldDescriptors;
      int result = 0;
      for (int i = 0; i < count; i++) {
        ref readonly var descriptor = ref fieldDescriptors[i];
        var state = GetFieldState(descriptor);
        var fieldHash = state == TupleFieldState.Available
          ? descriptor.GetAccessor().GetValueHashCode(this, descriptor)
          : 0;
        result = HashCodeMultiplier * result ^ fieldHash;
      }
      return result;
    }

    public override TupleFieldState GetFieldState(int fieldIndex) =>
      GetFieldState(PackedDescriptor.FieldDescriptors[fieldIndex]);

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      if (fieldState == TupleFieldState.Null) {
        throw new ArgumentOutOfRangeException(nameof(fieldState));
      }

      SetFieldState(PackedDescriptor.FieldDescriptors[fieldIndex], fieldState);
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      ref readonly var descriptor = ref PackedDescriptor.FieldDescriptors[fieldIndex];
      return descriptor.GetAccessor().GetUntypedValue(this, descriptor, out fieldState);
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      ref readonly var descriptor = ref PackedDescriptor.FieldDescriptors[fieldIndex];
      descriptor.GetAccessor().SetUntypedValue(this, descriptor, fieldValue);
    }

    public void SetFieldState(in PackedFieldDescriptor d, TupleFieldState fieldState)
    {
      var bits = (long) fieldState;
      ref var block = ref Values[d.GetStateIndex()];
      var stateBitOffset = d.GetStateBitOffset();
      block = (block & ~(3L << stateBitOffset)) | (bits << stateBitOffset);

      if (fieldState != TupleFieldState.Available && d.IsObjectField()) {
        Objects[d.GetObjectIndex()] = null;
      }
    }

    public TupleFieldState GetFieldState(in PackedFieldDescriptor d)
    {
      int stateIndex = d.GetStateIndex(), stateBitOffset = d.GetStateBitOffset();
      return (TupleFieldState) ((Values[stateIndex] >> stateBitOffset) & 3);
    }

    public PackedTuple(in TupleDescriptor descriptor)
    {
      PackedDescriptor = descriptor;

      Values = new long[PackedDescriptor.ValuesLength];
      Objects = PackedDescriptor.ObjectsLength > 0
        ? new object[PackedDescriptor.ObjectsLength]
        : Array.Empty<object>();
    }

    private PackedTuple(PackedTuple origin)
    {
      PackedDescriptor = origin.PackedDescriptor;

      Values = (long[]) origin.Values.Clone();
      Objects = PackedDescriptor.ObjectsLength > 0
        ? (object[]) origin.Objects.Clone()
        : Array.Empty<object>();
    }
  }
}