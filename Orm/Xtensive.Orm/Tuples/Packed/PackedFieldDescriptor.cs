// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    internal ushort StatePosition;

    [NonSerialized]
    internal byte AccessorIndex;

    public PackedFieldAccessor Accessor => PackedFieldAccessor.All[AccessorIndex];

    public bool IsObjectField => Accessor.Rank < 0;

    public int ObjectIndex => DataPosition;

    public int ValueIndex => DataPosition >> OffsetBitCount;
    public int ValueBitOffset => DataPosition & OffsetMask;

    public int StateIndex => StatePosition >> OffsetBitCount;
    public int StateBitOffset => StatePosition & OffsetMask;
  }
}