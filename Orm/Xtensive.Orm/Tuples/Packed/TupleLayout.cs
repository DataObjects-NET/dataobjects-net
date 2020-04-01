// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Runtime.CompilerServices;

namespace Xtensive.Tuples.Packed
{
  internal static class TupleLayout
  {
    private const int Val128Rank = 7;
    private const int Val064Rank = 6;
    private const int Val032Rank = 5;
    private const int Val016Rank = 4;
    private const int Val008Rank = 3;
    private const int Val001Rank = 0;

    private const int Val064BitCount = 1 << Val064Rank;
    private const int Val032BitCount = 1 << Val032Rank;
    private const int Modulo064RemainderMask = Val064BitCount - 1;
    private const int Modulo032RemainderMask = Val032BitCount - 1;

    private ref struct ValPointer
    {
      public int Index;
      public int Offset;
    }

    private ref struct ValPointers
    {
      public ValPointer Val001Pointer;
      public ValPointer Val008Pointer;
      public ValPointer Val016Pointer;
      public ValPointer Val032Pointer;
      public ValPointer Val064Pointer;
      public ValPointer Val128Pointer;
    }

    private ref struct ValCounters
    {
      public int ObjectCounter;

      public int Val001Counter;
      public int Val008Counter;
      public int Val016Counter;
      public int Val032Counter;
      public int Val064Counter;
      public int Val128Counter;
    }

    private static class ValueFieldAccessorResolver
    {
      private static readonly Type BoolType = typeof(bool);
      private static readonly Type NullableBoolType = typeof(bool?);
      private static readonly Type ByteType = typeof(byte);
      private static readonly Type NullableByteType = typeof(byte?);
      private static readonly Type SByteType = typeof(sbyte);
      private static readonly Type NullableSByteType = typeof(sbyte?);
      private static readonly Type Int16Type = typeof(short);
      private static readonly Type NullableInt16Type = typeof(short?);
      private static readonly Type UInt16Type = typeof(ushort);
      private static readonly Type NullableUInt16Type = typeof(ushort?);
      private static readonly Type Int32Type = typeof(int);
      private static readonly Type NullableInt32Type = typeof(int?);
      private static readonly Type UInt32Type = typeof(uint);
      private static readonly Type NullableUInt32Type = typeof(uint?);
      private static readonly Type Int64Type = typeof(long);
      private static readonly Type NullableInt64Type = typeof(long?);
      private static readonly Type UInt64Type = typeof(ulong);
      private static readonly Type NullableUInt64Type = typeof(ulong?);
      private static readonly Type SingleType = typeof(float);
      private static readonly Type NullableSingleType = typeof(float?);
      private static readonly Type DoubleType = typeof(double);
      private static readonly Type NullableDoubleType = typeof(double?);
      private static readonly Type DateTimeType = typeof(DateTime);
      private static readonly Type NullableDateTimeType = typeof(DateTime?);
      private static readonly Type TimeSpanType = typeof(TimeSpan);
      private static readonly Type NullableTimeSpanType = typeof(TimeSpan?);
      private static readonly Type DecimalType = typeof(decimal);
      private static readonly Type NullableDecimalType = typeof(decimal?);
      private static readonly Type GuidType = typeof(Guid);
      private static readonly Type NullableGuidType = typeof(Guid?);

      private static readonly ValueFieldAccessor BoolAccessor = new BooleanFieldAccessor();
      private static readonly ValueFieldAccessor ByteAccessor = new ByteFieldAccessor();
      private static readonly ValueFieldAccessor SByteAccessor = new SByteFieldAccessor();
      private static readonly ValueFieldAccessor Int16Accessor = new ShortFieldAccessor();
      private static readonly ValueFieldAccessor UInt16Accessor = new UShortFieldAccessor();
      private static readonly ValueFieldAccessor Int32Accessor = new IntFieldAccessor();
      private static readonly ValueFieldAccessor UInt32Accessor = new UIntFieldAccessor();
      private static readonly ValueFieldAccessor Int64Accessor = new LongFieldAccessor();
      private static readonly ValueFieldAccessor UInt64Accessor = new ULongFieldAccessor();
      private static readonly ValueFieldAccessor SingleAccessor = new FloatFieldAccessor();
      private static readonly ValueFieldAccessor DoubleAccessor = new DoubleFieldAccessor();
      private static readonly ValueFieldAccessor DateTimeAccessor = new DateTimeFieldAccessor();
      private static readonly ValueFieldAccessor TimeSpanAccessor = new TimeSpanFieldAccessor();
      private static readonly ValueFieldAccessor DecimalAccessor = new DecimalFieldAccessor();
      private static readonly ValueFieldAccessor GuidAccessor = new GuidFieldAccessor();

