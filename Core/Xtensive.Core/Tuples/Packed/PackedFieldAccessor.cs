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

    public void SetValue<T>(PackedTuple tuple, PackedFieldDescriptor descriptor, bool isNullable, T value)
    {
      var setter = (isNullable ? NullableSetter : Setter) as SetValueDelegate<T>;
      if (setter!=null)
        setter.Invoke(tuple, descriptor, value);
      else
        SetUntypedValue(tuple, descriptor, value);
    }

    public T GetValue<T>(PackedTuple tuple, PackedFieldDescriptor descriptor, bool isNullable, out TupleFieldState fieldState)
    {
      var getter = (isNullable ? NullableGetter : Getter) as GetValueDelegate<T>;
      if (getter!=null)
        return getter.Invoke(tuple, descriptor, out fieldState);
      return (T) GetUntypedValue(tuple, descriptor, out fieldState);
    }

    public abstract object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, out TupleFieldState fieldState);

    public abstract void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value);

    public abstract void CopyValue(PackedTuple source, PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, PackedFieldDescriptor targetDescriptor);

    public abstract bool ValueEquals(PackedTuple left, PackedFieldDescriptor leftDescriptor,
      PackedTuple right, PackedFieldDescriptor rightDescriptor);

    public abstract int GetValueHashCode(PackedTuple tuple, PackedFieldDescriptor descriptor);
  }

  internal sealed class ObjectFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? tuple.Objects[descriptor.ValueIndex] : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      tuple.Objects[descriptor.ValueIndex] = value;
      if (value!=null)
        tuple.SetFieldState(descriptor, TupleFieldState.Available);
      else
        tuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
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
    private static readonly T DefaultValue = default(T);
    private static readonly T? NullValue = null;

    protected abstract long Encode(T value);

    protected abstract T Decode(long value);

    public override object GetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? (object) Load(tuple, descriptor) : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null) {
        Store(tuple, descriptor, (T) value);
        tuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else
        tuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
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

    private T GetValue(PackedTuple tuple, PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? Load(tuple, descriptor) : DefaultValue;
    }

    private T? GetNullableValue(PackedTuple tuple, PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? Load(tuple, descriptor) : NullValue;
    }

    private void SetValue(PackedTuple tuple, PackedFieldDescriptor descriptor, T value)
    {
      Store(tuple, descriptor, value);
      tuple.SetFieldState(descriptor, TupleFieldState.Available);
    }

    private void SetNullableValue(PackedTuple tuple, PackedFieldDescriptor descriptor, T? value)
    {
      if (value!=null) {
        Store(tuple, descriptor, value.Value);
        tuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else
        tuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
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
