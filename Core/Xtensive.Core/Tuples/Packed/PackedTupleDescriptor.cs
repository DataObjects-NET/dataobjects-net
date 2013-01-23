// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections.Generic;

namespace Xtensive.Tuples.Packed
{
  internal sealed class PackedTupleDescriptor : TupleDescriptor
  {
    public int FlagsLength;
    public int ValuesLength;
    public int ObjectsLength;

    public PackedFieldDescriptor[] FieldDescriptors;

    private void Initialize()
    {
      var valueIndex = 0;
      var objectIndex = 0;
      var flagIndex = FieldCount * 2;

      FieldDescriptors = new PackedFieldDescriptor[FieldCount];

      int fieldIndex = 0;
      foreach (var type in FieldTypes) {
        var descriptor = new PackedFieldDescriptor();
        FieldDescriptors[fieldIndex++] = descriptor;
        PackedFieldAccessorFactory.ProvideAccessor(type, descriptor);
        switch (descriptor.PackingType) {
        case FieldPackingType.Object:
          descriptor.Index = objectIndex++;
          break;
        case FieldPackingType.Flag:
          descriptor.Index = flagIndex++;
          break;
        case FieldPackingType.Value:
          descriptor.Index = valueIndex++;
          break;
        default:
          throw new ArgumentOutOfRangeException("descriptor.PackType");
        }
      }

      ValuesLength = valueIndex;
      ObjectsLength = objectIndex;
      FlagsLength = flagIndex;
    }

    public PackedTupleDescriptor(IList<Type> fieldTypes)
      : base(fieldTypes)
    {
      Initialize();
    }
  }
}