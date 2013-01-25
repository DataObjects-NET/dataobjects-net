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
    public readonly int ValuesLength;
    public readonly int ObjectsLength;

    public readonly PackedFieldDescriptor[] FieldDescriptors;

    public PackedTupleDescriptor(IList<Type> fieldTypes)
      : base(fieldTypes)
    {
      const int longBits = 64;
      const int stateBits = 2;
      const int statesPerLong = longBits / stateBits;

      var stateIndex = 0;
      var stateOffset = 0;

      var valueIndex = FieldCount / statesPerLong + Math.Min(1, FieldCount % statesPerLong);
      var objectIndex = 0;

      FieldDescriptors = new PackedFieldDescriptor[FieldCount];

      int fieldIndex = 0;
      foreach (var type in FieldTypes) {
        var descriptor = new PackedFieldDescriptor {FieldIndex = fieldIndex};
        FieldDescriptors[fieldIndex] = descriptor;

        PackedFieldAccessorFactory.ProvideAccessor(type, descriptor);
        switch (descriptor.PackingType) {
        case FieldPackingType.Object:
          descriptor.ValueIndex = objectIndex++;
          break;
        case FieldPackingType.Value:
          descriptor.ValueIndex = valueIndex++;
          break;
        default:
          throw new ArgumentOutOfRangeException("descriptor.PackType");
        }

        descriptor.StateIndex = stateIndex;
        descriptor.StateBitOffset = stateOffset;

        fieldIndex++;

        if (stateOffset==longBits - stateBits) {
          stateIndex++;
          stateOffset = 0;
        }
        else
          stateOffset += stateBits;
      }

      ValuesLength = valueIndex;
      ObjectsLength = objectIndex;
    }
  }
}