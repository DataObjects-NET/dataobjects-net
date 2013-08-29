// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal sealed class PackedFieldDescriptor
  {
    public FieldPackingType PackingType;

    [NonSerialized]
    public PackedFieldAccessor Accessor;

    public int FieldIndex;

    public int ValueIndex;
    public int ValueBitOffset;
    public int ValueBitCount;

    public long ValueBitMask;

    public int StateIndex;
    public int StateBitOffset;
  }
}