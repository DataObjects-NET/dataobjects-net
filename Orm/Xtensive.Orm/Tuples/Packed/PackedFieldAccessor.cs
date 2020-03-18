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

    public void SetValue<T>(PackedTuple tuple, ref PackedFieldDescriptor descriptor, bool isNullable, T value)
    {
      var setter = (isNullable ? NullableSetter : Setter) as SetValueDelegate<T>;
      if (setter!=null)
        setter.Invoke(tuple, ref descriptor, value);
      else
        SetUntypedValue(tuple, ref descriptor, value);
    }

    public T GetValue<T>(PackedTuple tuple, ref PackedFieldDescriptor descriptor, bool isNullable, out TupleFieldState fieldState)
    {
      var getter = (isNullable ? NullableGetter : Getter) as GetValueDelegate<T>;
      if (getter!=null)
        return getter.Invoke(tuple, ref descriptor, out fieldState);
      var targetType = typeof (T);

      //Dirty hack of nullable enum reading
      if (isNullable)
        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
      if (targetType.IsEnum)
        return (T) Enum.ToObject(targetType, GetUntypedValue(tuple, ref descriptor, out fieldState));
      return (T) GetUntypedValue(tuple, ref descriptor, out fieldState);
    }

    public abstract object GetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState);

    public abstract void SetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, object value);

    public abstract void CopyValue(PackedTuple source, ref PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, ref PackedFieldDescriptor targetDescriptor);

    public abstract bool ValueEquals(PackedTuple left, ref PackedFieldDescriptor leftDescriptor,
      PackedTuple right, ref PackedFieldDescriptor rightDescriptor);

    public abstract int GetValueHashCode(PackedTuple tuple, ref PackedFieldDescriptor descriptor);
  }

  internal sealed class ObjectFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(ref descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? tuple.Objects[descriptor.ValueIndex] : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, object value)
    {
      tuple.Objects[descriptor.ValueIndex] = value;
      if (value!=null)
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available);
      else
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available | TupleFieldState.Null);
    }

    public override void CopyValue(PackedTuple source, ref PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, ref PackedFieldDescriptor targetDescriptor)
    {
      target.Objects[targetDescriptor.ValueIndex] = source.Objects[sourceDescriptor.ValueIndex];
    }

    public override bool ValueEquals(PackedTuple left, ref PackedFieldDescriptor leftDescriptor,
      PackedTuple right, ref PackedFieldDescriptor rightDescriptor)
    {
      var leftValue = left.Objects[leftDescriptor.ValueIndex];
      var rightValue = right.Objects[rightDescriptor.ValueIndex];
      return leftValue.Equals(rightValue);
    }

    public override int GetValueHashCode(PackedTuple tuple, ref PackedFieldDescriptor descriptor)
    {
      return tuple.Objects[descriptor.ValueIndex].GetHashCode();
    }
  }

  internal abstract class ValueFieldAccessor : PackedFieldAccessor
  {
    public readonly int Rank;

    public Type FieldType { get; protected set; }

    private static int GetRank(int bitSize)
    {
      var rank = 0;
      while ((bitSize >>= 1) > 0) {
        rank++;
      }

      return rank;
    }

    protected ValueFieldAccessor(int bitCount)
    {
      Rank = GetRank(bitCount);
    }
  }

  internal abstract class ValueFieldAccessor<T> : ValueFieldAccessor
    where T : struct, IEquatable<T>
  {
    private static readonly T DefaultValue = default;
    private static readonly T? NullValue = null;

    protected virtual long Encode(T value) => throw new NotSupportedException();

    protected virtual void Encode(T value, long[] values, int offset) => throw new NotSupportedException();

    protected virtual T Decode(long value) => throw new NotSupportedException();

    protected virtual T Decode(long[] values, int offset) => throw new NotSupportedException();

    public override object GetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(ref descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? (object) Load(tuple, ref descriptor) : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, object value)
    {
      if (value!=null) {
        Store(tuple, ref descriptor, (T) value);
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available);
      }
      else {
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available | TupleFieldState.Null);
      }
    }

    public override void CopyValue(PackedTuple source, ref PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, ref PackedFieldDescriptor targetDescriptor)
    {
      Store(target, ref targetDescriptor, Load(source, ref sourceDescriptor));
    }

    public override bool ValueEquals(PackedTuple left, ref PackedFieldDescriptor leftDescriptor,
      PackedTuple right, ref PackedFieldDescriptor rightDescriptor)
    {
      var leftValue = Load(left, ref leftDescriptor);
      var rightValue = Load(right, ref rightDescriptor);
      return leftValue.Equals(rightValue);
    }

    public override int GetValueHashCode(PackedTuple tuple, ref PackedFieldDescriptor descriptor)
    {
      return Load(tuple, ref descriptor).GetHashCode();
    }

    private T GetValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(ref descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? Load(tuple, ref descriptor) : DefaultValue;
    }

    private T? GetNullableValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(ref descriptor);
      fieldState = state;
      return state==TupleFieldState.Available ? Load(tuple, ref descriptor) : NullValue;
    }

    private void SetValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, T value)
    {
      Store(tuple, ref descriptor, value);
      tuple.SetFieldState(ref descriptor, TupleFieldState.Available);
    }

    private void SetNullableValue(PackedTuple tuple, ref PackedFieldDescriptor descriptor, T? value)
    {
      if (value!=null) {
        Store(tuple, ref descriptor, value.Value);
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available);
      }
      else
        tuple.SetFieldState(ref descriptor, TupleFieldState.Available | TupleFieldState.Null);
    }

    private void Store(PackedTuple tuple, ref PackedFieldDescriptor d, T value)
    {
      if (Rank > 6) {
        Encode(value, tuple.Values, d.ValueIndex);
        return;
      }

      var encoded = Encode(value);
      var block = tuple.Values[d.ValueIndex];
      var mask = d.ValueBitMask << d.ValueBitOffset;
      tuple.Values[d.ValueIndex] = (block & ~mask) | ((encoded << d.ValueBitOffset) & mask);
    }

    private T Load(PackedTuple tuple, ref PackedFieldDescriptor d)
    {
      if (Rank > 6) {
        return Decode(tuple.Values, d.ValueIndex);
      }

      var encoded = (tuple.Values[d.ValueIndex] >> d.ValueBitOffset) & d.ValueBitMask;
      return Decode(encoded);
    }

    protected ValueFieldAccessor(int bits)
      : base(bits)
    {
      FieldType = typeof(T);
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
      unsafe {
        return *(int*) &value;
      }
    }

    protected override float Decode(long value)
    {
      var intValue = unchecked ((int) value);
      unsafe {
        return *(float*) &intValue;
      }
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
      : base(sizeof(long) * 8)
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
      : base(sizeof(ulong) * 8)
    {
    }
  }

  internal sealed class GuidFieldAccessor : ValueFieldAccessor<Guid>
  {
    protected override Guid Decode(long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          return *(Guid*) valuePtr;
      }
    }

    protected override void Encode(Guid value, long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          *(Guid*) valuePtr = value;
      }
    }

    private static unsafe int GetSize()
    {
      return sizeof (Guid);
    }

    public GuidFieldAccessor()
      : base(GetSize() * 8)
    {
    }
  }

  internal sealed class DecimalFieldAccessor : ValueFieldAccessor<decimal>
  {
    protected override decimal Decode(long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          return *(decimal*) valuePtr;
      }
    }

    protected override void Encode(decimal value, long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          *(decimal*) valuePtr = value;
      }
    }
    public DecimalFieldAccessor()
      : base(sizeof(decimal) * 8)
    {
    }
  }
}
