// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Tuples.Packed
{
  internal ref struct ValPointers
  {
    public ValPointer Val001Pointer;
    public ValPointer Val008Pointer;
    public ValPointer Val016Pointer;
    public ValPointer Val032Pointer;
    public ValPointer Val064Pointer;
    public ValPointer Val128Pointer;
  }

  internal ref struct ValPointer
  {
    public int Index;
    public int Offset;
  }

  internal ref struct ValCounters
  {
    public int ObjectCounter;

    public int Val001Counter;
    public int Val008Counter;
    public int Val016Counter;
    public int Val032Counter;
    public int Val064Counter;
    public int Val128Counter;
  }

  internal static class PackedFieldAccessorFactory
  {
    private delegate int CounterIncrementer(ref ValCounters valCounters);
    private delegate void PositionUpdater(ref PackedFieldDescriptor descriptor, ref ValPointers valPointers);

    private static readonly CounterIncrementer[] incrementers = {
      (ref ValCounters valueCounters) => valueCounters.Val001Counter++,
      (ref ValCounters valueCounters) => throw new NotSupportedException(),
      (ref ValCounters valueCounters) => throw new NotSupportedException(),
      (ref ValCounters valueCounters) => valueCounters.Val008Counter++,
      (ref ValCounters valueCounters) => valueCounters.Val016Counter++,
      (ref ValCounters valueCounters) => valueCounters.Val032Counter++,
      (ref ValCounters valueCounters) => valueCounters.Val064Counter++,
      (ref ValCounters valueCounters) => valueCounters.Val128Counter++
    };

    private static readonly PositionUpdater[] positionUpdaters = {
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

    private const int Val064Rank = 6;

    private static void UpdateDescriptorPosition(ref PackedFieldDescriptor descriptor, ref ValPointer valPointer)
    {
      const int remainderBitMask = (1 << Val064Rank) - 1;

      var increasedOffset = valPointer.Offset + descriptor.ValueBitCount;

      descriptor.ValueIndex = valPointer.Index;
      descriptor.ValueBitOffset = valPointer.Offset;

      valPointer.Index += increasedOffset >> Val064Rank;
      valPointer.Offset = increasedOffset & remainderBitMask;
    }

    private static readonly ObjectFieldAccessor ObjectAccessor;
    private static readonly Dictionary<Type, ValueFieldAccessor> ValueAccessors;

    public static IEnumerable<Type> KnownTypes => ValueAccessors.Keys.Where(t => !t.IsGenericType);

    public static void SetAccessor(ref PackedFieldDescriptor descriptor, Type fieldType) =>
      descriptor.Accessor = ValueAccessors.TryGetValue(fieldType, out var valueAccessor)
        ? (PackedFieldAccessor) valueAccessor
        : ObjectAccessor;

    public static Type ConfigureDescriptorPhase1(ref ValCounters counters, ref PackedFieldDescriptor descriptor, int fieldIndex, Type fieldType)
    {
      descriptor.StateIndex = fieldIndex >> 5; // d.FieldIndex / 32
      descriptor.StateBitOffset = (fieldIndex & 31) << 1;

      if (ValueAccessors.TryGetValue(fieldType, out var valueAccessor)) {
        ConfigureValueDescriptor(ref descriptor, ref counters, valueAccessor);
        return valueAccessor.FieldType;
      }

      ConfigureObjectDescriptor(ref descriptor, ref counters, ObjectAccessor);
      return fieldType;
    }

    private static void ConfigureObjectDescriptor(ref PackedFieldDescriptor descriptor, ref ValCounters counters,
      ObjectFieldAccessor objectAccessor)
    {
      descriptor.Accessor = objectAccessor;
      descriptor.PackingType = FieldPackingType.Object;
      descriptor.ValueIndex = counters.ObjectCounter++;
    }

    private static void ConfigureValueDescriptor(ref PackedFieldDescriptor descriptor, ref ValCounters counters,
      ValueFieldAccessor valueAccessor)
    {
        descriptor.Accessor = valueAccessor;
        descriptor.PackingType = FieldPackingType.Value;
        descriptor.Rank = valueAccessor.Rank;
        descriptor.ValueIndex = incrementers[valueAccessor.Rank].Invoke(ref counters);
    }

    private static void RegisterAccessor<T>(ValueFieldAccessor<T> accessor)
      where T : struct, IEquatable<T>
    {
      ValueAccessors.Add(typeof (T), accessor);
      ValueAccessors.Add(typeof (T?), accessor);
    }

    static PackedFieldAccessorFactory()
    {
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
    }

    public static void ConfigureDescriptorPhase2(ref PackedFieldDescriptor descriptor, ref ValPointers valPointers)
    {
      if (descriptor.PackingType == FieldPackingType.Object) {
        return;
      }

      // d.PackingType == FieldPackingType.Value
      positionUpdaters[descriptor.Rank].Invoke(ref descriptor, ref valPointers);
    }
  }
}
