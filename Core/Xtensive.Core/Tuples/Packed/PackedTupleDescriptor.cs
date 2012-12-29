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
    public int BitsLength;
    public int ValuesLength;
    public int ObjectsLength;

    public PackedTupleFieldDescriptor[] FieldDescriptors;

    private void Initialize()
    {
      BitsLength = fieldTypes.Length * 2;

      var valueIndex = 0;
      var objectIndex = 0;

      FieldDescriptors = new PackedTupleFieldDescriptor[fieldTypes.Length];

      int fieldIndex = 0;
      foreach (var type in fieldTypes) {
        var descriptor = new PackedTupleFieldDescriptor();
        FieldDescriptors[fieldIndex++] = descriptor;
        if (PackedTupleAccessor.TryGetAccessors(type, descriptor)) {
          descriptor.ValueIndex = valueIndex++;
        }
        else {
          descriptor.IsObject = true;
          descriptor.ObjectIndex = objectIndex++;
        }
      }

      ValuesLength = valueIndex;
      ObjectsLength = objectIndex;
    }

    public PackedTupleDescriptor(IList<Type> fieldTypes)
      : base(fieldTypes)
    {
      Initialize();
    }
  }
}