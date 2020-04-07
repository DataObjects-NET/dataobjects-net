// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal struct PackedFieldDescriptor
  {
    private const int OffsetBitCount = 6;
    private const int OffsetMask = (1 << OffsetBitCount) - 1;

    internal int DataPosition;
    internal int StatePosition;

    [NonSerialized]
    public PackedFieldAccessor Accessor;

    public bool IsObjectField => Accessor.Rank < 0;

    public int ObjectIndex => DataPosition;

    public int ValueIndex => DataPosition >> OffsetBitCount;
    public int ValueBitOffset => DataPosition & OffsetMask;

    public int StateIndex => StatePosition >> OffsetBitCount;
    public int StateBitOffset => StatePosition & OffsetMask;
  }
}