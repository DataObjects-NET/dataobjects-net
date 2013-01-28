// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.22

using System;

namespace Xtensive.Tuples.Packed
{
  internal abstract class PackedFieldAccessor
  {
    /// <summary>
    /// Getter delegate.
    /// </summary>
    protected Delegate Getter;

    /// <summary>
    /// Setter delegate.
    /// </summary>
    protected Delegate Setter;

    /// <summary>
    /// Nullable getter delegate.
    /// </summary>
    protected Delegate NullableGetter;

    /// <summary>
    /// Nullable setter delegate.
    /// </summary>
    protected Delegate NullableSetter;

    /// <summary>
    /// Gets setter for the field.
    /// </summary>
    /// <typeparam name="T">Field type.</typeparam>
    /// <param name="isNullable">Flag indicating if field type is nullable.</param>
    /// <returns><see cref="GetValueDelegate{TValue}"/> for the field.</returns>
    public GetValueDelegate<T> GetGetter<T>(bool isNullable)
    {
      return (isNullable ? NullableGetter : Getter) as GetValueDelegate<T>;
    }

    /// <summary>
    /// Gets setter for the field.
    /// </summary>
    /// <typeparam name="T">Field type.</typeparam>
    /// <param name="isNullable">Flag indicating if field type is nullable.</param>
    /// <returns><see cref="SetValueDelegate{TValue}"/> for the field.</returns>
    public SetValueDelegate<T> GetSetter<T>(bool isNullable)
    {
      return (isNullable ? NullableSetter : Setter) as SetValueDelegate<T>;
    }

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

  internal abstract class ValueFieldAccessor : PackedFieldAccessor
  {
    public readonly int BitCount;
    public readonly long BitMask;

    private static long GetMask(int bits)
    {
      long result = 0;

      for (int i = 0; i < bits; i++) {
        result <<= 1;
        result |= 1;
      }

      return result;
    }

    protected ValueFieldAccessor(int bits)
    {
      BitCount = bits;
      BitMask = GetMask(bits);
    }
  }

  internal abstract class ValueFieldAccessor<T> : ValueFieldAccessor
    where T : struct, IEquatable<T>
  {
    protected abstract long Encode(T value);

    protected abstract T Decode(long value);

    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return Load(tuple, descriptor);
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null)
        Store(tuple, descriptor, (T) value);
    }

    public override void CopyValue(PackedTuple source, PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, PackedFieldDescriptor targetDescriptor)
    {
      Store(target, targetDescriptor, Load(source, sourceDescriptor));
    }

    public override bool ValueEquals(PackedTuple left, PackedFieldDescriptor leftDescriptor,
      PackedTuple right, PackedFieldDescriptor rightDescriptor)
    {
      var leftValue = Load(left, leftDescriptor);
      var rightValue = Load(right, rightDescriptor);
      return leftValue.Equals(rightValue);
    }

    public override int GetValueHashCode(PackedTuple tuple, PackedFieldDescriptor descriptor)
    {
      return Load(tuple, descriptor).GetHashCode();
    }

    private T GetValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      var state = packedTuple.GetFieldState(descriptor);
      fieldState = state;
      if (!state.HasValue())
        return default (T);
      return Load(packedTuple, descriptor);
    }

    private T? GetNullableValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      var state = packedTuple.GetFieldState(descriptor);
      fieldState = state;
      if (!state.HasValue())
        return null;
      return Load(packedTuple, descriptor);
    }

    private void SetValue(Tuple tuple, int fieldIndex, T value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      Store(packedTuple, descriptor, value);
      packedTuple.SetFieldState(descriptor, TupleFieldState.Available);
    }

    private void SetNullableValue(Tuple tuple, int fieldIndex, T? value)
    {
      var packedTuple = (PackedTuple) tuple;
      var descriptor = packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
      if (value!=null) {
        Store(packedTuple, descriptor, value.Value);
        packedTuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else {
        packedTuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
      }
    }

    private void Store(PackedTuple tuple, PackedFieldDescriptor d, T value)
    {
      var encoded = Encode(value);
      var block = tuple.Values[d.ValueIndex];
      tuple.Values[d.ValueIndex] = (block & ~(d.ValueBitMask << d.ValueBitOffset)) | (encoded << d.ValueBitOffset);
    }

    private T Load(PackedTuple tuple, PackedFieldDescriptor d)
    {
      var encoded = (tuple.Values[d.ValueIndex] >> d.ValueBitOffset) & d.ValueBitMask;
      return Decode(encoded);
    }

    protected ValueFieldAccessor(int bits)
      : base(bits)
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

    public BooleanFieldAccessor()
      : base(1)
    {
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

    public FloatFieldAccessor()
      : base(sizeof(float) * 8)
    {
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

    public DoubleFieldAccessor()
      : base(sizeof(double) * 8)
    {
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

    public TimeSpanFieldAccessor()
      : base(sizeof(long) * 8)
    {
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

    public DateTimeFieldAccessor()
      : base(sizeof(long) * 8)
    {
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

    public ByteFieldAccessor()
      : base(sizeof(byte) * 8)
    {
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

    public SByteFieldAccessor()
      : base(sizeof(sbyte) * 8)
    {
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

    public ShortFieldAccessor()
      : base(sizeof(short) * 8)
    {
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

    public UShortFieldAccessor()
      : base(sizeof(ushort) * 8)
    {
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

    public IntFieldAccessor()
      : base(sizeof(int) * 8)
    {
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

    public UIntFieldAccessor()
      : base(sizeof(uint) * 8)
    {
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

    public LongFieldAccessor()
      : base(sizeof(long) * 4)
    {
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

    public ULongFieldAccessor()
      : base(sizeof(ulong) * 4)
    {
    }
  }
}
