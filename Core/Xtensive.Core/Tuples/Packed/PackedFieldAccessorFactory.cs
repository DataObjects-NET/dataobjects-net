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
    private static readonly Dictionary<Type, PackedFieldAccessor> ValueAccessors;

    public static void ProvideAccessor(Type valueType, PackedFieldDescriptor descriptor)
    {
      if (ValueAccessors.TryGetValue(valueType, out descriptor.Accessor)) {
        descriptor.PackingType = FieldPackingType.Value;
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

      ValueAccessors = new Dictionary<Type, PackedFieldAccessor>();

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
    }
  }
}