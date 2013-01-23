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
  }

  internal sealed class ObjectFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return tuple.Objects[descriptor.Index];
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      // TODO: check type
      tuple.Objects[descriptor.Index] = value;
    }
  }

  internal sealed class BooleanFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return tuple.Flags[descriptor.Index];
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null)
        tuple.Flags[descriptor.Index] = (bool) value;
    }

    private bool GetValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var state = packedTuple.GetFieldState(fieldIndex);
      fieldState = state;
      if (!state.HasValue())
        return false;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      return packedTuple.Flags[descriptor.Index];
    }

    private bool? GetNullableValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var state = packedTuple.GetFieldState(fieldIndex);
      fieldState = state;
      if (state.IsNull())
        return null;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      return packedTuple.Flags[descriptor.Index];
    }

    private void SetValue(Tuple tuple, int fieldIndex, bool value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      packedTuple.Flags[descriptor.Index] = value;
      packedTuple.SetFieldAvailable(fieldIndex, false);
    }

    private void SetNullableValue(Tuple tuple, int fieldIndex, bool? value)
    {
      var packedTuple = (PackedTuple) tuple;
      if (value!=null) {
        var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
        packedTuple.Flags[descriptor.Index] = value.Value;
        packedTuple.SetFieldAvailable(fieldIndex, false);
      }
      else
        packedTuple.SetFieldAvailable(fieldIndex, true);
    }

    public void Initialize()
    {
      Getter = (GetValueDelegate<bool>) GetValue;
      Setter = (SetValueDelegate<bool>) SetValue;

      NullableGetter = (GetValueDelegate<bool?>) GetNullableValue;
      NullableSetter = (SetValueDelegate<bool?>) SetNullableValue;
    }
  }

  internal abstract class ValueFieldAccessor<T> : PackedFieldAccessor
    where T : struct
  {
    protected abstract long Encode(T value);

    protected abstract T Decode(long value);

    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return Decode(tuple.Values[descriptor.Index]);
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null)
        tuple.Values[descriptor.Index] = Encode((T) value);
    }

    private T GetValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var state = packedTuple.GetFieldState(fieldIndex);
      fieldState = state;
      if (!state.HasValue())
        return default (T);
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      return Decode(packedTuple.Values[descriptor.Index]);
    }

    private T? GetNullableValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var state = packedTuple.GetFieldState(fieldIndex);
      fieldState = state;
      if (state.IsNull())
        return null;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      return Decode(packedTuple.Values[descriptor.Index]);
    }

    private void SetValue(Tuple tuple, int fieldIndex, T value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      packedTuple.Values[descriptor.Index] = Encode(value);
      packedTuple.SetFieldAvailable(fieldIndex, false);
    }

    private void SetNullableValue(Tuple tuple, int fieldIndex, T? value)
    {
      var packedTuple = (PackedTuple) tuple;
      if (value!=null) {
        var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
        packedTuple.Values[descriptor.Index] = Encode(value.Value);
        packedTuple.SetFieldAvailable(fieldIndex, false);
      }
      else {
        packedTuple.SetFieldAvailable(fieldIndex, true);
      }
    }

    public void Initialize()
    {
      Getter = (GetValueDelegate<T>) GetValue;
      Setter = (SetValueDelegate<T>) SetValue;

      NullableGetter = (GetValueDelegate<T?>) GetNullableValue;
      NullableSetter = (SetValueDelegate<T?>) SetNullableValue;
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
