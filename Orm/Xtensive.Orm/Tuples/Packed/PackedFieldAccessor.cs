// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.01.22

using System;
using System.Numerics;

namespace Xtensive.Tuples.Packed
{
  internal abstract class PackedFieldAccessor
  {
    public static readonly PackedFieldAccessor[] All = new PackedFieldAccessor[20];

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

    public readonly int Rank;
    public readonly int ValueBitCount;
    protected readonly long ValueBitMask;
    public readonly byte Index;

    public void SetValue<T>(PackedTuple tuple, in PackedFieldDescriptor descriptor, bool isNullable, T value)
    {
      if ((isNullable ? NullableSetter : Setter) is SetValueDelegate<T> setter) {
        setter.Invoke(tuple, descriptor, value);
      }
      else {
        SetUntypedValue(tuple, descriptor, value);
      }
    }

    public T GetValue<T>(PackedTuple tuple, in PackedFieldDescriptor descriptor, bool isNullable, out TupleFieldState fieldState)
    {
      var getter = (isNullable ? NullableGetter : Getter) as GetValueDelegate<T>;
      if (getter != null) {
        return getter.Invoke(tuple, descriptor, out fieldState);
      }
      var targetType = typeof(T);

      //Dirty hack of nullable enum reading
      if (isNullable) {
        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
      }
      if (targetType.IsEnum) {
        return (T) Enum.ToObject(targetType, GetUntypedValue(tuple, descriptor, out fieldState));
      }
      return (T) GetUntypedValue(tuple, descriptor, out fieldState);
    }

    public abstract object GetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, out TupleFieldState fieldState);

