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
    private static readonly PackedFieldAccessor BooleanAccessor;
    private static readonly Dictionary<Type, PackedFieldAccessor> ValueAccessors;

    public static void ProvideAccessor(Type valueType, PackedFieldDescriptor descriptor)
    {
      if (Type.GetTypeCode(valueType)==TypeCode.Boolean) {
        descriptor.Accessor = BooleanAccessor;
        descriptor.PackingType = FieldPackingType.Flag;
        return;
      }

      if (ValueAccessors.TryGetValue(valueType, out descriptor.Accessor)) {
        descriptor.PackingType = FieldPackingType.Value;
        return;
      }

      descriptor.Accessor = ObjectAccessor;
      descriptor.PackingType = FieldPackingType.Object;
    }

    private static void RegisterValueAccessor<T>(ValueFieldAccessor<T> accessor)
      where T : struct
    {
      accessor.Initialize();
      ValueAccessors.Add(typeof (T), accessor);
    }

    static PackedFieldAccessorFactory()
    {
      ValueAccessors = new Dictionary<Type, PackedFieldAccessor>();
      ObjectAccessor = new ObjectFieldAccessor();
      BooleanAccessor = new BooleanFieldAccessor();

      RegisterValueAccessor(new ByteFieldAccessor());
      RegisterValueAccessor(new SByteFieldAccessor());
      RegisterValueAccessor(new ShortFieldAccessor());
      RegisterValueAccessor(new UShortFieldAccessor());
      RegisterValueAccessor(new IntFieldAccessor());
      RegisterValueAccessor(new UIntFieldAccessor());
      RegisterValueAccessor(new LongFieldAccessor());
      RegisterValueAccessor(new ULongFieldAccessor());
      RegisterValueAccessor(new FloatFieldAccessor());
      RegisterValueAccessor(new DoubleFieldAccessor());
      RegisterValueAccessor(new DateTimeFieldAccessor());
      RegisterValueAccessor(new TimeSpanFieldAccessor());
    }
  }
}