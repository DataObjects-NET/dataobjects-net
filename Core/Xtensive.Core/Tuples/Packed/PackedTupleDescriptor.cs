// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal sealed class PackedTupleDescriptor : TupleDescriptor
  {
    public readonly PackedFieldDescriptor[] FieldDescriptors;

    public readonly int ValuesLength;
    public readonly int ObjectsLength;

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      for (int i = 0; i < FieldCount; i++)
        PackedFieldAccessorFactory.ProvideAccessor(FieldTypes[i], FieldDescriptors[i]);
    }

    public PackedTupleDescriptor(IList<Type> fieldTypes)
      : base(fieldTypes)
    {
      const int longBits = 64;
      const int stateBits = 2;
      const int statesPerLong = longBits / stateBits;

      var objectIndex = 0;

      var valueIndex = FieldCount / statesPerLong + Math.Min(1, FieldCount % statesPerLong);
      var valueOffset = 0;

      var stateIndex = 0;
      var stateOffset = 0;

      FieldDescriptors = new PackedFieldDescriptor[FieldCount];
      for (int i = 0; i < FieldCount; i++) {
        var descriptor = new PackedFieldDescriptor {FieldIndex = i};
        PackedFieldAccessorFactory.ProvideAccessor(FieldTypes[i], descriptor);
        FieldDescriptors[i] = descriptor;
      }

      var orderedDescriptors = FieldDescriptors
        .OrderByDescending(d => d.ValueBitCount)
        .ThenBy(d => d.FieldIndex);

      foreach (var descriptor in orderedDescriptors) {
        switch (descriptor.PackingType) {
        case FieldPackingType.Object:
          descriptor.ValueIndex = objectIndex++;
          break;
        case FieldPackingType.Value:
          if (valueOffset + descriptor.ValueBitCount > longBits) {
            valueIndex++;
            valueOffset = 0;
          }
          descriptor.ValueIndex = valueIndex;
          descriptor.ValueBitOffset = valueOffset;
          valueOffset += descriptor.ValueBitCount;
          break;
        default:
          throw new ArgumentOutOfRangeException("descriptor.PackType");
        }

        if (stateOffset + stateBits > longBits) {
          stateIndex++;
          stateOffset = 0;
        }

        descriptor.StateIndex = stateIndex;
        descriptor.StateBitOffset = stateOffset;
        stateOffset += stateBits;
      }

      ValuesLength = valueIndex + Math.Min(1, valueOffset);
      ObjectsLength = objectIndex;
    }
  }
}