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

    private const int Val064BitCount = 1 << Val064Rank;

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

      private static readonly int NullableTypeMetadataToken = typeof(Nullable<>).MetadataToken;

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

        return (probeType.MetadataToken ^ NullableTypeMetadataToken)==0
          ? ResolveByNullableType(probeType)
          : ResolveByType(probeType);
      }
    }

    private delegate void CounterIncrementer(ref ValCounters valCounters);

    private delegate void PositionUpdater(ref PackedFieldDescriptor descriptor, ref ValCounters valCounters);

    private static readonly ObjectFieldAccessor ObjectAccessor = new ObjectFieldAccessor();
    private static readonly CounterIncrementer[] IncrementerByRank;
    private static readonly PositionUpdater[] PositionUpdaterByRank;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureFieldAccessor(ref PackedFieldDescriptor descriptor, Type fieldType) =>
      descriptor.Accessor = (PackedFieldAccessor) ValueFieldAccessorResolver.GetValue(fieldType) ?? ObjectAccessor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureLen1(Type[] fieldTypes, ref PackedFieldDescriptor descriptor, out int valuesLength,
      out int objectsLength)
    {
      var valueAccessor = ValueFieldAccessorResolver.GetValue(fieldTypes[0]);
      if (valueAccessor != null) {
        descriptor.Accessor = valueAccessor;
        descriptor.SetValueBitOffset(Val064BitCount);

        valuesLength = ((1 << valueAccessor.Rank)  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
        objectsLength = 0;
        fieldTypes[0] = valueAccessor.FieldType;
        return;
      }

      descriptor.Accessor = ObjectAccessor;
      valuesLength = 1;
      objectsLength = 1;
    }

    public static void ConfigureLen2(Type[] fieldTypes, ref PackedFieldDescriptor descriptor1,
      ref PackedFieldDescriptor descriptor2, out int valuesLength, out int objectsLength)
    {
      var valCounters = new ValCounters();
      ConfigureFieldPhase1(ref descriptor1, ref valCounters, fieldTypes, 0);
      ConfigureFieldPhase1(ref descriptor2, ref valCounters, fieldTypes, 1);
      objectsLength = valCounters.ObjectCounter;
      int val1BitCount, val2BitCount;
      switch (objectsLength) {
        case 2:
          valuesLength = 1;
          return;
        case 1: {
          if (descriptor1.IsObjectField) {
            descriptor2.SetValueBitOffset(Val064BitCount);
            val1BitCount = descriptor2.Accessor.ValueBitCount;
          }
          else {
            descriptor1.SetValueBitOffset(Val064BitCount);
            val1BitCount = descriptor1.Accessor.ValueBitCount;
          }
          valuesLength = (val1BitCount  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
          return;
        }
      }
      // Both descriptors are value descriptors
      val1BitCount = descriptor1.Accessor.ValueBitCount;
      val2BitCount = descriptor2.Accessor.ValueBitCount;
      if (val2BitCount > val1BitCount) {
        descriptor2.SetValueBitOffset(Val064BitCount);
        descriptor1.SetValueBitOffset(Val064BitCount + val2BitCount);
      }
      else {
        descriptor1.SetValueBitOffset(Val064BitCount);
        descriptor2.SetValueBitOffset(Val064BitCount + val1BitCount);
      }
      valuesLength = (val1BitCount + val2BitCount  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Configure(Type[] fieldTypes, PackedFieldDescriptor[] fieldDescriptors, out int valuesLength,
      out int objectsLength)
    {
      var fieldCount = fieldTypes.Length;
      var valCounters = new ValCounters();
      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ConfigureFieldPhase1(ref fieldDescriptors[fieldIndex], ref valCounters, fieldTypes, fieldIndex);
      }

      const int statesPerLong = Val064BitCount / 2;

      var totalBitCount = ((fieldCount + (statesPerLong - 1)) >> Val032Rank) << Val064Rank;
      var prevCount = valCounters.Val128Counter;
      valCounters.Val128Counter = totalBitCount;

      totalBitCount += prevCount << Val128Rank;
      prevCount = valCounters.Val064Counter;
      valCounters.Val064Counter = totalBitCount;

      totalBitCount += prevCount << Val064Rank;
      prevCount = valCounters.Val032Counter;
      valCounters.Val032Counter = totalBitCount;

      totalBitCount += prevCount << Val032Rank;
      prevCount = valCounters.Val016Counter;
      valCounters.Val016Counter = totalBitCount;

      totalBitCount += prevCount << Val016Rank;
      prevCount = valCounters.Val008Counter;
      valCounters.Val008Counter = totalBitCount;

      totalBitCount += prevCount << Val008Rank;
      prevCount = valCounters.Val001Counter;
      valCounters.Val001Counter = totalBitCount;

      totalBitCount += prevCount;

      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ConfigureFieldPhase2(ref fieldDescriptors[fieldIndex], ref valCounters);
      }

      valuesLength = (totalBitCount + (Val064BitCount - 1)) >> Val064Rank;
      objectsLength = valCounters.ObjectCounter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateDescriptorPosition(ref PackedFieldDescriptor descriptor, ref int valPointer)
    {
      descriptor.SetValueBitOffset(valPointer);
      valPointer += descriptor.Accessor.ValueBitCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConfigureFieldPhase1(ref PackedFieldDescriptor descriptor, ref ValCounters counters,
      Type[] fieldTypes, int fieldIndex)
    {
      descriptor.SetStateTotalBitOffset(fieldIndex << 1);

      var valueAccessor = ValueFieldAccessorResolver.GetValue(fieldTypes[fieldIndex]);
      if (valueAccessor != null) {
        descriptor.Accessor = valueAccessor;

        IncrementerByRank[valueAccessor.Rank].Invoke(ref counters);

        fieldTypes[fieldIndex] = valueAccessor.FieldType;
        return;
      }

      descriptor.Accessor = ObjectAccessor;
      descriptor.ObjectIndex = counters.ObjectCounter++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConfigureFieldPhase2(ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
    {
      if (descriptor.IsObjectField) {
        return;
      }

      // d.PackingType == FieldPackingType.Value
      PositionUpdaterByRank[descriptor.Accessor.Rank].Invoke(ref descriptor, ref valCounters);
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
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val001Counter),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val008Counter),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val016Counter),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val032Counter),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val064Counter),
        (ref PackedFieldDescriptor descriptor, ref ValCounters valCounters)
          => UpdateDescriptorPosition(ref descriptor, ref valCounters.Val128Counter)
      };
    }
  }
}