      public static ValueFieldAccessor GetValue(Type probeType)
      {
        ValueFieldAccessor ResolveByType(Type type) =>
          ReferenceEquals(type, BoolType) ? BoolAccessor :
          ReferenceEquals(type, ByteType) ? ByteAccessor :
          ReferenceEquals(type, SByteType) ? SByteAccessor :
          ReferenceEquals(type, Int16Type) ? Int16Accessor :
          ReferenceEquals(type, UInt16Type) ? UInt16Accessor :
          ReferenceEquals(type, Int32Type) ? Int32Accessor :
          ReferenceEquals(type, UInt32Type) ? UInt32Accessor :
          ReferenceEquals(type, Int64Type) ? Int64Accessor :
          ReferenceEquals(type, UInt64Type) ? UInt64Accessor :
          ReferenceEquals(type, SingleType) ? SingleAccessor :
          ReferenceEquals(type, DoubleType) ? DoubleAccessor :
          ReferenceEquals(type, DateTimeType) ? DateTimeAccessor :
          ReferenceEquals(type, TimeSpanType) ? TimeSpanAccessor :
          ReferenceEquals(type, DecimalType) ? DecimalAccessor :
          ReferenceEquals(type, GuidType) ? GuidAccessor : null;

        ValueFieldAccessor ResolveByNullableType(Type type) =>
          ReferenceEquals(type, NullableBoolType) ? BoolAccessor :
          ReferenceEquals(type, NullableByteType) ? ByteAccessor :
          ReferenceEquals(type, NullableSByteType) ? SByteAccessor :
          ReferenceEquals(type, NullableInt16Type) ? Int16Accessor :
          ReferenceEquals(type, NullableUInt16Type) ? UInt16Accessor :
          ReferenceEquals(type, NullableInt32Type) ? Int32Accessor :
          ReferenceEquals(type, NullableUInt32Type) ? UInt32Accessor :
          ReferenceEquals(type, NullableInt64Type) ? Int64Accessor :
          ReferenceEquals(type, NullableUInt64Type) ? UInt64Accessor :
          ReferenceEquals(type, NullableSingleType) ? SingleAccessor :
          ReferenceEquals(type, NullableDoubleType) ? DoubleAccessor :
          ReferenceEquals(type, NullableDateTimeType) ? DateTimeAccessor :
          ReferenceEquals(type, NullableTimeSpanType) ? TimeSpanAccessor :
          ReferenceEquals(type, NullableDecimalType) ? DecimalAccessor :
          ReferenceEquals(type, NullableGuidType) ? GuidAccessor : null;

        return probeType.IsGenericType ? ResolveByNullableType(probeType) : ResolveByType(probeType);
      }
    }

    private delegate void CounterIncrementer(ref ValCounters valCounters);

    private delegate void PositionUpdater(ref PackedFieldDescriptor descriptor, ref ValPointers valPointers);

    private static readonly ObjectFieldAccessor ObjectAccessor = new ObjectFieldAccessor();
    private static readonly CounterIncrementer[] IncrementerByRank;
    private static readonly PositionUpdater[] PositionUpdaterByRank;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureFieldAccessor(ref PackedFieldDescriptor descriptor, Type fieldType) =>
      descriptor.Accessor = (PackedFieldAccessor) ValueFieldAccessorResolver.GetValue(fieldType) ?? ObjectAccessor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Configure(Type[] fieldTypes, PackedFieldDescriptor[] fieldDescriptors, out int valuesLength,
      out int objectsLength)
    {
      var valCounters = new ValCounters();
      var fieldCount = fieldTypes.Length;
      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ConfigureFieldPhase1(ref fieldDescriptors[fieldIndex], ref valCounters, fieldTypes, fieldIndex);
      }

      const int stateBitCount = 2;
      const int statesPerLong = Val064BitCount / stateBitCount;

