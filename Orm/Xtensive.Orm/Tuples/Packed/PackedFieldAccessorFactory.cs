// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections.Generic;

namespace Xtensive.Tuples.Packed
{
  internal static class PackedFieldAccessorFactory
  {
    private static readonly PackedFieldAccessor ObjectAccessor;
    private static readonly Dictionary<Type, ValueFieldAccessor> ValueAccessors;

    public static IEnumerable<Type> KnownTypes => ValueAccessors.Keys;

    public static void ConfigureDescriptor(ref PackedFieldDescriptor descriptor, int fieldIndex, Type accessorType)
    {
      descriptor.FieldIndex = fieldIndex;
      if (ValueAccessors.TryGetValue(accessorType, out var valueAccessor)) {
        descriptor.Accessor = valueAccessor;
        descriptor.PackingType = FieldPackingType.Value;
        descriptor.ValueBitCount = valueAccessor.BitCount;
        descriptor.ValueBitMask = valueAccessor.BitMask;
      }
      else {
        descriptor.Accessor = ObjectAccessor;
        descriptor.PackingType = FieldPackingType.Object;
      }
    }

    private static void RegisterAccessor<T>(ValueFieldAccessor<T> accessor)
      where T : struct, IEquatable<T>
    {
      ValueAccessors.Add(typeof (T), accessor);
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
  }
}
