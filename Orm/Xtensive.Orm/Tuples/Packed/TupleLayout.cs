// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections.Generic;
using System.Linq;
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

    private delegate void CounterIncrementer(ref ValCounters valCounters);
    private delegate void PositionUpdater(ref PackedFieldDescriptor descriptor, ref ValPointers valPointers);

    private static readonly ObjectFieldAccessor ObjectAccessor;
    private static readonly Dictionary<Type, ValueFieldAccessor> ValueAccessors;
    private static readonly CounterIncrementer[] IncrementerByRank;
    private static readonly PositionUpdater[] PositionUpdaterByRank;

    public static Type[] KnownTypes => ValueAccessors.Keys.Where(t => !t.IsGenericType)
      .Concat(new[] {typeof(string), typeof(byte[])})
      .ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureFieldAccessor(ref PackedFieldDescriptor descriptor, Type fieldType) =>
      descriptor.Accessor = ValueAccessors.TryGetValue(fieldType, out var valueAccessor)
        ? (PackedFieldAccessor) valueAccessor
        : ObjectAccessor;

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
    private static void ConfigureFieldPhase1(
      ref PackedFieldDescriptor descriptor, ref ValCounters counters, Type[] fieldTypes, int fieldIndex)
    {
      descriptor.StateIndex = fieldIndex >> Val032Rank; // d.FieldIndex / 32
      descriptor.StateBitOffset = (fieldIndex & Modulo032RemainderMask) << 1;

      if (ValueAccessors.TryGetValue(fieldTypes[fieldIndex], out var valueAccessor)) {
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
      void RegisterAccessor<T>(ValueFieldAccessor<T> accessor)
        where T : struct, IEquatable<T>
      {
        ValueAccessors.Add(typeof(T), accessor);
        ValueAccessors.Add(typeof(T?), accessor);
      }

      ObjectAccessor = new ObjectFieldAccessor();
      ValueAccessors = new Dictionary<Type, ValueFieldAccessor>();
      RegisterAccessor(new BooleanFieldAccessor());
      RegisterAccessor(new ByteFieldAccessor());
      RegisterAccessor(new SByteFieldAccessor());
      RegisterAccessor(new ShortFieldAccessor());
      RegisterAccessor(new UShortFieldAccessor());
      RegisterAccessor(new IntFieldAccessor());
      RegisterAccessor(new UIntFieldAccessor());
      RegisterAccessor(new LongFieldAccessor());
      RegisterAccessor(new ULongFieldAccessor());
      RegisterAccessor(new FloatFieldAccessor());
      RegisterAccessor(new DoubleFieldAccessor());
      RegisterAccessor(new DateTimeFieldAccessor());
      RegisterAccessor(new TimeSpanFieldAccessor());
      RegisterAccessor(new DecimalFieldAccessor());
      RegisterAccessor(new GuidFieldAccessor());

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
