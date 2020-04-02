// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Runtime.CompilerServices;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal struct PackedFieldDescriptor
  {
    private const int OffsetBitCount = 6;
    private const int OffsetMask = (1 << OffsetBitCount) - 1;

    private int indexField;
    private int stateField;

    [NonSerialized]
    public PackedFieldAccessor Accessor;

    public bool IsObjectField => Accessor.Rank < 0;

    public int ObjectIndex
    {
      get => indexField;
      set => indexField = value;
    }

    public int ValueIndex => indexField >> OffsetBitCount;
    public int ValueBitOffset => indexField & OffsetMask;

    public int StateIndex => stateField >> OffsetBitCount;
    public int StateBitOffset => stateField & OffsetMask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValueBitOffset(int totalBitOffset)
      => indexField = totalBitOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStateTotalBitOffset(int stateBitOffset)
      => stateField = stateBitOffset;
  }
}