      var valPointers = new ValPointers {
        Val128Pointer = new ValPointer {
          Index = (fieldCount + (statesPerLong - 1)) / statesPerLong,
          Offset = 0
        }
      };
      InitValPointer(ref valPointers.Val064Pointer, ref valPointers.Val128Pointer, valCounters.Val128Counter, Val128Rank);
      InitValPointer(ref valPointers.Val032Pointer, ref valPointers.Val064Pointer, valCounters.Val064Counter, Val064Rank);
      InitValPointer(ref valPointers.Val016Pointer, ref valPointers.Val032Pointer, valCounters.Val032Counter, Val032Rank);
      InitValPointer(ref valPointers.Val008Pointer, ref valPointers.Val016Pointer, valCounters.Val016Counter, Val016Rank);
      InitValPointer(ref valPointers.Val001Pointer, ref valPointers.Val008Pointer, valCounters.Val008Counter, Val008Rank);

      var valuesEndPointer = new ValPointer();
      InitValPointer(ref valuesEndPointer, ref valPointers.Val001Pointer, valCounters.Val001Counter, Val001Rank);

      const int max032Shift = Val032BitCount - 1;
      // Expression ((N - 1) >> max032Shift) evaluates to 0 for all N > 0; for N <= 0 it evaluates to -1
      // This means we add one element to values array if Offset is positive; otherwise we don't
      valuesLength = valuesEndPointer.Index + ((valuesEndPointer.Offset - 1) >> max032Shift) + 1;

      objectsLength = valCounters.ObjectCounter;

      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ConfigureFieldPhase2(ref fieldDescriptors[fieldIndex], ref valPointers);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitValPointer(
      ref ValPointer pointer, ref ValPointer prevPointer, int prevValueCount, int prevRank)
    {
      var prevBitCountWithOffset = (prevValueCount << prevRank) + prevPointer.Offset;

      pointer.Index = prevPointer.Index + (prevBitCountWithOffset >> Val064Rank);
      pointer.Offset = prevBitCountWithOffset & Modulo064RemainderMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateDescriptorPosition(ref PackedFieldDescriptor descriptor, ref ValPointer valPointer)
    {
      descriptor.ValueIndex = valPointer.Index;
      descriptor.ValueBitOffset = valPointer.Offset;

      var increasedOffset = valPointer.Offset + descriptor.ValueBitCount;
      valPointer.Index += increasedOffset >> Val064Rank;
      valPointer.Offset = increasedOffset & Modulo064RemainderMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConfigureFieldPhase1(ref PackedFieldDescriptor descriptor, ref ValCounters counters,
      Type[] fieldTypes, int fieldIndex)
    {
      descriptor.StateIndex = fieldIndex >> Val032Rank; // d.FieldIndex / 32
      descriptor.StateBitOffset = (fieldIndex & Modulo032RemainderMask) << 1;

      var valueAccessor = ValueFieldAccessorResolver.GetValue(fieldTypes[fieldIndex]);
      if (valueAccessor != null) {
        descriptor.Accessor = valueAccessor;
        descriptor.PackingType = FieldPackingType.Value;
        descriptor.Rank = valueAccessor.Rank;

        IncrementerByRank[valueAccessor.Rank].Invoke(ref counters);

        fieldTypes[fieldIndex] = valueAccessor.FieldType;
        return;
      }

      descriptor.Accessor = ObjectAccessor;
      descriptor.PackingType = FieldPackingType.Object;
      descriptor.ValueIndex = counters.ObjectCounter++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConfigureFieldPhase2(ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
    {
      if (descriptor.PackingType == FieldPackingType.Object) {
        return;
      }

      // d.PackingType == FieldPackingType.Value
      PositionUpdaterByRank[descriptor.Rank].Invoke(ref descriptor, ref valPointers);
    }

    static TupleLayout()
    {
      IncrementerByRank = new CounterIncrementer[] {
        (ref ValCounters valueCounters) => valueCounters.Val001Counter++,
        (ref ValCounters valueCounters) => throw new NotSupportedException(),
        (ref ValCounters valueCounters) => throw new NotSupportedException(),
        (ref ValCounters valueCounters) => valueCounters.Val008Counter++,
        (ref ValCounters valueCounters) => valueCounters.Val016Counter++,
        (ref ValCounters valueCounters) => valueCounters.Val032Counter++,
        (ref ValCounters valueCounters) => valueCounters.Val064Counter++,
        (ref ValCounters valueCounters) => valueCounters.Val128Counter++
      };

      PositionUpdaterByRank = new PositionUpdater[] {
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val001Pointer),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val008Pointer),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val016Pointer),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val032Pointer),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val064Pointer),
        (ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
          => UpdateDescriptorPosition(ref descriptor, ref valPointers.Val128Pointer)
      };
    }
  }
}