    public abstract void SetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, object value);

    public abstract void CopyValue(PackedTuple source, in PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, in PackedFieldDescriptor targetDescriptor);

    public abstract bool ValueEquals(PackedTuple left, in PackedFieldDescriptor leftDescriptor,
      PackedTuple right, in PackedFieldDescriptor rightDescriptor);

    public abstract int GetValueHashCode(PackedTuple tuple, in PackedFieldDescriptor descriptor);

    protected PackedFieldAccessor(int rank, byte index)
    {
      Rank = rank;
      Index = index;
      if (All[Index] != null) {
        throw new IndexOutOfRangeException($"Duplicated Index {Index} of PackedFieldAccessor instance");
      }
      All[Index] = this;
      ValueBitCount = 1 << Rank;

      // What we want here is to shift 1L by ValueBitCount to left and then subtract 1
      // This gives us a mask. For example if bit count = 4 then
      // 0000_0001 << 4 = 0001_0000
      // 0001_000 - 1 = 0000_1111
      // However in case bit count equal to data type size left shift doesn't work as we want
      // e.g. for Int8 : 0000_0001 << 8 = 0000_0001 but we would like it to be 0000_0000
      // because 0000_0000 - 1 = 1111_1111 and this is exactly what we need.
      // As a workaround we do left shift in two steps. In the example above
      // 0000_0001 << 7 = 1000_0000
      // and then
      // 1000_0000 << 1 = 0000_0000
      ValueBitMask = (1L << (ValueBitCount - 1) << 1) - 1;
    }
  }

  internal sealed class ObjectFieldAccessor : PackedFieldAccessor
  {
    public override object GetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      var state = tuple.GetFieldState(descriptor);
      fieldState = state;
      return state == TupleFieldState.Available ? tuple.Objects[descriptor.GetObjectIndex()] : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, object value)
    {
      tuple.Objects[descriptor.GetObjectIndex()] = value;
      tuple.SetFieldState(descriptor, value != null ? TupleFieldState.Available : (TupleFieldState.Available | TupleFieldState.Null));
    }

    public override void CopyValue(PackedTuple source, in PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, in PackedFieldDescriptor targetDescriptor)
    {
      target.Objects[targetDescriptor.GetObjectIndex()] = source.Objects[sourceDescriptor.GetObjectIndex()];
    }

    public override bool ValueEquals(PackedTuple left, in PackedFieldDescriptor leftDescriptor,
      PackedTuple right, in PackedFieldDescriptor rightDescriptor)
    {
      return Equals(left.Objects[leftDescriptor.GetObjectIndex()], right.Objects[rightDescriptor.GetObjectIndex()]);
    }

    public override int GetValueHashCode(PackedTuple tuple, in PackedFieldDescriptor descriptor)
    {
      return tuple.Objects[descriptor.GetObjectIndex()]?.GetHashCode() ?? 0;
    }

    public ObjectFieldAccessor()
      : base(-1, 1)
    { }
  }

  internal abstract class ValueFieldAccessor : PackedFieldAccessor
  {
    public Type FieldType { get; protected set; }

    private static int GetRank(int bitSize) =>
      BitOperations.Log2((uint)bitSize);

    protected ValueFieldAccessor(int bitCount, byte index)
      : base(GetRank(bitCount), index)
    {}
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

    public override object GetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      fieldState = tuple.GetFieldState(descriptor);
      return fieldState == TupleFieldState.Available ? (object) Load(tuple, descriptor) : null;
    }

    public override void SetUntypedValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, object value)
    {
      if (value != null) {
        Store(tuple, descriptor, (T) value);
        tuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else {
        tuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
      }
    }

    public override void CopyValue(PackedTuple source, in PackedFieldDescriptor sourceDescriptor,
      PackedTuple target, in PackedFieldDescriptor targetDescriptor)
    {
      Store(target, targetDescriptor, Load(source, sourceDescriptor));
    }

    public override bool ValueEquals(PackedTuple left, in PackedFieldDescriptor leftDescriptor,
        PackedTuple right, in PackedFieldDescriptor rightDescriptor) =>
      Load(left, leftDescriptor).Equals(Load(right, rightDescriptor));

    public override int GetValueHashCode(PackedTuple tuple, in PackedFieldDescriptor descriptor)
    {
      return Load(tuple, descriptor).GetHashCode();
    }

    private T GetValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      fieldState = tuple.GetFieldState(descriptor);
      return fieldState == TupleFieldState.Available ? Load(tuple, descriptor) : DefaultValue;
    }

    private T? GetNullableValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, out TupleFieldState fieldState)
    {
      fieldState = tuple.GetFieldState(descriptor);
      return fieldState == TupleFieldState.Available ? Load(tuple, descriptor) : NullValue;
    }

    private void SetValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, T value)
    {
      Store(tuple, descriptor, value);
      tuple.SetFieldState(descriptor, TupleFieldState.Available);
    }

    private void SetNullableValue(PackedTuple tuple, in PackedFieldDescriptor descriptor, T? value)
    {
      if (value != null) {
        Store(tuple, descriptor, value.Value);
        tuple.SetFieldState(descriptor, TupleFieldState.Available);
      }
      else {
        tuple.SetFieldState(descriptor, TupleFieldState.Available | TupleFieldState.Null);
      }
    }

    private void Store(PackedTuple tuple, in PackedFieldDescriptor d, T value)
    {
      var valueIndex = d.GetValueIndex();
      if (Rank > 6) {
        Encode(value, tuple.Values, valueIndex);
        return;
      }

      var encoded = Encode(value);
      ref var block = ref tuple.Values[valueIndex];
      var valueBitOffset = d.GetValueBitOffset();
      var mask = ValueBitMask << valueBitOffset;
      block = (block & ~mask) | ((encoded << valueBitOffset) & mask);
    }

    private T Load(PackedTuple tuple, in PackedFieldDescriptor d)
    {
      var valueIndex = d.GetValueIndex();
      if (Rank > 6) {
        return Decode(tuple.Values, valueIndex);
      }

      var encoded = (tuple.Values[valueIndex] >> d.GetValueBitOffset()) & ValueBitMask;
      return Decode(encoded);
    }

    protected ValueFieldAccessor(int bits, byte index)
      : base(bits, index)
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
      return value != 0;
    }

    public BooleanFieldAccessor()
      : base(1, 2)
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
      var intValue = unchecked((int) value);
      unsafe {
        return *(float*) &intValue;
      }
    }

    public FloatFieldAccessor()
      : base(sizeof(float) * 8, 3)
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
      : base(sizeof(double) * 8, 4)
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
      : base(sizeof(long) * 8, 5)
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
      : base(sizeof(long) * 8, 6)
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
      return unchecked((byte) value);
    }

    public ByteFieldAccessor()
      : base(sizeof(byte) * 8, 7)
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
      return unchecked((sbyte) value);
    }

    public SByteFieldAccessor()
      : base(sizeof(sbyte) * 8, 8)
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
      return unchecked((short) value);
    }

    public ShortFieldAccessor()
      : base(sizeof(short) * 8, 9)
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
      return unchecked((ushort) value);
    }

    public UShortFieldAccessor()
      : base(sizeof(ushort) * 8, 10)
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
      return unchecked((int) value);
    }

    public IntFieldAccessor()
      : base(sizeof(int) * 8, 11)
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
      return unchecked((uint) value);
    }

    public UIntFieldAccessor()
      : base(sizeof(uint) * 8, 12)
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
      : base(sizeof(long) * 8, 13)
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
      : base(sizeof(ulong) * 8, 14)
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
      return sizeof(Guid);
    }

    public GuidFieldAccessor()
      : base(GetSize() * 8, 15)
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
      : base(sizeof(decimal) * 8, 16)
    {
    }
  }

  internal sealed class DateTimeOffsetFieldAccessor : ValueFieldAccessor<DateTimeOffset>
  {
    protected override DateTimeOffset Decode(long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          return *(DateTimeOffset*) valuePtr;
      }
    }

    protected override void Encode(DateTimeOffset value, long[] values, int offset)
    {
      unsafe {
        fixed (long* valuePtr = &values[offset])
          *(DateTimeOffset*) valuePtr = value;
      }
    }

    private static unsafe int GetSize()
    {
      // Depending on architecture, x86 or x64, the size of DateTimeOffset is either 12 or 16 respectively.
      // Due to the fact that Rank calculation algorithm expects sizes to be equal to one of the power of two
      // it returns wrong rank value for size 12 (bitsize = 96) which causes wrong choice of Encode/Decode methods.
      // Setting it to 16 helps to solve Rank problem.
      return sizeof(long) * 2;
    }

    public DateTimeOffsetFieldAccessor()
       : base(GetSize() * 8, 17)
    { }
  }

  internal sealed class DateOnlyFieldAccessor : ValueFieldAccessor<DateOnly>
  {
    protected override DateOnly Decode(long value) =>
      DateOnly.FromDayNumber((int)value);

    protected override long Encode(DateOnly value) =>
      value.DayNumber;

    public DateOnlyFieldAccessor()
       : base(sizeof(int) * 8, 18)
    { }
  }

  internal sealed class TimeOnlyFieldAccessor : ValueFieldAccessor<TimeOnly>
  {
    protected override TimeOnly Decode(long value) =>
      new TimeOnly(value);

    protected override long Encode(TimeOnly value) =>
      value.Ticks;

    public TimeOnlyFieldAccessor()
       : base(sizeof(long) * 8, 19)
    { }
  }
}
