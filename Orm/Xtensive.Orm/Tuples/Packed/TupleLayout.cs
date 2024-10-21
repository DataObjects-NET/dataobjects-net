// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Runtime.CompilerServices;
using Xtensive.Reflection;

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

    private ref struct Counters
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
      private static readonly ValueFieldAccessor DateOnlyAccessor = new DateOnlyFieldAccessor();
      private static readonly ValueFieldAccessor TimeOnlyAccessor = new TimeOnlyFieldAccessor();
      private static readonly ValueFieldAccessor TimeSpanAccessor = new TimeSpanFieldAccessor();
      private static readonly ValueFieldAccessor DecimalAccessor = new DecimalFieldAccessor();
      private static readonly ValueFieldAccessor GuidAccessor = new GuidFieldAccessor();
      private static readonly ValueFieldAccessor DateTimeOffsetAccessor = new DateTimeOffsetFieldAccessor();

      private static readonly int NullableTypeMetadataToken = WellKnownTypes.NullableOfT.MetadataToken;

      public static ValueFieldAccessor GetValue(Type probeType)
      {
        return (probeType.MetadataToken ^ NullableTypeMetadataToken) == 0
          ? ResolveByNullableType(probeType)
          : ResolveByType(probeType);

        ValueFieldAccessor ResolveByType(Type type)
        {
          return ReferenceEquals(type, WellKnownTypes.Int64) ? Int64Accessor :
            ReferenceEquals(type, WellKnownTypes.Guid) ? GuidAccessor :
            ReferenceEquals(type, WellKnownTypes.Int32) ? Int32Accessor :
            ReferenceEquals(type, WellKnownTypes.Bool) ? BoolAccessor :
            ReferenceEquals(type, WellKnownTypes.DateTime) ? DateTimeAccessor :
            ReferenceEquals(type, WellKnownTypes.TimeSpan) ? TimeSpanAccessor :
            ReferenceEquals(type, WellKnownTypes.Double) ? DoubleAccessor :
            ReferenceEquals(type, WellKnownTypes.Decimal) ? DecimalAccessor :
            ReferenceEquals(type, WellKnownTypes.Single) ? SingleAccessor :
            ReferenceEquals(type, WellKnownTypes.DateTimeOffset) ? DateTimeOffsetAccessor :
            ReferenceEquals(type, WellKnownTypes.DateOnly) ? DateOnlyAccessor :
            ReferenceEquals(type, WellKnownTypes.TimeOnly) ? TimeOnlyAccessor :
            ReferenceEquals(type, WellKnownTypes.Int16) ? Int16Accessor :
            ReferenceEquals(type, WellKnownTypes.Byte) ? ByteAccessor :
            ReferenceEquals(type, WellKnownTypes.SByte) ? SByteAccessor :
            ReferenceEquals(type, WellKnownTypes.UInt16) ? UInt16Accessor :
            ReferenceEquals(type, WellKnownTypes.UInt32) ? UInt32Accessor :
            ReferenceEquals(type, WellKnownTypes.UInt64) ? UInt64Accessor : null;
        }

        ValueFieldAccessor ResolveByNullableType(Type type)
        {
          return ReferenceEquals(type, WellKnownTypes.NullableBool) ? BoolAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableInt32) ? Int32Accessor :
            ReferenceEquals(type, WellKnownTypes.NullableDouble) ? DoubleAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableDecimal) ? DecimalAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableInt64) ? Int64Accessor :
            ReferenceEquals(type, WellKnownTypes.NullableDateTime) ? DateTimeAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableTimeSpan) ? TimeSpanAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableDateTimeOffset) ? DateTimeOffsetAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableSingle) ? SingleAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableDateOnly) ? DateOnlyAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableTimeOnly) ? TimeOnlyAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableGuid) ? GuidAccessor : 
            ReferenceEquals(type, WellKnownTypes.NullableInt16) ? Int16Accessor :
            ReferenceEquals(type, WellKnownTypes.NullableByte) ? ByteAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableSByte) ? SByteAccessor :
            ReferenceEquals(type, WellKnownTypes.NullableUInt16) ? UInt16Accessor :
            ReferenceEquals(type, WellKnownTypes.NullableUInt32) ? UInt32Accessor :
            ReferenceEquals(type, WellKnownTypes.NullableUInt64) ? UInt64Accessor : null;
        }
      }
    }

    private delegate void CounterIncrementer(ref Counters counters);

    private delegate void PositionUpdater(ref PackedFieldDescriptor descriptor, ref Counters counters);

    private static readonly ObjectFieldAccessor ObjectAccessor = new ObjectFieldAccessor();
    private static readonly CounterIncrementer[] IncrementerByRank;
    private static readonly PositionUpdater[] PositionUpdaterByRank;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureFieldAccessor(ref PackedFieldDescriptor descriptor, Type fieldType) =>
      descriptor.AccessorIndex = ((PackedFieldAccessor)ValueFieldAccessorResolver.GetValue(fieldType) ?? ObjectAccessor).Index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureLen1(ref Type fieldType, ref PackedFieldDescriptor descriptor, out int valuesLength,
      out int objectsLength)
    {
      var valueAccessor = ValueFieldAccessorResolver.GetValue(fieldType);
      if (valueAccessor != null) {
        descriptor.AccessorIndex = valueAccessor.Index;
        descriptor.DataPosition = Val064BitCount;

        valuesLength = (valueAccessor.ValueBitCount  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
        objectsLength = 0;
        fieldType = valueAccessor.FieldType;
        return;
      }

      descriptor.AccessorIndex = ObjectAccessor.Index;
      valuesLength = 1;
      objectsLength = 1;
    }

    public static void ConfigureLen2(Type[] fieldTypes, ref PackedFieldDescriptor descriptor1,
      ref PackedFieldDescriptor descriptor2, out int valuesLength, out int objectsLength)
    {
      var counters = new Counters();
      ConfigureFieldPhase1(ref descriptor1, ref counters, fieldTypes, 0);
      ConfigureFieldPhase1(ref descriptor2, ref counters, fieldTypes, 1);
      objectsLength = counters.ObjectCounter;
      int val1BitCount, val2BitCount;
      switch (objectsLength) {
        case 2:
          valuesLength = 1;
          return;
        case 1: {
          if (descriptor1.IsObjectField()) {
            descriptor2.DataPosition = Val064BitCount;
            val1BitCount = descriptor2.GetAccessor().ValueBitCount;
          }
          else {
            descriptor1.DataPosition = Val064BitCount;
            val1BitCount = descriptor1.GetAccessor().ValueBitCount;
          }
          valuesLength = (val1BitCount  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
          return;
        }
      }
      // Both descriptors are value descriptors
      val1BitCount = descriptor1.GetAccessor().ValueBitCount;
      val2BitCount = descriptor2.GetAccessor().ValueBitCount;
      if (val2BitCount > val1BitCount) {
        descriptor2.DataPosition = Val064BitCount;
        descriptor1.DataPosition = Val064BitCount + val2BitCount;
      }
      else {
        descriptor1.DataPosition = Val064BitCount;
        descriptor2.DataPosition = Val064BitCount + val1BitCount;
      }
      valuesLength = (val1BitCount + val2BitCount  + ((Val064BitCount * 2) - 1)) >> Val064Rank;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Configure(Type[] fieldTypes, PackedFieldDescriptor[] fieldDescriptors, out int valuesLength,
      out int objectsLength)
    {
      var fieldCount = fieldTypes.Length;
      var counters = new Counters();
      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ConfigureFieldPhase1(ref fieldDescriptors[fieldIndex], ref counters, fieldTypes, fieldIndex);
      }

      const int statesPerLong = Val064BitCount / 2;

      var totalBitCount = ((fieldCount + (statesPerLong - 1)) >> Val032Rank) << Val064Rank;
      var prevCount = counters.Val128Counter;
      counters.Val128Counter = totalBitCount;

      totalBitCount += prevCount << Val128Rank;
      prevCount = counters.Val064Counter;
      counters.Val064Counter = totalBitCount;

      totalBitCount += prevCount << Val064Rank;
      prevCount = counters.Val032Counter;
      counters.Val032Counter = totalBitCount;

      totalBitCount += prevCount << Val032Rank;
      prevCount = counters.Val016Counter;
      counters.Val016Counter = totalBitCount;

      totalBitCount += prevCount << Val016Rank;
      prevCount = counters.Val008Counter;
      counters.Val008Counter = totalBitCount;

      totalBitCount += prevCount << Val008Rank;
      prevCount = counters.Val001Counter;
      counters.Val001Counter = totalBitCount;

      totalBitCount += prevCount;

      for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
        ref var descriptor = ref fieldDescriptors[fieldIndex];
        if (descriptor.IsObjectField()) {
          continue;
        }

        PositionUpdaterByRank[descriptor.GetAccessor().Rank].Invoke(ref descriptor, ref counters);
      }

      valuesLength = (totalBitCount + (Val064BitCount - 1)) >> Val064Rank;
      objectsLength = counters.ObjectCounter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateDescriptorPosition(ref PackedFieldDescriptor descriptor, ref int bitCounter)
    {
      descriptor.DataPosition = bitCounter;
      bitCounter += descriptor.GetAccessor().ValueBitCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConfigureFieldPhase1(ref PackedFieldDescriptor descriptor, ref Counters counters,
      Type[] fieldTypes, int fieldIndex)
    {
      descriptor.StatePosition = checked((ushort)(fieldIndex << 1));

      ref var fieldType = ref fieldTypes[fieldIndex];
      var valueAccessor = ValueFieldAccessorResolver.GetValue(fieldType);
      if (valueAccessor != null) {
        descriptor.AccessorIndex = valueAccessor.Index;

        IncrementerByRank[valueAccessor.Rank].Invoke(ref counters);

        fieldType = valueAccessor.FieldType;
        return;
      }

      descriptor.AccessorIndex = ObjectAccessor.Index;
      descriptor.DataPosition = counters.ObjectCounter++;
    }

    static TupleLayout()
    {
      IncrementerByRank = new CounterIncrementer[] {
        (ref Counters counters) => counters.Val001Counter++,
        (ref Counters counters) => throw new NotSupportedException(),
        (ref Counters counters) => throw new NotSupportedException(),
        (ref Counters counters) => counters.Val008Counter++,
        (ref Counters counters) => counters.Val016Counter++,
        (ref Counters counters) => counters.Val032Counter++,
        (ref Counters counters) => counters.Val064Counter++,
        (ref Counters counters) => counters.Val128Counter++
      };

      PositionUpdaterByRank = new PositionUpdater[] {
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val001Counter),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => throw new NotSupportedException(),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val008Counter),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val016Counter),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val032Counter),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val064Counter),
        (ref PackedFieldDescriptor descriptor, ref Counters counters)
          => UpdateDescriptorPosition(ref descriptor, ref counters.Val128Counter)
      };
    }
  }
}
