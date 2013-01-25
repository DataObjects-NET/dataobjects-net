// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.22

using System;

namespace Xtensive.Tuples.Packed
{
  internal abstract class PackedFieldAccessor : TupleFieldAccessor
  {
    public abstract object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor);

    public abstract void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value);

    public abstract void CopyValue(PackedTuple source, PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, PackedFieldDescriptor targetDescriptor);

    public abstract bool ValueEquals(PackedTuple left, PackedFieldDescriptor leftDescriptor,
      PackedTuple right, PackedFieldDescriptor rightDescriptor);

    public abstract int GetValueHashCode(PackedTuple tuple, PackedFieldDescriptor descriptor);
  }

  internal sealed class ObjectFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return tuple.Objects[descriptor.ValueIndex];
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      tuple.Objects[descriptor.ValueIndex] = value;
    }

    public override void CopyValue(PackedTuple source, PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, PackedFieldDescriptor targetDescriptor)
    {
      target.Objects[targetDescriptor.ValueIndex] = source.Objects[sourceDescriptor.ValueIndex];
    }

    public override bool ValueEquals(PackedTuple left, PackedFieldDescriptor leftDescriptor,
      PackedTuple right, PackedFieldDescriptor rightDescriptor)
    {
      var leftValue = left.Objects[leftDescriptor.ValueIndex];
      var rightValue = right.Objects[rightDescriptor.ValueIndex];
      return leftValue.Equals(rightValue);
    }

    public override int GetValueHashCode(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return tuple.Objects[descriptor.ValueIndex].GetHashCode();
    }
  }

  internal abstract class ValueFieldAccessor<T> : PackedFieldAccessor
    where T : struct, IEquatable<T>
  {
    protected abstract long Encode(T value);

    protected abstract T Decode(long value);

    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return Decode(tuple.Values[descriptor.ValueIndex]);
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null)
        tuple.Values[descriptor.ValueIndex] = Encode((T) value);
    }

    public override void CopyValue(PackedTuple source, PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, PackedFieldDescriptor targetDescriptor)
    {
      target.Values[targetDescriptor.ValueIndex] = source.Values[sourceDescriptor.ValueIndex];
    }

    public override bool ValueEquals(PackedTuple left, PackedFieldDescriptor leftDescriptor,
      PackedTuple right, PackedFieldDescriptor rightDescriptor)
    {
      var leftValue = Decode(left.Values[leftDescriptor.ValueIndex]);
      var rightValue = Decode(right.Values[rightDescriptor.ValueIndex]);
      return leftValue.Equals(rightValue);
    }

    public override int GetValueHashCode(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      var value = Decode(tuple.Values[descriptor.ValueIndex]);
      return value.GetHashCode();
    }

    private T GetValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      var state = packedTuple.GetFieldState(descriptor);
      fieldState = state;
      if (!state.HasValue())
        return default (T);
      return Decode(packedTuple.Values[descriptor.ValueIndex]);
    }

    private T? GetNullableValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      var state = packedTuple.GetFieldState(descriptor);
      fieldState = state;
      if (state.IsNull())
        return null;
      return Decode(packedTuple.Values[descriptor.ValueIndex]);
    }

    private void SetValue(Tuple tuple, int fieldIndex, T value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      packedTuple.Values[descriptor.ValueIndex] = Encode(value);
      packedTuple.SetFieldState(descriptor, TupleFieldState.Available);
    }

    private void SetNullableValue(Tuple tuple, int fieldIndex, T? value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      if (value!=null) {
        packedTuple.Values[descriptor.ValueIndex] = Encode(value.Value);
        packedTuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else {
        packedTuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
      }
    }

    protected ValueFieldAccessor()
    {
      Getter = (GetValueDelegate<T>) GetValue;
      Setter = (SetValueDelegate<T>) SetValue;

      NullableGetter = (GetValueDelegate<T?>) GetNullableValue;
      NullableSetter = (SetValueDelegate<T?>) SetNullableValue;
    }
  }

  internal sealed class BooleanFieldAccessor : ValueFieldAccessor<bool>
  {
    protected override long Encode(bool value)
    {
      return value ? 1L : 0L;
    }

    protected override bool Decode(long value)
    {
      return value!=0;
    }
  }

  internal sealed class FloatFieldAccessor : ValueFieldAccessor<float>
  {
    protected override long Encode(float value)
    {
      return BitConverter.DoubleToInt64Bits(value);
    }

    protected override float Decode(long value)
    {
      return (float) BitConverter.Int64BitsToDouble(value);
    }
  }

  internal sealed class DoubleFieldAccessor : ValueFieldAccessor<double>
  {
    protected override long Encode(double value)
    {
      return BitConverter.DoubleToInt64Bits(value);
    }

    protected override double Decode(long value)
    {
      return BitConverter.Int64BitsToDouble(value);
    }
  }

  internal sealed class TimeSpanFieldAccessor : ValueFieldAccessor<TimeSpan>
  {
    protected override long Encode(TimeSpan value)
    {
      return value.Ticks;
    }

    protected override TimeSpan Decode(long value)
    {
      return TimeSpan.FromTicks(value);
    }
  }

  internal sealed class DateTimeFieldAccessor : ValueFieldAccessor<DateTime>
  {
    protected override long Encode(DateTime value)
    {
      return value.ToBinary();
    }

    protected override DateTime Decode(long value)
    {
      return DateTime.FromBinary(value);
    }
  }

  internal sealed class ByteFieldAccessor : ValueFieldAccessor<byte>
  {
    protected override long Encode(byte value)
    {
      return value;
    }

    protected override byte Decode(long value)
    {
      return unchecked ((byte) value);
    }
  }

  internal sealed class SByteFieldAccessor : ValueFieldAccessor<sbyte>
  {
    protected override long Encode(sbyte value)
    {
      return value;
    }

    protected override sbyte Decode(long value)
    {
      return unchecked ((sbyte) value);
    }
  }
  
  internal sealed class ShortFieldAccessor : ValueFieldAccessor<short>
  {
    protected override long Encode(short value)
    {
      return value;
    }

    protected override short Decode(long value)
    {
      return unchecked ((short) value);
    }
  }
  
  internal sealed class UShortFieldAccessor : ValueFieldAccessor<ushort>
  {
    protected override long Encode(ushort value)
    {
      return value;
    }

    protected override ushort Decode(long value)
    {
      return unchecked ((ushort) value);
    }
  }
  
  internal sealed class IntFieldAccessor : ValueFieldAccessor<int>
  {
    protected override long Encode(int value)
    {
      return value;
    }

    protected override int Decode(long value)
    {
      return unchecked ((int) value);
    }
  }
  
  internal sealed class UIntFieldAccessor : ValueFieldAccessor<uint>
  {
    protected override long Encode(uint value)
    {
      return value;
    }

    protected override uint Decode(long value)
    {
      return unchecked ((uint) value);
    }
  }
  
  internal sealed class LongFieldAccessor : ValueFieldAccessor<long>
  {
    protected override long Encode(long value)
    {
      return value;
    }

    protected override long Decode(long value)
    {
      return value;
    }
  }
  
  internal sealed class ULongFieldAccessor : ValueFieldAccessor<ulong>
  {
    protected override long Encode(ulong value)
    {
      return unchecked((long) value);
    }

    protected override ulong Decode(long value)
    {
      return unchecked((ulong) value);
    }
  }
}
