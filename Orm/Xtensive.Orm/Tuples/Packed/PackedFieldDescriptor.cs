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
    private const int IndexBitCount = 20;
    private const int IndexMask = (1 << IndexBitCount) - 1;
    private const int OffsetBitCount = 7;
    private const int OffsetMask = ((1 << OffsetBitCount) - 1) << IndexBitCount;

    private const int MaskBitCount = (sizeof(int) * 8) - (IndexBitCount + OffsetBitCount);
    private const int GetValueMask = (1 << MaskBitCount) - 1;
    private const int SetValueMask = GetValueMask << (IndexBitCount + OffsetBitCount);

    private int data1;
    private int data2;

    [NonSerialized]
    public PackedFieldAccessor Accessor;

    public int ValueIndex
    {
      get => data1 & IndexMask;
      set => data1 = (data1 & ~IndexMask) | (value & IndexMask);
    }

    public int ValueBitOffset
    {
      get => (data1 & OffsetMask) >> IndexBitCount;
      set => data1 = (data1 & ~OffsetMask) | ((value << IndexBitCount) & OffsetMask);
    }

    public int Rank
    {
      get => (data1 >> (IndexBitCount + OffsetBitCount)) & GetValueMask;
      set => data1 = (data1 & ~SetValueMask) | ((value << (IndexBitCount + OffsetBitCount)) & SetValueMask);
    }

    public int ValueBitCount => 1 << Rank;

    public long ValueBitMask => (1L << (ValueBitCount - 1) << 1) - 1;

    public int StateIndex
    {
      get => data2 & IndexMask;
      set => data2 = (data2 & ~IndexMask) | (value & IndexMask);
    }

    public int StateBitOffset
    {
      get => (data2 & OffsetMask) >> IndexBitCount;
      set => data2 = (data2 & ~OffsetMask) | ((value << IndexBitCount) & OffsetMask);
    }

    public FieldPackingType PackingType
    {
      get => (FieldPackingType)((data2 >> (IndexBitCount + OffsetBitCount)) & GetValueMask);
      set => data2 = (data2 & ~SetValueMask) | (((int)value << (IndexBitCount + OffsetBitCount)) & SetValueMask);
    }
  }
}